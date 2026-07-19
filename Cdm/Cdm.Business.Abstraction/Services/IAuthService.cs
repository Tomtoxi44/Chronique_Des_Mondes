namespace Cdm.Business.Abstraction.Services;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
using Cdm.Common.Errors;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration request DTO</param>
    /// <param name="confirmUrlTemplate">
    /// Optionnel : gabarit d'URL de la page de confirmation d'email contenant <c>{token}</c>.
    /// Si fourni, un email de confirmation est envoyé au nouvel utilisateur.
    /// </param>
    /// <returns>Service result with registration response</returns>
    Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request, string? confirmUrlTemplate = null);

    /// <summary>
    /// Authenticate a user
    /// </summary>
    /// <param name="request">Login request DTO</param>
    /// <returns>Service result with login response</returns>
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// Refresh a JWT access token using a valid refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token string</param>
    /// <returns>Service result with a new login response containing fresh tokens</returns>
    Task<ServiceResult<LoginResponse>> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Demande une réinitialisation de mot de passe et envoie le lien par email.
    /// </summary>
    /// <param name="request">Adresse e-mail du compte.</param>
    /// <param name="resetUrlTemplate">
    /// Gabarit d'URL de la page de réinitialisation, contenant le marqueur <c>{token}</c>
    /// (ex. <c>https://mon-app/reset-password?token={token}</c>).
    /// </param>
    /// <returns>
    /// Toujours un succès si la demande est traitable, afin de ne pas révéler
    /// si l'adresse correspond à un compte existant (énumération de comptes).
    /// </returns>
    Task<ServiceResult<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request, string resetUrlTemplate);

    /// <summary>
    /// Définit un nouveau mot de passe à partir d'un jeton de réinitialisation valide.
    /// </summary>
    /// <param name="request">Jeton et nouveau mot de passe.</param>
    /// <returns>Succès si le jeton est valide et le mot de passe mis à jour.</returns>
    Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Confirme l'adresse email d'un compte à partir d'un jeton de confirmation.
    /// </summary>
    /// <param name="token">Le jeton reçu par email.</param>
    /// <returns>Succès si le jeton est valide et l'adresse confirmée.</returns>
    Task<ServiceResult<bool>> ConfirmEmailAsync(string token);

    /// <summary>
    /// Renvoie un email de confirmation à un utilisateur authentifié non confirmé,
    /// en respectant un délai minimal entre deux envois (cooldown).
    /// </summary>
    /// <param name="userId">L'utilisateur concerné (issu du jeton JWT).</param>
    /// <param name="confirmUrlTemplate">Gabarit d'URL contenant <c>{token}</c>.</param>
    /// <returns>
    /// <c>Data = 0</c> si l'email a été envoyé ; <c>Data = N &gt; 0</c> s'il faut encore
    /// attendre N secondes avant un nouvel envoi (cooldown).
    /// </returns>
    Task<ServiceResult<int>> ResendConfirmationEmailAsync(int userId, string confirmUrlTemplate);
}

/// <summary>
/// Standard service result wrapper
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public CdmErrorCode? ErrorCode { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }

    public static ServiceResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static ServiceResult<T> Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };

    public static ServiceResult<T> Failure(CdmErrorCode code) => new()
    {
        IsSuccess = false,
        ErrorCode = code,
        ErrorMessage = code.ToString()
    };

    public static ServiceResult<T> ValidationFailure(Dictionary<string, string[]> validationErrors) => new()
    {
        IsSuccess = false,
        ErrorCode = CdmErrorCode.ValidationFailed,
        ValidationErrors = validationErrors
    };
}
