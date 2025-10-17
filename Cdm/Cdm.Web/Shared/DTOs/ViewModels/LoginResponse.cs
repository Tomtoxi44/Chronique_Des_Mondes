namespace Cdm.Web.Shared.DTOs.ViewModels;

/// <summary>
/// Represents a successful login response (Web layer).
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
    /// Gets or sets the success message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
