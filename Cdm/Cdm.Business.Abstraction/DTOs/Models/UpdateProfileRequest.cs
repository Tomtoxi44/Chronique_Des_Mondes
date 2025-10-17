namespace Cdm.Business.Abstraction.DTOs.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request DTO for updating user profile
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// User unique username (3-30 characters, optional)
    /// </summary>
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters")]
    public string? Username { get; set; }

    /// <summary>
    /// User preferences as JSON string
    /// </summary>
    public string? Preferences { get; set; }
}
