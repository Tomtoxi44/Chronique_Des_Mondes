namespace Cdm.Business.Abstraction.DTOs.ViewModels;

/// <summary>
/// Response DTO for user profile information
/// </summary>
public class ProfileResponse
{
    /// <summary>
    /// User unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User display nickname
    /// </summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// User unique username (optional)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Avatar image URL or path
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User preferences as JSON string
    /// </summary>
    public string? Preferences { get; set; }

    /// <summary>
    /// Account creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
