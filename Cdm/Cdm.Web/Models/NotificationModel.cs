using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;

namespace Cdm.Web.Models;

/// <summary>
/// View-friendly model representing a user notification.
/// </summary>
public class NotificationModel
{
    /// <summary>Notification identifier.</summary>
    public int Id { get; set; }

    /// <summary>Recipient user identifier.</summary>
    public int UserId { get; set; }

    /// <summary>Type of notification.</summary>
    public NotificationType Type { get; set; }

    /// <summary>Notification title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Notification message body.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Identifier of the related entity, if any.</summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>Type name of the related entity.</summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>URL for the notification action.</summary>
    public string? ActionUrl { get; set; }

    /// <summary>Whether the notification has been read.</summary>
    public bool IsRead { get; set; }

    /// <summary>Date the notification was read.</summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>Date the notification was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Identifier of the user who triggered the notification.</summary>
    public int? SentBy { get; set; }

    /// <summary>Display name for the notification type.</summary>
    public string TypeDisplayName => Type switch
    {
        NotificationType.CampaignInvite => "Invitation à une campagne",
        NotificationType.SessionStarting => "Session imminente",
        NotificationType.AchievementUnlocked => "Succès débloqué",
        NotificationType.TradeProposed => "Échange proposé",
        NotificationType.MessageMention => "Mention",
        NotificationType.CombatTurn => "Tour de combat",
        NotificationType.SystemAnnouncement => "Annonce système",
        NotificationType.CharacterUpdate => "Mise à jour du personnage",
        _ => "Notification"
    };

    /// <summary>CSS icon class for the notification type.</summary>
    public string TypeIcon => Type switch
    {
        NotificationType.CampaignInvite => "bi-envelope",
        NotificationType.SessionStarting => "bi-clock",
        NotificationType.AchievementUnlocked => "bi-trophy",
        NotificationType.TradeProposed => "bi-arrow-left-right",
        NotificationType.MessageMention => "bi-at",
        NotificationType.CombatTurn => "bi-shield-exclamation",
        NotificationType.SystemAnnouncement => "bi-megaphone",
        NotificationType.CharacterUpdate => "bi-person-gear",
        _ => "bi-bell"
    };

    /// <summary>Whether the notification has an action URL.</summary>
    public bool HasAction => !string.IsNullOrWhiteSpace(ActionUrl);

    /// <summary>Relative time display for the notification (e.g., "il y a 3h").</summary>
    public string TimeAgoDisplay
    {
        get
        {
            var elapsed = DateTime.UtcNow - CreatedAt;
            return elapsed.TotalMinutes switch
            {
                < 1 => "À l'instant",
                < 60 => $"il y a {(int)elapsed.TotalMinutes} min",
                < 1440 => $"il y a {(int)elapsed.TotalHours}h",
                < 43200 => $"il y a {(int)elapsed.TotalDays}j",
                _ => CreatedAt.ToString("dd MMM yyyy")
            };
        }
    }

    /// <summary>Formatted creation date string.</summary>
    public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy HH:mm");

    /// <summary>
    /// Creates a <see cref="NotificationModel"/> from a <see cref="NotificationDto"/>.
    /// </summary>
    public static NotificationModel FromDto(NotificationDto dto) => new()
    {
        Id = dto.Id,
        UserId = dto.UserId,
        Type = dto.Type,
        Title = dto.Title,
        Message = dto.Message,
        RelatedEntityId = dto.RelatedEntityId,
        RelatedEntityType = dto.RelatedEntityType,
        ActionUrl = dto.ActionUrl,
        IsRead = dto.IsRead,
        ReadAt = dto.ReadAt,
        CreatedAt = dto.CreatedAt,
        SentBy = dto.SentBy
    };
}
