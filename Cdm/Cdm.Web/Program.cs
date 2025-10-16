using Cdm.Web;
using Cdm.Web.Components;
using Cdm.Web.Components.Pages.Auth;
using Cdm.Web.Services.Storage;
using Cdm.Web.Services.State;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

// Authentication & Authorization
// We need AddAuthentication for the middleware, but we disable automatic redirects
// The CustomAuthStateProvider handles authentication state based on localStorage JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    // Disable automatic redirects - let Blazor handle navigation
    options.LoginPath = null;
    options.LogoutPath = null;
    options.AccessDeniedPath = null;
    options.Events.OnRedirectToLogin = context =>
    {
        // Don't redirect, just return 401
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        // Don't redirect, just return 403
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthStateProvider>());

// Handlers - Component-Handler-Service pattern
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<RegisterHandler>();

// HTTP Clients - Using Aspire Service Discovery
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    // Aspire Service Discovery will resolve "https+http://apiservice" automatically
    client.BaseAddress = new Uri("https+http://apiservice");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

// Enable authentication and authorization middleware
// Authentication is handled by CustomAuthStateProvider (client-side JWT)
// but middleware is required for UseAuthorization to work
app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
