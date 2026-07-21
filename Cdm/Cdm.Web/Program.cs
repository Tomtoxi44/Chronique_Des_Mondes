using Cdm.Web;
using Cdm.Web.Components;
using Cdm.Web.Services;
using Cdm.Web.Services.Storage;
using Cdm.Web.Services.State;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Fluent UI Blazor
builder.Services.AddFluentUIComponents();

// Localization — co-located .resx files + shared AppStrings
builder.Services.AddLocalization(options => options.ResourcesPath = "");

// Theme service — centralized game-type-based theming
builder.Services.AddScoped<ThemeService>();

// Navigation context — communicates deep nav context between pages and MainLayout
builder.Services.AddScoped<NavigationContextService>();

// Toast notification service
builder.Services.AddScoped<Cdm.Web.Services.ToastService>();

builder.Services.AddOutputCache();

// Authentication & Authorization
// CustomAuthStateProvider handles auth state based on localStorage JWT
// BlazorAuthorizationMiddlewareResultHandler prevents HTTP 401 so Blazor can render
builder.Services.AddAuthentication();
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, BlazorAuthorizationMiddlewareResultHandler>();
builder.Services.AddCascadingAuthenticationState();
// Auth tokens stored in localStorage are encrypted at rest with Data Protection so a XSS payload
// cannot steal a usable token. ILocalStorageService is decorated transparently for callers.
builder.Services.AddDataProtection();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ILocalStorageService>(sp => new ProtectedLocalStorageService(
    sp.GetRequiredService<LocalStorageService>(),
    sp.GetRequiredService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>(),
    sp.GetRequiredService<ILogger<ProtectedLocalStorageService>>()));
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthStateProvider>());

// Get API Base URL from configuration (supports Aspire in Dev, real URL in Production)
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https+http://apiservice";

// Shared HTTP client configuration for all API clients (audit fix #10 — removes 15x duplication)
Action<HttpClient> ConfigureApiClient = client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
};

// HTTP Clients - Using Aspire Service Discovery (Dev) or configured URL (Production)
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(ConfigureApiClient);

// Register ProfileApiClient with scoped lifetime
builder.Services.AddScoped<ProfileApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ProfileApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<ProfileApiClient>>();
    return new ProfileApiClient(httpClient, localStorage, logger);
});

builder.Services.AddHttpClient("ProfileApiClient", ConfigureApiClient);

// Register RoleService with scoped lifetime
builder.Services.AddScoped<IRoleService, RoleService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("RoleApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<RoleService>>();
    return new RoleService(httpClient, localStorage, logger);
});

builder.Services.AddHttpClient("RoleApiClient", ConfigureApiClient);

// Register CampaignService with scoped lifetime
builder.Services.AddScoped<ICampaignService, CampaignService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("CampaignApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CampaignService>>();
    return new CampaignService(httpClient, localStorage, logger);
});

builder.Services.AddHttpClient("CampaignApiClient", ConfigureApiClient);

// Register CharacterApiClient with scoped lifetime
builder.Services.AddScoped<ICharacterApiClient, CharacterApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("CharacterApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CharacterApiClient>>();
    return new CharacterApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("CharacterApiClient", ConfigureApiClient);

// Register WorldApiClient with scoped lifetime
builder.Services.AddScoped<WorldApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("WorldApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<WorldApiClient>>();
    return new WorldApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("WorldApiClient", ConfigureApiClient);

// Register CampaignApiClient with scoped lifetime
builder.Services.AddScoped<CampaignApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("CampaignApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CampaignApiClient>>();
    return new CampaignApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("CampaignApiClient", ConfigureApiClient);

// Register ChapterApiClient with scoped lifetime
builder.Services.AddScoped<ChapterApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ChapterApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<ChapterApiClient>>();
    return new ChapterApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("ChapterApiClient", ConfigureApiClient);

// Register EventApiClient with scoped lifetime
builder.Services.AddScoped<EventApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("EventApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<EventApiClient>>();
    return new EventApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("EventApiClient", ConfigureApiClient);

// Register AchievementApiClient with scoped lifetime
builder.Services.AddScoped<AchievementApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("AchievementApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<AchievementApiClient>>();
    return new AchievementApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("AchievementApiClient", ConfigureApiClient);

// Register StatisticsApiClient with scoped lifetime
builder.Services.AddScoped<StatisticsApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("StatisticsApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<StatisticsApiClient>>();
    return new StatisticsApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("StatisticsApiClient", ConfigureApiClient);

// Register ImageApiClient with scoped lifetime
builder.Services.AddScoped<ImageApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ImageApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<ImageApiClient>>();
    return new ImageApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("ImageApiClient", ConfigureApiClient);

// Register CodexApiClient with scoped lifetime
builder.Services.AddScoped<CodexApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("CodexApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CodexApiClient>>();
    return new CodexApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("CodexApiClient", ConfigureApiClient);

// Register MarketplaceApiClient with scoped lifetime
builder.Services.AddScoped<MarketplaceApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("MarketplaceApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<MarketplaceApiClient>>();
    return new MarketplaceApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("MarketplaceApiClient", ConfigureApiClient);

// Register LootApiClient with scoped lifetime
builder.Services.AddScoped<LootApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("LootApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<LootApiClient>>();
    return new LootApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("LootApiClient", ConfigureApiClient);

// Register InventoryApiClient with scoped lifetime
builder.Services.AddScoped<InventoryApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("InventoryApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<InventoryApiClient>>();
    return new InventoryApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("InventoryApiClient", ConfigureApiClient);

// Register NotificationApiClient with scoped lifetime
builder.Services.AddScoped<NotificationApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("NotificationApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<NotificationApiClient>>();
    return new NotificationApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("NotificationApiClient", ConfigureApiClient);

// Register SessionApiClient with scoped lifetime
builder.Services.AddScoped<SessionApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("SessionApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<SessionApiClient>>();
    return new SessionApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("SessionApiClient", ConfigureApiClient);

// Register NpcApiClient with scoped lifetime
builder.Services.AddScoped<NpcApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("NpcApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<NpcApiClient>>();
    return new NpcApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("NpcApiClient", ConfigureApiClient);

// Register DndApiClient with scoped lifetime
builder.Services.AddScoped<DndApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("DndApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<DndApiClient>>();
    return new DndApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("DndApiClient", ConfigureApiClient);

// Register CombatApiClient with scoped lifetime
builder.Services.AddScoped<CombatApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("CombatApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CombatApiClient>>();
    return new CombatApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("CombatApiClient", ConfigureApiClient);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security headers (OWASP Best Practices) — audit fix #4 (XSS mitigation) & #11
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

    // Content-Security-Policy tuned for Blazor Server + FluentUI + external fonts/icons.
    // connect-src is restricted to same-origin (+ websockets) which limits token exfiltration
    // even if a script is injected — the main mitigation for the token stored in localStorage.
    // Follow-up: migrate the JWT to an HttpOnly cookie and drop 'unsafe-inline' from script-src via nonce/hash.
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "connect-src 'self' ws: wss:; " +
        "frame-ancestors 'none'; object-src 'none'; base-uri 'self'";

    await next();
});

// Localization — French default
var supportedCultures = new[] { new CultureInfo("fr"), new CultureInfo("en") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("fr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapStaticAssets();

// Bascule de langue : pose le cookie de culture standard d'ASP.NET Core.
// (L'ancienne implémentation écrivait un cookie "Culture=fr" côté JS, que
// CookieRequestCultureProvider ne lit pas — la langue ne changeait donc jamais.)
app.MapGet("/set-culture", (string culture, string? redirectUri, HttpContext context) =>
{
    if (!string.IsNullOrWhiteSpace(culture) &&
        supportedCultures.Any(c => c.Name.Equals(culture, StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.Cookies.Append(
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(
                new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
            new CookieOptions
            {
                Path = "/",
                MaxAge = TimeSpan.FromDays(365),
                IsEssential = true,
            });
    }

    // On n'accepte qu'un chemin relatif simple. `LocalRedirect` lèverait une exception
    // (donc une 500) sur une URL externe : on retombe silencieusement sur /settings.
    var isSafeRedirect =
        !string.IsNullOrWhiteSpace(redirectUri) &&
        redirectUri.StartsWith('/') &&
        !redirectUri.StartsWith("//", StringComparison.Ordinal) &&
        !redirectUri.Contains(':', StringComparison.Ordinal);

    return Results.LocalRedirect(isSafeRedirect ? redirectUri! : "/settings");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
