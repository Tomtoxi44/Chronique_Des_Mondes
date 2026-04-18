using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;

namespace Cdm.Web.Models;

/// <summary>
/// View-friendly model representing an event that can affect characters or the narrative.
/// </summary>
public class EventModel
{
    /// <summary>Event identifier.</summary>
    public int Id { get; set; }

    /// <summary>Event name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional event description.</summary>
    public string? Description { get; set; }

    /// <summary>Scope level of the event (World, Campaign, or Chapter).</summary>
    public EventLevel Level { get; set; }

    /// <summary>World identifier, if the event is scoped to a world.</summary>
    public int? WorldId { get; set; }

    /// <summary>Campaign identifier, if the event is scoped to a campaign.</summary>
    public int? CampaignId { get; set; }

    /// <summary>Chapter identifier, if the event is scoped to a chapter.</summary>
    public int? ChapterId { get; set; }

    /// <summary>The type of effect this event applies.</summary>
    public EventEffectType EffectType { get; set; }

    /// <summary>The stat targeted by the effect, if applicable.</summary>
    public string? TargetStat { get; set; }

    /// <summary>The modifier value applied by the effect.</summary>
    public int? ModifierValue { get; set; }

    /// <summary>Whether the event is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Whether the event effect is permanent.</summary>
    public bool IsPermanent { get; set; }

    /// <summary>Expiration date of the event effect, if not permanent.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Date the event was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date the event was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Identifier of the user who created the event.</summary>
    public int CreatedBy { get; set; }

    /// <summary>Display name for the event level.</summary>
    public string LevelDisplayName => Level switch
    {
        EventLevel.World => "Monde",
        EventLevel.Campaign => "Campagne",
        EventLevel.Chapter => "Chapitre",
        _ => "Inconnu"
    };

    /// <summary>Display name for the effect type.</summary>
    public string EffectTypeDisplayName => EffectType switch
    {
        EventEffectType.StatModifier => "Modificateur de stat",
        EventEffectType.HealthModifier => "Modificateur de santé",
        EventEffectType.DiceModifier => "Modificateur de dé",
        EventEffectType.Narrative => "Narratif",
        _ => "Inconnu"
    };

    /// <summary>CSS class for the event level badge.</summary>
    public string LevelBadgeClass => Level switch
    {
        EventLevel.World => "badge-primary",
        EventLevel.Campaign => "badge-info",
        EventLevel.Chapter => "badge-secondary",
        _ => "badge-secondary"
    };

    /// <summary>Whether the event has a description.</summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    /// <summary>Whether the event has expired.</summary>
    public bool IsExpired => !IsPermanent && ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    /// <summary>Display text for the event duration.</summary>
    public string DurationDisplay => IsPermanent
        ? "Permanent"
        : ExpiresAt.HasValue
            ? $"Expire le {ExpiresAt.Value:dd MMM yyyy}"
            : "Temporaire";

    /// <summary>Formatted modifier display (e.g., "+3 Force").</summary>
    public string ModifierDisplay => ModifierValue.HasValue && !string.IsNullOrWhiteSpace(TargetStat)
        ? $"{(ModifierValue.Value >= 0 ? "+" : "")}{ModifierValue.Value} {TargetStat}"
        : string.Empty;

    /// <summary>Formatted creation date string.</summary>
    public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy");

    /// <summary>
    /// Creates an <see cref="EventModel"/> from an <see cref="EventDto"/>.
    /// </summary>
    public static EventModel FromDto(EventDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        Level = dto.Level,
        WorldId = dto.WorldId,
        CampaignId = dto.CampaignId,
        ChapterId = dto.ChapterId,
        EffectType = dto.EffectType,
        TargetStat = dto.TargetStat,
        ModifierValue = dto.ModifierValue,
        IsActive = dto.IsActive,
        IsPermanent = dto.IsPermanent,
        ExpiresAt = dto.ExpiresAt,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        CreatedBy = dto.CreatedBy
    };
}
