// -----------------------------------------------------------------------
// <copyright file="UpdateCampaignDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System.ComponentModel.DataAnnotations;
using Cdm.Common.Enums;

/// <summary>
/// DTO for updating an existing campaign.
/// </summary>
public class UpdateCampaignDto
{
    /// <summary>
    /// Gets or sets the campaign name (3-100 characters).
    /// </summary>
    [Required(ErrorMessage = "Campaign name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the campaign description (max 5000 characters).
    /// </summary>
    [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the campaign visibility.
    /// </summary>
    [Required(ErrorMessage = "Visibility is required.")]
    public Visibility Visibility { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of players (1-20).
    /// Note: Cannot be less than current player count (validated in service).
    /// </summary>
    [Required(ErrorMessage = "Max players is required.")]
    [Range(1, 20, ErrorMessage = "Max players must be between 1 and 20.")]
    public int MaxPlayers { get; set; }

    /// <summary>
    /// Gets or sets the cover image in Base64 format.
    /// If null or empty, the existing image is kept.
    /// </summary>
    public string? CoverImageBase64 { get; set; }
}
