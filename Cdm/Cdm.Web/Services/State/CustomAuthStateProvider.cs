using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Cdm.Web.Services.Storage;

namespace Cdm.Web.Services.State;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService localStorage;
    private readonly ILogger<CustomAuthStateProvider> logger;
    
    private const string AuthTokenKey = "auth_token";
    private const string AuthUserIdKey = "auth_user_id";
    private const string AuthUserEmailKey = "auth_user_email";
    
    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        ILogger<CustomAuthStateProvider> logger)
    {
        this.localStorage = localStorage;
        this.logger = logger;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await this.localStorage.GetItemAsync(AuthTokenKey);
        
        if (string.IsNullOrEmpty(token))
        {
            this.logger.LogDebug("No authentication token found, user is anonymous");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        try
        {
            var userId = await this.localStorage.GetItemAsync(AuthUserIdKey);
            var userEmail = await this.localStorage.GetItemAsync(AuthUserEmailKey);
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            {
                this.logger.LogWarning("Token found but user info incomplete");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(ClaimTypes.Name, userEmail)
            };
            
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            
            this.logger.LogInformation("User authenticated: {Email}", userEmail);
            
            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error parsing authentication token");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
    
    public async Task MarkUserAsAuthenticatedAsync(int userId, string email, string token)
    {
        await this.localStorage.SetItemAsync(AuthTokenKey, token);
        await this.localStorage.SetItemAsync(AuthUserIdKey, userId.ToString());
        await this.localStorage.SetItemAsync(AuthUserEmailKey, email);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email)
        };
        
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        
        this.logger.LogInformation("User marked as authenticated: {Email}", email);
    }
    
    public async Task MarkUserAsLoggedOutAsync()
    {
        await this.localStorage.RemoveItemAsync(AuthTokenKey);
        await this.localStorage.RemoveItemAsync(AuthUserIdKey);
        await this.localStorage.RemoveItemAsync(AuthUserEmailKey);
        
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        
        this.logger.LogInformation("User logged out");
    }
}

