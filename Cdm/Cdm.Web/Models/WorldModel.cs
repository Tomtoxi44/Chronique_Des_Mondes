using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;

namespace Cdm.Web.Models;

/// <summary>
/// View-friendly model representing a world in the application.
/// </summary>
public class WorldModel
{
    /// <summary>World identifier.</summary>
    public int Id { get; set; }

    /// <summary>Owner user identifier.</summary>
    public int UserId { get; set; }

    /// <summary>World name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description of the world.</summary>
    public string? Description { get; set; }

    /// <summary>The game system used by this world.</summary>
    public GameType GameType { get; set; }

    /// <summary>Whether the world is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Date the world was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date the world was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Number of campaigns in this world.</summary>
    public int CampaignCount { get; set; }

    /// <summary>Number of characters in this world.</summary>
    public int CharacterCount { get; set; }

    /// <summary>Display name for the game type.</summary>
    public string GameTypeDisplayName => GameTypeInfo.GetDisplayName(GameType);

    /// <summary>CSS icon class for the game type.</summary>
    public string GameTypeIcon => GameTypeInfo.GetIcon(GameType);

    /// <summary>CSS class for the game type badge.</summary>
    public string GameTypeCssClass => GameTypeInfo.GetCssClass(GameType);

    /// <summary>Formatted creation date string.</summary>
    public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy");

    /// <summary>Formatted update date string, or empty if never updated.</summary>
    public string UpdatedAtDisplay => UpdatedAt?.ToString("dd MMM yyyy") ?? string.Empty;

    /// <summary>Whether the world has a description.</summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    /// <summary>
    /// Creates a <see cref="WorldModel"/> from a <see cref="WorldDto"/>.
    /// </summary>
    public static WorldModel FromDto(WorldDto dto) => new()
    {
        Id = dto.Id,
        UserId = dto.UserId,
        Name = dto.Name,
        Description = dto.Description,
        GameType = dto.GameType,
        IsActive = dto.IsActive,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        CampaignCount = dto.CampaignCount,
        CharacterCount = dto.CharacterCount
    };
}
