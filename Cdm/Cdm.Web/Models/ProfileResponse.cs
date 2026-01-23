namespace Cdm.Web.Models;

/// <summary>
/// Response model for user profile information.
/// </summary>
public class ProfileResponse
{
    /// <summary>
    /// Gets or sets the user's unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display nickname.
    /// </summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username (optional).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the display name (kept for backward compatibility).
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the biography (kept for backward compatibility).
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets user preferences as JSON string.
    /// </summary>
    public string? Preferences { get; set; }

    /// <summary>
    /// Gets or sets the date when the account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last login timestamp.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

