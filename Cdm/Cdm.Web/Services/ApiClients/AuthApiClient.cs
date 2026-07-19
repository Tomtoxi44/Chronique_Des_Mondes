using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.Storage;
using Cdm.Web.Shared.DTOs.Models;
using Cdm.Web.Shared.DTOs.ViewModels;

namespace Cdm.Web.Services.ApiClients;

public interface IAuthApiClient
{
    Task<RegisterResponse?> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> RefreshAsync(string refreshToken);

    /// <summary>Demande l'envoi d'un lien de réinitialisation de mot de passe.</summary>
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>Définit un nouveau mot de passe à partir d'un jeton reçu par email.</summary>
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>Confirme une adresse email à partir d'un jeton reçu par email.</summary>
    Task<bool> ConfirmEmailAsync(string token);

    /// <summary>
    /// Renvoie l'email de confirmation. Retourne le nombre de secondes à attendre
    /// avant un prochain envoi (0 = envoyé), ou -1 en cas d'erreur.
    /// </summary>
    Task<int> ResendConfirmationAsync();
}

public class AuthApiClient : BaseApiClient, IAuthApiClient
{
    public AuthApiClient(
        HttpClient httpClient,
        ILogger<AuthApiClient> logger,
        ILocalStorageService localStorage)
        : base(httpClient, logger, localStorage)
    {
    }
    
    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
    {
        this.logger.LogInformation("Attempting to register user: {Email}", request.Email);
        
        var response = await PostAsync<RegisterRequest, RegisterResponse>(
            "/api/auth/register", 
            request);
        
        if (response != null)
        {
            this.logger.LogInformation("User registered successfully: {Email}", response.Email);
        }
        
        return response;
    }
    
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        this.logger.LogInformation("Attempting to login user: {Email}", request.Email);
        
        var response = await PostAsync<LoginRequest, LoginResponse>(
            "/api/auth/login", 
            request);
        
        if (response != null)
        {
            this.logger.LogInformation("User logged in successfully: {Email}", response.Email);
        }
        
        return response;
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken)
    {
        this.logger.LogInformation("Attempting to refresh access token");

        var response = await PostAsync<object, LoginResponse>(
            "/api/auth/refresh",
            new { RefreshToken = refreshToken });

        if (response != null)
        {
            this.logger.LogInformation("Token refreshed successfully for user: {UserId}", response.UserId);
        }

        return response;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        this.logger.LogInformation("Requesting password reset for {Email}", request.Email);

        // L'API répond toujours 200 (anti-énumération de comptes) : une réponse
        // non nulle suffit à considérer la demande comme prise en compte.
        var response = await PostAsync<ForgotPasswordRequest, MessageResponse>(
            "/api/auth/forgot-password",
            request);

        return response != null;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        this.logger.LogInformation("Submitting new password from reset token");

        var response = await PostAsync<ResetPasswordRequest, MessageResponse>(
            "/api/auth/reset-password",
            request);

        return response != null;
    }

    public async Task<bool> ConfirmEmailAsync(string token)
    {
        this.logger.LogInformation("Confirming email from token");

        try
        {
            var response = await PostAsync<object, MessageResponse>(
                "/api/auth/confirm-email",
                new { Token = token });
            return response != null;
        }
        catch (ApiException)
        {
            return false;
        }
    }

    public async Task<int> ResendConfirmationAsync()
    {
        this.logger.LogInformation("Requesting confirmation email resend");

        try
        {
            var response = await PostAsync<object, ResendResponse>(
                "/api/auth/resend-confirmation",
                new { });
            return response?.RetryAfterSeconds ?? 0;
        }
        catch (ApiException)
        {
            return -1;
        }
    }
}

/// <summary>Réponse générique ne portant qu'un message.</summary>
public class MessageResponse
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>Réponse du renvoi d'email de confirmation.</summary>
public class ResendResponse
{
    /// <summary>Secondes à attendre avant un nouvel envoi (0 = envoyé).</summary>
    public int RetryAfterSeconds { get; set; }
}

