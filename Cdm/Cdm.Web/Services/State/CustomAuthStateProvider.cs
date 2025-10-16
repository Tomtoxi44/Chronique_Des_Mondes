using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Cdm.Web.Services.Storage;

namespace Cdm.Web.Services.State;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    
    private const string AuthTokenKey = "auth_token";
    private const string AuthUserIdKey = "auth_user_id";
    private const string AuthUserEmailKey = "auth_user_email";
    
    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        ILogger<CustomAuthStateProvider> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync(AuthTokenKey);
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogDebug("No authentication token found, user is anonymous");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        try
        {
            var userId = await _localStorage.GetItemAsync(AuthUserIdKey);
            var userEmail = await _localStorage.GetItemAsync(AuthUserEmailKey);
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Token found but user info incomplete");
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
            
            _logger.LogInformation("User authenticated: {Email}", userEmail);
            
            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing authentication token");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
    
    public async Task MarkUserAsAuthenticatedAsync(int userId, string email, string token)
    {
        await _localStorage.SetItemAsync(AuthTokenKey, token);
        await _localStorage.SetItemAsync(AuthUserIdKey, userId.ToString());
        await _localStorage.SetItemAsync(AuthUserEmailKey, email);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email)
        };
        
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        
        _logger.LogInformation("User marked as authenticated: {Email}", email);
    }
    
    public async Task MarkUserAsLoggedOutAsync()
    {
        await _localStorage.RemoveItemAsync(AuthTokenKey);
        await _localStorage.RemoveItemAsync(AuthUserIdKey);
        await _localStorage.RemoveItemAsync(AuthUserEmailKey);
        
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        
        _logger.LogInformation("User logged out");
    }
}
