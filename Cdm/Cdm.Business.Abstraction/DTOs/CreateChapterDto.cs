// -----------------------------------------------------------------------
// <copyright file="CreateChapterDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for creating a new Chapter.
/// </summary>
public class CreateChapterDto
{
    /// <summary>
    /// Gets or sets the campaign ID this chapter belongs to.
    /// </summary>
    [Required(ErrorMessage = "Campaign ID is required")]
    public int CampaignId { get; set; }

    /// <summary>
    /// Gets or sets the chapter number (sequential ordering).
    /// If not provided or 0, auto-increments from the last chapter.
    /// </summary>
    public int ChapterNumber { get; set; }

    /// <summary>
    /// Gets or sets the chapter title (3-200 characters).
    /// </summary>
    [Required(ErrorMessage = "Chapter title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the narrative content of the chapter (max 10000 characters).
    /// </summary>
    [MaxLength(10000, ErrorMessage = "Content cannot exceed 10000 characters")]
    public string? Content { get; set; }
}
