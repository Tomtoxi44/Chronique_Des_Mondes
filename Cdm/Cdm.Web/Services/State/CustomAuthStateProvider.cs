using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.Storage;

namespace Cdm.Web.Services.State;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService localStorage;
    private readonly ILogger<CustomAuthStateProvider> logger;
    private readonly IAuthApiClient authClient;
    
    private const string AuthTokenKey = "auth_token";
    private const string AuthUserIdKey = "auth_user_id";
    private const string AuthUserEmailKey = "auth_user_email";
    private const string AuthUserNicknameKey = "auth_user_nickname";
    private const string AuthRefreshTokenKey = "auth_refresh_token";
    private const string AuthRefreshTokenExpiryKey = "auth_refresh_token_expiry";
    
    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        ILogger<CustomAuthStateProvider> logger,
        IAuthApiClient authClient)
    {
        this.localStorage = localStorage;
        this.logger = logger;
        this.authClient = authClient;
    }

    public void SetAuthClient(IAuthApiClient client)
    {
        // kept for backward compat — constructor injection is preferred
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await this.localStorage.GetItemAsync(AuthTokenKey);
        
        if (string.IsNullOrEmpty(token))
        {
            this.logger.LogDebug("No authentication token found, user is anonymous");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        // Check if JWT is expired and attempt refresh
        if (IsJwtExpired(token))
        {
            this.logger.LogInformation("JWT is expired, attempting refresh");
            var refreshed = await TryRefreshTokenAsync();
            if (!refreshed)
            {
                await this.ClearStorageAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            token = await this.localStorage.GetItemAsync(AuthTokenKey) ?? string.Empty;
        }
        
        try
        {
            var userId = await this.localStorage.GetItemAsync(AuthUserIdKey);
            var userEmail = await this.localStorage.GetItemAsync(AuthUserEmailKey);
            var userNickname = await this.localStorage.GetItemAsync(AuthUserNicknameKey);
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            {
                this.logger.LogWarning("Token found but user info incomplete");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(ClaimTypes.Name, userNickname ?? userEmail),
                new Claim("nickname", userNickname ?? userEmail)
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
    
    public async Task MarkUserAsAuthenticatedAsync(
        int userId, string email, string nickname, string token,
        string? refreshToken = null, DateTime? refreshTokenExpiry = null)
    {
        await this.localStorage.SetItemAsync(AuthTokenKey, token);
        await this.localStorage.SetItemAsync(AuthUserIdKey, userId.ToString());
        await this.localStorage.SetItemAsync(AuthUserEmailKey, email);
        await this.localStorage.SetItemAsync(AuthUserNicknameKey, nickname);

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await this.localStorage.SetItemAsync(AuthRefreshTokenKey, refreshToken);
            await this.localStorage.SetItemAsync(AuthRefreshTokenExpiryKey, 
                (refreshTokenExpiry ?? DateTime.UtcNow.AddDays(7)).ToString("O"));
        }
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, nickname ?? email),
            new Claim("nickname", nickname ?? email)
        };
        
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        
        this.logger.LogInformation("User marked as authenticated: {Email}", email);
    }
    
    public async Task MarkUserAsLoggedOutAsync()
    {
        await this.ClearStorageAsync();
        
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        
        this.logger.LogInformation("User logged out");
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            var refreshToken = await this.localStorage.GetItemAsync(AuthRefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var response = await this.authClient.RefreshAsync(refreshToken);
            if (response == null || string.IsNullOrEmpty(response.Token)) return false;

            await this.MarkUserAsAuthenticatedAsync(
                response.UserId, response.Email, response.Nickname, response.Token,
                response.RefreshToken, response.RefreshTokenExpiry);

            this.logger.LogInformation("Token refreshed successfully for user {UserId}", response.UserId);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Token refresh failed");
            return false;
        }
    }

    private async Task ClearStorageAsync()
    {
        await this.localStorage.RemoveItemAsync(AuthTokenKey);
        await this.localStorage.RemoveItemAsync(AuthUserIdKey);
        await this.localStorage.RemoveItemAsync(AuthUserEmailKey);
        await this.localStorage.RemoveItemAsync(AuthUserNicknameKey);
        await this.localStorage.RemoveItemAsync(AuthRefreshTokenKey);
        await this.localStorage.RemoveItemAsync(AuthRefreshTokenExpiryKey);
    }

    private static bool IsJwtExpired(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return true;

            var payload = parts[1];
            // Pad base64url to valid base64
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(jsonBytes);
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("exp", out var expElement))
            {
                var exp = expElement.GetInt64();
                var expiry = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return expiry <= DateTime.UtcNow.AddSeconds(30); // 30s buffer
            }

            return true;
        }
        catch
        {
            return true;
        }
    }
}

