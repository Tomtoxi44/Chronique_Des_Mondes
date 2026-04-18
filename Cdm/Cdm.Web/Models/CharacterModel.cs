using Cdm.Web.Shared.DTOs.Models;

namespace Cdm.Web.Models;

/// <summary>
/// View-friendly model representing a character.
/// </summary>
public class CharacterModel
{
    /// <summary>Character identifier.</summary>
    public int Id { get; set; }

    /// <summary>Owner user identifier.</summary>
    public int UserId { get; set; }

    /// <summary>Character last name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Character first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Character description or backstory.</summary>
    public string? Description { get; set; }

    /// <summary>Character age.</summary>
    public int? Age { get; set; }

    /// <summary>URL to the character's avatar image.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Date the character was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date the character was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Full display name combining first name and last name.</summary>
    public string FullName => string.IsNullOrWhiteSpace(FirstName)
        ? Name
        : $"{FirstName} {Name}";

    /// <summary>Whether the character has an avatar image.</summary>
    public bool HasAvatar => !string.IsNullOrWhiteSpace(AvatarUrl);

    /// <summary>Whether the character has a description.</summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    /// <summary>Display text for the character's age.</summary>
    public string AgeDisplay => Age.HasValue ? $"{Age} ans" : "Âge inconnu";

    /// <summary>Formatted creation date string.</summary>
    public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy");

    /// <summary>Formatted update date string, or empty if never updated.</summary>
    public string UpdatedAtDisplay => UpdatedAt?.ToString("dd MMM yyyy") ?? string.Empty;

    /// <summary>
    /// Creates a <see cref="CharacterModel"/> from a <see cref="CharacterDto"/>.
    /// </summary>
    public static CharacterModel FromDto(CharacterDto dto) => new()
    {
        Id = dto.Id,
        UserId = dto.UserId,
        Name = dto.Name,
        FirstName = dto.FirstName,
        Description = dto.Description,
        Age = dto.Age,
        AvatarUrl = dto.AvatarUrl,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt
    };
}
