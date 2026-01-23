namespace Cdm.Business.Abstraction.DTOs.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a user login request.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
