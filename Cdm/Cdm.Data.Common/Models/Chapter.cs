// -----------------------------------------------------------------------
// <copyright file="Chapter.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a chapter within a campaign.
/// A chapter is a session/episode of a campaign with organized content.
/// </summary>
[Table("Chapters")]
public class Chapter
{
    /// <summary>
    /// Gets or sets the chapter unique identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the campaign ID this chapter belongs to.
    /// </summary>
    [Required]
    public int CampaignId { get; set; }

    /// <summary>
    /// Gets or sets the chapter number (sequential ordering).
    /// </summary>
    [Required]
    public int ChapterNumber { get; set; }

    /// <summary>
    /// Gets or sets the chapter title (max 200 characters).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the narrative content of the chapter (max 10000 characters).
    /// </summary>
    [MaxLength(10000)]
    [Column(TypeName = "nvarchar(max)")]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the chapter is completed.
    /// </summary>
    [Required]
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the chapter is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the chapter creation timestamp (UTC).
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update timestamp (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the completion timestamp (UTC).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the campaign.
    /// </summary>
    [ForeignKey(nameof(CampaignId))]
    public virtual Campaign Campaign { get; set; } = null!;

    /// <summary>
    /// Gets or sets the events specific to this chapter.
    /// </summary>
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    /// <summary>
    /// Gets or sets the achievements specific to this chapter.
    /// </summary>
    public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
}
