// -----------------------------------------------------------------------
// <copyright file="ChapterDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Data transfer object for a Chapter.
/// </summary>
public class ChapterDto
{
    /// <summary>
    /// Gets or sets the chapter identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the campaign ID this chapter belongs to.
    /// </summary>
    public int CampaignId { get; set; }

    /// <summary>
    /// Gets or sets the chapter number (sequential ordering).
    /// </summary>
    public int ChapterNumber { get; set; }

    /// <summary>
    /// Gets or sets the chapter title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the narrative content of the chapter.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the chapter is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the chapter is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the chapter creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of events in this chapter.
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Gets or sets the number of achievements in this chapter.
    /// </summary>
    public int AchievementCount { get; set; }
}
