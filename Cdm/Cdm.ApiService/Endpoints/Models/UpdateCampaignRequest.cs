// -----------------------------------------------------------------------
// <copyright file="UpdateCampaignRequest.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints.Models;

using System.ComponentModel.DataAnnotations;
using Cdm.Common.Enums;

/// <summary>
/// Request model for updating a campaign.
/// </summary>
public class UpdateCampaignRequest
{
    /// <summary>
    /// Gets or sets the campaign name.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the campaign description.
    /// </summary>
    [MaxLength(5000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the campaign visibility.
    /// </summary>
    [Required]
    public Visibility Visibility { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of players.
    /// </summary>
    [Required]
    [Range(1, 20)]
    public int MaxPlayers { get; set; }

    /// <summary>
    /// Gets or sets the cover image in Base64 format.
    /// If null or empty, the existing image is kept.
    /// </summary>
    public string? CoverImageBase64 { get; set; }
}
