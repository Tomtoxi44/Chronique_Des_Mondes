using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;

namespace Cdm.Web.Models;

/// <summary>
/// View-friendly model representing an achievement that can be unlocked by players.
/// </summary>
public class AchievementModel
{
    /// <summary>Achievement identifier.</summary>
    public int Id { get; set; }

    /// <summary>Achievement name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Achievement description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Scope level of the achievement (World, Campaign, or Chapter).</summary>
    public AchievementLevel Level { get; set; }

    /// <summary>World identifier, if the achievement is scoped to a world.</summary>
    public int? WorldId { get; set; }

    /// <summary>Campaign identifier, if the achievement is scoped to a campaign.</summary>
    public int? CampaignId { get; set; }

    /// <summary>Chapter identifier, if the achievement is scoped to a chapter.</summary>
    public int? ChapterId { get; set; }

    /// <summary>Rarity tier of the achievement.</summary>
    public AchievementRarity Rarity { get; set; }

    /// <summary>URL to the achievement icon.</summary>
    public string? IconUrl { get; set; }

    /// <summary>Description of the reward granted on unlock.</summary>
    public string? RewardDescription { get; set; }

    /// <summary>Whether the achievement is unlocked automatically.</summary>
    public bool IsAutomatic { get; set; }

    /// <summary>Whether the achievement is hidden until unlocked.</summary>
    public bool IsSecret { get; set; }

    /// <summary>Whether the achievement is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Date the achievement was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date the achievement was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Identifier of the user who created the achievement.</summary>
    public int CreatedBy { get; set; }

    /// <summary>Number of times this achievement has been unlocked.</summary>
    public int UnlockCount { get; set; }

    /// <summary>Display name for the achievement level.</summary>
    public string LevelDisplayName => Level switch
    {
        AchievementLevel.World => "Monde",
        AchievementLevel.Campaign => "Campagne",
        AchievementLevel.Chapter => "Chapitre",
        _ => "Inconnu"
    };

    /// <summary>Display name for the rarity tier.</summary>
    public string RarityDisplayName => Rarity switch
    {
        AchievementRarity.Common => "Commun",
        AchievementRarity.Rare => "Rare",
        AchievementRarity.Epic => "Épique",
        AchievementRarity.Legendary => "Légendaire",
        _ => "Inconnu"
    };

    /// <summary>CSS class for the rarity badge.</summary>
    public string RarityBadgeClass => Rarity switch
    {
        AchievementRarity.Common => "badge-secondary",
        AchievementRarity.Rare => "badge-info",
        AchievementRarity.Epic => "badge-primary",
        AchievementRarity.Legendary => "badge-warning",
        _ => "badge-secondary"
    };

    /// <summary>CSS class for the rarity text color.</summary>
    public string RarityColorClass => Rarity switch
    {
        AchievementRarity.Common => "text-secondary",
        AchievementRarity.Rare => "text-info",
        AchievementRarity.Epic => "text-primary",
        AchievementRarity.Legendary => "text-warning",
        _ => "text-secondary"
    };

    /// <summary>Whether the achievement has an icon.</summary>
    public bool HasIcon => !string.IsNullOrWhiteSpace(IconUrl);

    /// <summary>Whether the achievement has a reward description.</summary>
    public bool HasReward => !string.IsNullOrWhiteSpace(RewardDescription);

    /// <summary>Formatted creation date string.</summary>
    public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy");

    /// <summary>
    /// Creates an <see cref="AchievementModel"/> from an <see cref="AchievementDto"/>.
    /// </summary>
    public static AchievementModel FromDto(AchievementDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        Level = dto.Level,
        WorldId = dto.WorldId,
        CampaignId = dto.CampaignId,
        ChapterId = dto.ChapterId,
        Rarity = dto.Rarity,
        IconUrl = dto.IconUrl,
        RewardDescription = dto.RewardDescription,
        IsAutomatic = dto.IsAutomatic,
        IsSecret = dto.IsSecret,
        IsActive = dto.IsActive,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        CreatedBy = dto.CreatedBy,
        UnlockCount = dto.UnlockCount
    };
}
