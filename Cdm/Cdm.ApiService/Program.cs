using Cdm.ApiService.Endpoints;
using Cdm.Business.Abstraction.Services;
using Cdm.Business.Abstraction.Services.DnD5e;
using Cdm.Business.Common.Services;
using Cdm.Business.DnD5e.Services;
using Cdm.Common.Services;
using Cdm.Data.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Azure Key Vault : source de secrets centralisée.
// Si "KeyVault:Uri" est renseigné, tous les secrets du coffre sont ajoutés à la
// configuration (un secret nommé "AzureEmail--ConnectionString" alimente la clé
// "AzureEmail:ConnectionString", etc.). L'authentification passe par
// DefaultAzureCredential : identité managée en production, `az login` du
// développeur en local — aucun secret dans le code ni dans appsettings.
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    // On écarte les credentials qui posent problème sur les postes multi-tenants
    // (Visual Studio / PowerShell / azd connectés à d'autres comptes) et on garde
    // l'identité managée (production) + l'Azure CLI (développement local).
    var credentialOptions = new Azure.Identity.DefaultAzureCredentialOptions
    {
        ExcludeVisualStudioCredential = true,
        ExcludeAzurePowerShellCredential = true,
        ExcludeAzureDeveloperCliCredential = true,
        ExcludeInteractiveBrowserCredential = true,
    };

    // Force le tenant du coffre : indispensable quand la machine a plusieurs
    // comptes Azure, sinon le mauvais tenant est choisi.
    var keyVaultTenantId = builder.Configuration["KeyVault:TenantId"];
    if (!string.IsNullOrWhiteSpace(keyVaultTenantId))
    {
        credentialOptions.TenantId = keyVaultTenantId;
    }

    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new Azure.Identity.DefaultAzureCredential(credentialOptions));
}

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
    throw new InvalidOperationException("JWT secret key configuration ('Jwt:SecretKey') is missing. Please set a secure value via environment variable, User Secrets or Key Vault.");
}

// Fail-fast against an insecure / default key (audit fix #2, #8)
if (jwtSecretKey.StartsWith("CHANGE-THIS", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("JWT secret key is still the default placeholder. Set a real secret ('Jwt:SecretKey') before starting the API.");
}

if (jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException("JWT secret key must be at least 32 characters long to be secure.");
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
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

// Rate limiting (audit fix #3) — protects auth endpoints against brute-force / enumeration
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Strict fixed window for authentication endpoints
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // Global safety-net limiter, partitioned by client IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
            }));
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IAvatarService, AvatarService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();

// Image storage abstraction: local disk in dev/CI, Azure Blob in prod (selected by config).
if (string.Equals(builder.Configuration["ImageStorage:Provider"], "AzureBlob", StringComparison.OrdinalIgnoreCase))
{
    var blobServiceUri = builder.Configuration["ImageStorage:BlobServiceUri"]
        ?? throw new InvalidOperationException("ImageStorage:BlobServiceUri is required when ImageStorage:Provider=AzureBlob.");
    var containerName = builder.Configuration["ImageStorage:ContainerName"] ?? "images";

    // Managed identity in production (no connection string / account key).
    builder.Services.AddSingleton(_ =>
        new Azure.Storage.Blobs.BlobServiceClient(new Uri(blobServiceUri), new Azure.Identity.DefaultAzureCredential())
            .GetBlobContainerClient(containerName));
    builder.Services.AddScoped<Cdm.Common.Services.IImageStorage, Cdm.Common.Services.AzureBlobImageStorage>();
}
else
{
    builder.Services.AddScoped<Cdm.Common.Services.IImageStorage, Cdm.Common.Services.LocalImageStorage>();
}
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IWorldService, WorldService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IAchievementEvaluationService, AchievementEvaluationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ITradeService, TradeService>();
builder.Services.AddScoped<INpcService, NpcService>();
builder.Services.AddScoped<ICombatService, CombatService>();

// D&D 5e services
builder.Services.AddScoped<IDndReferenceService, DndReferenceService>();
builder.Services.AddScoped<IDndNpcService, DndNpcService>();
builder.Services.AddScoped<IDndCharacterService, DndCharacterService>();
// Service d'email : Azure Communication Services si configuré, sinon repli qui
// journalise (le parcours « mot de passe oublié » reste testable en local, le lien
// de réinitialisation apparaît alors dans les logs de l'API).
if (!string.IsNullOrWhiteSpace(builder.Configuration["AzureEmail:ConnectionString"]))
{
    builder.Services.AddScoped<IEmailService, AzureEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, LoggingEmailService>();
}

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

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Rate limiting (audit fix #3)
app.UseRateLimiter();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Security headers (OWASP Best Practices) — audit fix #11
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self'";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    await next();
});

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
app.MapCombatEndpoints();
app.MapDndEndpoints();
app.MapStatisticsEndpoints();

// Map SignalR hubs
app.MapHub<Cdm.ApiService.Hubs.SessionHub>("/hubs/session");
app.MapHub<Cdm.ApiService.Hubs.CombatHub>("/hubs/combat");
app.MapHub<Cdm.ApiService.Hubs.NotificationHub>("/hubs/notifications");

app.MapDefaultEndpoints();

app.Run();
