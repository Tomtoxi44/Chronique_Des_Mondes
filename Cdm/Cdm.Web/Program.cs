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
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthStateProvider>());

// Get API Base URL from configuration (supports Aspire in Dev, real URL in Production)
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https+http://apiservice";

// HTTP Clients - Using Aspire Service Discovery (Dev) or configured URL (Production)
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register ProfileApiClient with scoped lifetime
builder.Services.AddScoped<ProfileApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ProfileApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<ProfileApiClient>>();
    return new ProfileApiClient(httpClient, localStorage, logger);
});

builder.Services.AddHttpClient("ProfileApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register RoleService with scoped lifetime
builder.Services.AddScoped<IRoleService, RoleService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("RoleApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<RoleService>>();
    return new RoleService(httpClient, localStorage, logger);
});

builder.Services.AddHttpClient("RoleApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register CampaignService with scoped lifetime
builder.Services.AddScoped<ICampaignService, CampaignService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("CampaignApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CampaignService>>();
    return new CampaignService(httpClient, localStorage, logger);
});

builder.Services.AddHttpClient("CampaignApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register CharacterApiClient with scoped lifetime
builder.Services.AddScoped<ICharacterApiClient, CharacterApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("CharacterApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CharacterApiClient>>();
    return new CharacterApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("CharacterApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register WorldApiClient with scoped lifetime
builder.Services.AddScoped<WorldApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("WorldApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<WorldApiClient>>();
    return new WorldApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("WorldApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register CampaignApiClient with scoped lifetime
builder.Services.AddScoped<CampaignApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("CampaignApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CampaignApiClient>>();
    return new CampaignApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("CampaignApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register ChapterApiClient with scoped lifetime
builder.Services.AddScoped<ChapterApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ChapterApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<ChapterApiClient>>();
    return new ChapterApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("ChapterApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register EventApiClient with scoped lifetime
builder.Services.AddScoped<EventApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("EventApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<EventApiClient>>();
    return new EventApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("EventApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register AchievementApiClient with scoped lifetime
builder.Services.AddScoped<AchievementApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("AchievementApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<AchievementApiClient>>();
    return new AchievementApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("AchievementApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register NotificationApiClient with scoped lifetime
builder.Services.AddScoped<NotificationApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("NotificationApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<NotificationApiClient>>();
    return new NotificationApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("NotificationApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register SessionApiClient with scoped lifetime
builder.Services.AddScoped<SessionApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("SessionApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<SessionApiClient>>();
    return new SessionApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("SessionApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register NpcApiClient with scoped lifetime
builder.Services.AddScoped<NpcApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("NpcApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<NpcApiClient>>();
    return new NpcApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("NpcApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register DndApiClient with scoped lifetime
builder.Services.AddScoped<DndApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("DndApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<DndApiClient>>();
    return new DndApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("DndApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register CombatApiClient with scoped lifetime
builder.Services.AddScoped<CombatApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("CombatApiClient");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var logger = sp.GetRequiredService<ILogger<CombatApiClient>>();
    return new CombatApiClient(httpClient, logger, localStorage);
});

builder.Services.AddHttpClient("CombatApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
