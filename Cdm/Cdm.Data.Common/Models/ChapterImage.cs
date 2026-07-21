// -----------------------------------------------------------------------
// <copyright file="ChapterImage.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// An image attached to a chapter by the Game Master (a map/plan or a place visual),
/// which can be shown to players during a session.
/// </summary>
public class ChapterImage
{
    /// <summary>Gets or sets the image identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the chapter this image belongs to.</summary>
    public int ChapterId { get; set; }

    /// <summary>Gets or sets the image URL.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional caption (e.g. "Plan du donjon", "Taverne du Sanglier").</summary>
    public string? Caption { get; set; }

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the chapter navigation property.</summary>
    public virtual Chapter Chapter { get; set; } = null!;
}
