// -----------------------------------------------------------------------
// <copyright file="ChapterImageDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>An image (map/place visual) attached to a chapter.</summary>
public class ChapterImageDto
{
    /// <summary>Gets or sets the image identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the chapter identifier.</summary>
    public int ChapterId { get; set; }

    /// <summary>Gets or sets the image URL.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional caption.</summary>
    public string? Caption { get; set; }

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }
}
