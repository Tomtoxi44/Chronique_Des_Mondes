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

    /// <summary>
    /// Sérialise le rafraîchissement du jeton. Blazor appelle
    /// <see cref="GetAuthenticationStateAsync"/> depuis chaque <c>AuthorizeView</c> et chaque
    /// page : sans ce verrou, un JWT expiré déclenchait autant de rafraîchissements simultanés
    /// que de composants. Or les refresh tokens sont à usage unique côté serveur (l'appel
    /// révoque le jeton présenté), donc le premier réussissait et **tous les autres
    /// échouaient**, ce qui vidait le stockage et déconnectait l'utilisateur — le tout en
    /// saturant au passage la limite de débit de /api/auth/refresh (10 requêtes/minute).
    /// Le provider étant Scoped, l'instance vaut pour un circuit, soit un utilisateur.
    /// </summary>
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    /// <summary>
    /// Dernier état d'authentification calculé, indexé par le jeton dont il est issu.
    /// Reconstruire l'état coûte cinq lectures de localStorage (aller-retour JS + déchiffrement
    /// Data Protection) : sans ce cache, chaque rendu d'un composant protégé les repayait.
    /// </summary>
    private string? cachedToken;
    private AuthenticationState? cachedState;

    private const string AuthTokenKey = "auth_token";
    private const string AuthUserIdKey = "auth_user_id";
    private const string AuthUserEmailKey = "auth_user_email";
    private const string AuthUserNicknameKey = "auth_user_nickname";
    private const string AuthRefreshTokenKey = "auth_refresh_token";
    private const string AuthRefreshTokenExpiryKey = "auth_refresh_token_expiry";
    private const string AuthEmailConfirmedKey = "auth_email_confirmed";
    
    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        ILogger<CustomAuthStateProvider> logger,
        IAuthApiClient authClient)
    {
        this.localStorage = localStorage;
        this.logger = logger;
        this.authClient = authClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await this.localStorage.GetItemAsync(AuthTokenKey);

        if (string.IsNullOrEmpty(token))
        {
            this.ClearCache();
            this.logger.LogDebug("No authentication token found, user is anonymous");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Check if JWT is expired and attempt refresh
        if (IsJwtExpired(token))
        {
            this.logger.LogDebug("JWT is expired, attempting refresh");
            var refreshed = await TryRefreshTokenAsync();
            if (!refreshed)
            {
                // La purge de la session est faite par TryRefreshTokenAsync, sous le verrou.
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            token = await this.localStorage.GetItemAsync(AuthTokenKey) ?? string.Empty;
        }

        // Le jeton est valide : si l'état a déjà été construit pour ce jeton exact, on le
        // réutilise au lieu de relire (et déchiffrer) quatre entrées supplémentaires.
        if (this.cachedState is not null && this.cachedToken == token)
        {
            return this.cachedState;
        }

        try
        {
            var userId = await this.localStorage.GetItemAsync(AuthUserIdKey);
            var userEmail = await this.localStorage.GetItemAsync(AuthUserEmailKey);
            var userNickname = await this.localStorage.GetItemAsync(AuthUserNicknameKey);
            var emailConfirmed = await this.localStorage.GetItemAsync(AuthEmailConfirmedKey);

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
                new Claim("nickname", userNickname ?? userEmail),
                new Claim("email_confirmed", emailConfirmed ?? "true")
            };
            
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            this.logger.LogDebug("User authenticated: {Email}", userEmail);

            var state = new AuthenticationState(user);
            this.cachedToken = token;
            this.cachedState = state;
            return state;
        }
        catch (Exception ex)
        {
            this.ClearCache();
            this.logger.LogError(ex, "Error parsing authentication token");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
    
    public async Task MarkUserAsAuthenticatedAsync(
        int userId, string email, string nickname, string token,
        string? refreshToken = null, DateTime? refreshTokenExpiry = null,
        bool emailConfirmed = true)
    {
        await this.localStorage.SetItemAsync(AuthTokenKey, token);
        await this.localStorage.SetItemAsync(AuthUserIdKey, userId.ToString());
        await this.localStorage.SetItemAsync(AuthUserEmailKey, email);
        await this.localStorage.SetItemAsync(AuthUserNicknameKey, nickname);
        await this.localStorage.SetItemAsync(AuthEmailConfirmedKey, emailConfirmed ? "true" : "false");

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
            new Claim("nickname", nickname ?? email),
            new Claim("email_confirmed", emailConfirmed ? "true" : "false")
        };

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        // L'état vient d'être construit ici : on le met en cache pour ce jeton plutôt que de
        // le laisser être recalculé (et relu depuis le stockage) au prochain rendu.
        var state = new AuthenticationState(user);
        this.cachedToken = token;
        this.cachedState = state;

        NotifyAuthenticationStateChanged(Task.FromResult(state));

        this.logger.LogInformation("User marked as authenticated: {Email}", email);
    }

    /// <summary>
    /// Met à jour l'état « email confirmé » sans reconnexion (après validation réussie),
    /// pour faire disparaître le bandeau immédiatement.
    /// </summary>
    public async Task MarkEmailConfirmedAsync()
    {
        await this.localStorage.SetItemAsync(AuthEmailConfirmedKey, "true");
        // Le jeton ne change pas ici, seulement une revendication : il faut donc invalider
        // explicitement le cache, sinon l'état reconstruit porterait encore
        // email_confirmed=false et le bandeau resterait affiché.
        this.ClearCache();
        NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
    }
    
    public async Task MarkUserAsLoggedOutAsync()
    {
        await this.ClearStorageAsync();
        
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        
        this.logger.LogInformation("User logged out");
    }

    /// <summary>
    /// Rafraîchit le jeton d'accès, au plus une fois à la fois par circuit. Les appelants
    /// concurrents attendent le rafraîchissement en cours puis réutilisent son résultat au lieu
    /// de rejouer l'appel avec un refresh token désormais révoqué.
    /// </summary>
    private async Task<bool> TryRefreshTokenAsync()
    {
        await this.refreshLock.WaitAsync();
        try
        {
            // Double vérification : pendant l'attente du verrou, un autre appelant a pu
            // rafraîchir. C'est ce test qui transforme N rafraîchissements simultanés en un
            // seul — sans lui, les suivants présenteraient un jeton déjà révoqué et
            // déconnecteraient l'utilisateur.
            var current = await this.localStorage.GetItemAsync(AuthTokenKey);
            if (!string.IsNullOrEmpty(current) && !IsJwtExpired(current))
            {
                this.logger.LogDebug("Token already refreshed by a concurrent caller");
                return true;
            }

            var refreshToken = await this.localStorage.GetItemAsync(AuthRefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var response = await this.authClient.RefreshAsync(refreshToken);
            if (response == null || string.IsNullOrEmpty(response.Token))
            {
                // Le refresh token est mort (expiré, révoqué, session close). On purge la
                // session ici, sous le verrou : les appelants encore en file d'attente
                // trouveront un stockage vide et renonceront au lieu de rejouer le même
                // jeton condamné — un seul aller-retour réseau au lieu d'un par composant.
                await this.ClearStorageAsync();
                return false;
            }

            await this.MarkUserAsAuthenticatedAsync(
                response.UserId, response.Email, response.Nickname, response.Token,
                response.RefreshToken, response.RefreshTokenExpiry, response.EmailConfirmed);

            this.logger.LogInformation("Token refreshed successfully for user {UserId}", response.UserId);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Token refresh failed");
            await this.ClearStorageAsync();
            return false;
        }
        finally
        {
            this.refreshLock.Release();
        }
    }

    private void ClearCache()
    {
        this.cachedToken = null;
        this.cachedState = null;
    }

    private async Task ClearStorageAsync()
    {
        this.ClearCache();
        await this.localStorage.RemoveItemAsync(AuthTokenKey);
        await this.localStorage.RemoveItemAsync(AuthUserIdKey);
        await this.localStorage.RemoveItemAsync(AuthUserEmailKey);
        await this.localStorage.RemoveItemAsync(AuthUserNicknameKey);
        await this.localStorage.RemoveItemAsync(AuthRefreshTokenKey);
        await this.localStorage.RemoveItemAsync(AuthRefreshTokenExpiryKey);
        await this.localStorage.RemoveItemAsync(AuthEmailConfirmedKey);
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

