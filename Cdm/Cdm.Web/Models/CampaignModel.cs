using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;

namespace Cdm.Web.Models;

/// <summary>
/// View-friendly model representing a campaign.
/// </summary>
public class CampaignModel
{
    /// <summary>Campaign identifier.</summary>
    public int Id { get; set; }

    /// <summary>Campaign name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional campaign description.</summary>
    public string? Description { get; set; }

    /// <summary>Identifier of the world this campaign belongs to.</summary>
    public int WorldId { get; set; }

    /// <summary>The game system used by this campaign.</summary>
    public GameType GameType { get; set; }

    /// <summary>Campaign visibility (public or private).</summary>
    public Visibility Visibility { get; set; }

    /// <summary>Maximum number of players allowed.</summary>
    public int MaxPlayers { get; set; }

    /// <summary>URL to the campaign cover image.</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>Identifier of the user who created the campaign.</summary>
    public int CreatedBy { get; set; }

    /// <summary>Date the campaign was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date the campaign was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Current status of the campaign.</summary>
    public CampaignStatus Status { get; set; }

    /// <summary>Invitation token for joining the campaign.</summary>
    public string? InviteToken { get; set; }

    /// <summary>Whether the campaign is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Display name for the game type.</summary>
    public string GameTypeDisplayName => GameTypeInfo.GetDisplayName(GameType);

    /// <summary>CSS icon class for the game type.</summary>
    public string GameTypeIcon => GameTypeInfo.GetIcon(GameType);

    /// <summary>Whether the campaign is publicly visible.</summary>
    public bool IsPublic => Visibility == Visibility.Public;

    /// <summary>Whether the campaign has a cover image.</summary>
    public bool HasCoverImage => !string.IsNullOrWhiteSpace(CoverImageUrl);

    /// <summary>Whether the campaign has an invite token.</summary>
    public bool HasInviteToken => !string.IsNullOrWhiteSpace(InviteToken);

    /// <summary>Whether the campaign has a description.</summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    /// <summary>Formatted creation date string.</summary>
    public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy");

    /// <summary>Formatted update date string, or empty if never updated.</summary>
    public string UpdatedAtDisplay => UpdatedAt?.ToString("dd MMM yyyy") ?? string.Empty;

    /// <summary>Human-readable display name for the campaign status.</summary>
    public string StatusDisplayName => Status switch
    {
        CampaignStatus.Planning => "En préparation",
        CampaignStatus.Active => "Active",
        CampaignStatus.OnHold => "En pause",
        CampaignStatus.Completed => "Terminée",
        CampaignStatus.Cancelled => "Annulée",
        _ => "Inconnu"
    };

    /// <summary>CSS class for the status badge.</summary>
    public string StatusBadgeClass => Status switch
    {
        CampaignStatus.Planning => "badge-warning",
        CampaignStatus.Active => "badge-success",
        CampaignStatus.OnHold => "badge-info",
        CampaignStatus.Completed => "badge-primary",
        CampaignStatus.Cancelled => "badge-danger",
        _ => "badge-secondary"
    };

    /// <summary>Display name for the visibility setting.</summary>
    public string VisibilityDisplayName => Visibility switch
    {
        Visibility.Private => "Privée",
        Visibility.Public => "Publique",
        _ => "Inconnu"
    };

    /// <summary>
    /// Creates a <see cref="CampaignModel"/> from a <see cref="CampaignDto"/>.
    /// </summary>
    public static CampaignModel FromDto(CampaignDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        WorldId = dto.WorldId,
        GameType = dto.GameType,
        Visibility = dto.Visibility,
        MaxPlayers = dto.MaxPlayers,
        CoverImageUrl = dto.CoverImageUrl,
        CreatedBy = dto.CreatedBy,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        Status = dto.Status,
        InviteToken = dto.InviteToken,
        IsActive = dto.IsActive
    };
}
