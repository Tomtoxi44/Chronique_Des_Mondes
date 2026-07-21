// -----------------------------------------------------------------------
// <copyright file="AddChapterImageDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>Payload to attach an image to a chapter.</summary>
public class AddChapterImageDto
{
    /// <summary>Gets or sets the image URL (already uploaded via the image endpoint).</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional caption.</summary>
    public string? Caption { get; set; }
}
