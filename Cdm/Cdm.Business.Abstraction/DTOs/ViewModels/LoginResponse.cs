namespace Cdm.Business.Abstraction.DTOs.ViewModels;

/// <summary>
/// Represents a successful user login response.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the user's identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's display nickname.
    /// </summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT authentication token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token for renewing the JWT.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the refresh token expires.
    /// </summary>
    public DateTime RefreshTokenExpiry { get; set; }

    /// <summary>
    /// Gets or sets the success message.
    /// </summary>
    public string Message { get; set; } = "Login successful";
}
