namespace Cdm.Web.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

/// <summary>
/// Request model for updating user profile.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// Gets or sets the username (optional, 3-30 characters).
    /// </summary>
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters")]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets user preferences as JSON string.
    /// </summary>
    public string? Preferences { get; set; }

    /// <summary>
    /// Gets or sets the display name (kept for backward compatibility).
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the biography (kept for backward compatibility).
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Gets or sets the avatar file for upload (client-side only).
    /// </summary>
    public IBrowserFile? AvatarFile { get; set; }
}

