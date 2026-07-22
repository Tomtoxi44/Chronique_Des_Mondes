using Cdm.Web;
using Cdm.Web.Components;
using Cdm.Web.Extensions;
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

// Register scoped API clients (named HttpClient + instance) via AddApiClient<T> (audit #6).
builder.Services.AddApiClient<ProfileApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<IRoleService, RoleService>(ConfigureApiClient);
builder.Services.AddApiClient<ICampaignService, CampaignService>(ConfigureApiClient);
builder.Services.AddApiClient<ICharacterApiClient, CharacterApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<WorldApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<CampaignApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<ChapterApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<EventApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<AchievementApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<StatisticsApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<ImageApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<CodexApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<MarketplaceApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<LootApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<InventoryApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<ChapterImageApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<NotificationApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<SessionApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<NpcApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<DndApiClient>(ConfigureApiClient);
builder.Services.AddApiClient<CombatApiClient>(ConfigureApiClient);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Les images envoyées par les utilisateurs sont servies par le stockage blob, donc depuis
// une autre origine que le site. Sans l'ajouter à img-src, le navigateur les bloque : le
// blob répond bien 200 mais l'utilisateur ne voit qu'une icône d'image cassée. En local le
// problème n'apparaît pas (stockage disque servi par la même origine).
var imageOrigin = builder.Configuration["ImageStorage:BlobServiceUri"];
var imgSrc = "img-src 'self' data:";
if (!string.IsNullOrWhiteSpace(imageOrigin) && Uri.TryCreate(imageOrigin, UriKind.Absolute, out var imageUri))
{
    imgSrc += $" {imageUri.GetLeftPart(UriPartial.Authority)}";
}

// Content-Security-Policy tuned for Blazor Server + FluentUI + external fonts/icons.
// connect-src is restricted to same-origin (+ websockets) which limits token exfiltration
// even if a script is injected — the main mitigation for the token stored in localStorage.
// Follow-up: migrate the JWT to an HttpOnly cookie and drop 'unsafe-inline' from script-src via nonce/hash.
var contentSecurityPolicy =
    "default-src 'self'; " +
    "script-src 'self' 'unsafe-inline'; " +
    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
    "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
    imgSrc + "; " +
    "connect-src 'self' ws: wss:; " +
    "frame-ancestors 'none'; object-src 'none'; base-uri 'self'";

// Security headers (OWASP Best Practices) — audit fix #4 (XSS mitigation) & #11
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

    context.Response.Headers["Content-Security-Policy"] = contentSecurityPolicy;

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
