namespace Cdm.Business.Abstraction.DTOs.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a request to refresh a JWT access token.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}
