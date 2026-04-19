using Cdm.ApiService.Endpoints;
using Cdm.Business.Abstraction.Services;
using Cdm.Business.Common.Services;
using Cdm.Common.Services;
using Cdm.Data.Common;
using Cdm.Migrations;
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

// Register MigrationsContext on the same connection to run migrations at startup
builder.Services.AddDbContext<MigrationsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Apply pending migrations automatically on startup (Production)
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var migrationsContext = scope.ServiceProvider.GetRequiredService<MigrationsContext>();
    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying pending database migrations...");
        await migrationsContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }

    // Safety net: ensure critical tables exist even if migration history is out of sync
    try
    {
        logger.LogInformation("Ensuring critical schema objects exist...");
        await appDbContext.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH('[dbo].[Characters]', 'IsLocked') IS NULL
    ALTER TABLE [dbo].[Characters] ADD [IsLocked] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[dbo].[Sessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Sessions] (
        [Id] int NOT NULL IDENTITY,
        [CampaignId] int NOT NULL,
        [GmUserId] int NOT NULL,
        [Status] int NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [EndedAt] datetime2 NULL,
        CONSTRAINT [PK_Sessions] PRIMARY KEY ([Id])
    );
    ALTER TABLE [dbo].[Sessions]
        ADD CONSTRAINT [FK_Sessions_Campaigns_CampaignId]
        FOREIGN KEY ([CampaignId]) REFERENCES [dbo].[Campaigns] ([Id]) ON DELETE CASCADE;
    ALTER TABLE [dbo].[Sessions]
        ADD CONSTRAINT [FK_Sessions_Users_GmUserId]
        FOREIGN KEY ([GmUserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
    CREATE INDEX [IX_Sessions_CampaignId] ON [dbo].[Sessions] ([CampaignId]);
    CREATE INDEX [IX_Sessions_GmUserId] ON [dbo].[Sessions] ([GmUserId]);
END

IF OBJECT_ID(N'[dbo].[SessionParticipants]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SessionParticipants] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [UserId] int NOT NULL,
        [CharacterId] int NULL,
        [JoinedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SessionParticipants] PRIMARY KEY ([Id])
    );
    ALTER TABLE [dbo].[SessionParticipants]
        ADD CONSTRAINT [FK_SessionParticipants_Sessions_SessionId]
        FOREIGN KEY ([SessionId]) REFERENCES [dbo].[Sessions] ([Id]) ON DELETE CASCADE;
    ALTER TABLE [dbo].[SessionParticipants]
        ADD CONSTRAINT [FK_SessionParticipants_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]);
    ALTER TABLE [dbo].[SessionParticipants]
        ADD CONSTRAINT [FK_SessionParticipants_Characters_CharacterId]
        FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Characters] ([Id]);
    CREATE INDEX [IX_SessionParticipants_SessionId] ON [dbo].[SessionParticipants] ([SessionId]);
    CREATE INDEX [IX_SessionParticipants_UserId] ON [dbo].[SessionParticipants] ([UserId]);
    CREATE INDEX [IX_SessionParticipants_CharacterId] ON [dbo].[SessionParticipants] ([CharacterId]);
END
");
        logger.LogInformation("Critical schema objects verified successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while ensuring critical schema objects.");
        throw;
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
