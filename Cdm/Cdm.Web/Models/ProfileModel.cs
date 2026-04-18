using Cdm.Business.Abstraction.DTOs.ViewModels;

namespace Cdm.Web.Models;

/// <summary>
/// View-friendly model representing the current user's profile.
/// </summary>
public class ProfileModel
{
    /// <summary>User identifier.</summary>
    public int Id { get; set; }

    /// <summary>User email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>User display nickname.</summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>Optional username.</summary>
    public string? Username { get; set; }

    /// <summary>URL to the user's avatar image.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>User preferences as a JSON string.</summary>
    public string? Preferences { get; set; }

    /// <summary>Date the account was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date of the last login.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Best available display name (nickname, username, or email).</summary>
    public string DisplayName => !string.IsNullOrWhiteSpace(Nickname)
        ? Nickname
        : !string.IsNullOrWhiteSpace(Username)
            ? Username
            : Email;

    /// <summary>Whether the user has an avatar image.</summary>
    public bool HasAvatar => !string.IsNullOrWhiteSpace(AvatarUrl);

    /// <summary>Whether the user has a username set.</summary>
    public bool HasUsername => !string.IsNullOrWhiteSpace(Username);

    /// <summary>Formatted account creation date.</summary>
    public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy");

    /// <summary>Formatted last login date, or "Jamais" if never logged in.</summary>
    public string LastLoginAtDisplay => LastLoginAt?.ToString("dd MMM yyyy HH:mm") ?? "Jamais";

    /// <summary>
    /// Creates a <see cref="ProfileModel"/> from a <see cref="ProfileResponse"/>.
    /// </summary>
    public static ProfileModel FromResponse(ProfileResponse response) => new()
    {
        Id = response.Id,
        Email = response.Email,
        Nickname = response.Nickname,
        Username = response.Username,
        AvatarUrl = response.AvatarUrl,
        Preferences = response.Preferences,
        CreatedAt = response.CreatedAt,
        LastLoginAt = response.LastLoginAt
    };
}
