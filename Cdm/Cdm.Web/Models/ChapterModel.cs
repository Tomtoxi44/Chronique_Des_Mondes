using Cdm.Business.Abstraction.DTOs;

namespace Cdm.Web.Models;

/// <summary>
/// View-friendly model representing a chapter within a campaign.
/// </summary>
public class ChapterModel
{
    /// <summary>Chapter identifier.</summary>
    public int Id { get; set; }

    /// <summary>Identifier of the campaign this chapter belongs to.</summary>
    public int CampaignId { get; set; }

    /// <summary>Sequential chapter number.</summary>
    public int ChapterNumber { get; set; }

    /// <summary>Chapter title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Chapter content or narrative text.</summary>
    public string? Content { get; set; }

    /// <summary>Whether the chapter has been completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Whether the chapter is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Date the chapter was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date the chapter was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Date the chapter was completed.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Number of events in this chapter.</summary>
    public int EventCount { get; set; }

    /// <summary>Number of achievements in this chapter.</summary>
    public int AchievementCount { get; set; }

    /// <summary>Formatted display title including the chapter number.</summary>
    public string DisplayTitle => $"Chapitre {ChapterNumber} — {Title}";

    /// <summary>Whether the chapter has content.</summary>
    public bool HasContent => !string.IsNullOrWhiteSpace(Content);

    /// <summary>Human-readable status label.</summary>
    public string StatusDisplayName => IsCompleted ? "Terminé" : "En cours";

    /// <summary>CSS class for the status badge.</summary>
    public string StatusBadgeClass => IsCompleted ? "badge-success" : "badge-warning";

    /// <summary>Formatted creation date string.</summary>
    public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy");

    /// <summary>Formatted completion date string, or empty if not completed.</summary>
    public string CompletedAtDisplay => CompletedAt?.ToString("dd MMM yyyy") ?? string.Empty;

    /// <summary>
    /// Creates a <see cref="ChapterModel"/> from a <see cref="ChapterDto"/>.
    /// </summary>
    public static ChapterModel FromDto(ChapterDto dto) => new()
    {
        Id = dto.Id,
        CampaignId = dto.CampaignId,
        ChapterNumber = dto.ChapterNumber,
        Title = dto.Title,
        Content = dto.Content,
        IsCompleted = dto.IsCompleted,
        IsActive = dto.IsActive,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        CompletedAt = dto.CompletedAt,
        EventCount = dto.EventCount,
        AchievementCount = dto.AchievementCount
    };
}
