using Cdm.ApiService.Endpoints;
using Cdm.Business.Abstraction.Services;
using Cdm.Business.Common.Services;
using Cdm.Common.Services;
using Cdm.Data.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure Database
builder.AddSqlServerDbContext<AppDbContext>("DefaultConnection");

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecretKey))
{
    throw new InvalidOperationException("JWT secret key configuration ('Jwt:SecretKey') is missing. Please set a secure value in your configuration.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ChroniqueDesMondes";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ChroniqueDesMondesWeb";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Configure JWT for SignalR (allow token from query string)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our SignalR hubs
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/session") ||
                 path.StartsWithSegments("/hubs/combat") ||
                 path.StartsWithSegments("/hubs/notifications")))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IAvatarService, AvatarService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IWorldService, WorldService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<INpcService, NpcService>();
// Email service is optional for MVP
// builder.Services.AddScoped<IEmailService, AzureEmailService>();

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 102400; // 100 KB
    options.StreamBufferCapacity = 10;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Apply schema safety net on startup (Production)
// Ensures Sessions/SessionParticipants tables exist with the correct schema.
// Also repairs if a previous bad deployment created them with wrong column names.
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Ensuring correct database schema...");
        await appDbContext.Database.ExecuteSqlRawAsync(@"
-- Repair: if Sessions was created with wrong schema (GmUserId column from bad hotfix), drop and recreate
IF COL_LENGTH('[dbo].[Sessions]', 'GmUserId') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'[dbo].[SessionParticipants]', N'U') IS NOT NULL
        DROP TABLE [dbo].[SessionParticipants];
    DROP TABLE [dbo].[Sessions];
END

-- Ensure IsLocked column on Characters
IF COL_LENGTH('[dbo].[Characters]', 'IsLocked') IS NULL
BEGIN
    ALTER TABLE [dbo].[Characters] ADD [IsLocked] bit NOT NULL DEFAULT 0;
    CREATE INDEX [IX_Characters_IsLocked] ON [dbo].[Characters] ([IsLocked]);
END

-- Ensure Sessions table with correct schema (StartedById, not GmUserId)
IF OBJECT_ID(N'[dbo].[Sessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sessions] (
        [Id] int NOT NULL IDENTITY,
        [CampaignId] int NOT NULL,
        [StartedById] int NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [EndedAt] datetime2 NULL,
        [Status] int NOT NULL DEFAULT 1,
        [CurrentChapterId] int NULL,
        [WelcomeMessage] nvarchar(max) NULL,
        CONSTRAINT [PK_Sessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sessions_Campaigns_CampaignId]
            FOREIGN KEY ([CampaignId]) REFERENCES [dbo].[Campaigns] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Sessions_Users_StartedById]
            FOREIGN KEY ([StartedById]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sessions_Chapters_CurrentChapterId]
            FOREIGN KEY ([CurrentChapterId]) REFERENCES [dbo].[Chapters] ([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_Sessions_CampaignId] ON [dbo].[Sessions] ([CampaignId]);
    CREATE INDEX [IX_Sessions_StartedById] ON [dbo].[Sessions] ([StartedById]);
    CREATE INDEX [IX_Sessions_Status] ON [dbo].[Sessions] ([Status]);
    CREATE INDEX [IX_Sessions_CurrentChapterId] ON [dbo].[Sessions] ([CurrentChapterId]);
END

-- Ensure SessionParticipants table with correct schema (WorldCharacterId, not UserId/CharacterId)
IF OBJECT_ID(N'[dbo].[SessionParticipants]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SessionParticipants] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [WorldCharacterId] int NOT NULL,
        [JoinedAt] datetime2 NOT NULL,
        [Status] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_SessionParticipants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SessionParticipants_Sessions_SessionId]
            FOREIGN KEY ([SessionId]) REFERENCES [dbo].[Sessions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SessionParticipants_WorldCharacters_WorldCharacterId]
            FOREIGN KEY ([WorldCharacterId]) REFERENCES [dbo].[WorldCharacters] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_SessionParticipants_SessionId] ON [dbo].[SessionParticipants] ([SessionId]);
    CREATE INDEX [IX_SessionParticipants_WorldCharacterId] ON [dbo].[SessionParticipants] ([WorldCharacterId]);
END
");
        logger.LogInformation("Database schema verified successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while ensuring database schema.");
        // Do not throw — allow app to start even if safety net fails
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthEndpoints();
app.MapProfileEndpoints();
app.MapRoleEndpoints();
app.MapCampaignEndpoints();
app.MapCharacterEndpoints();
app.MapWorldEndpoints();
app.MapChapterEndpoints();
app.MapEventEndpoints();
app.MapAchievementEndpoints();
app.MapNotificationEndpoints();
app.MapSessionEndpoints();
app.MapNpcEndpoints();

// Map SignalR hubs
app.MapHub<Cdm.ApiService.Hubs.SessionHub>("/hubs/session");
app.MapHub<Cdm.ApiService.Hubs.CombatHub>("/hubs/combat");
app.MapHub<Cdm.ApiService.Hubs.NotificationHub>("/hubs/notifications");

app.MapDefaultEndpoints();

app.Run();
