// -----------------------------------------------------------------------
// <copyright file="CreateCampaignRequest.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints.Models;

using Cdm.Common.Enums;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request model for creating a new campaign.
/// </summary>
public class CreateCampaignRequest
{
    /// <summary>
    /// Gets or sets the campaign name.
    /// </summary>
    [Required(ErrorMessage = "Campaign name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Campaign name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the campaign description.
    /// </summary>
    [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the world ID this campaign belongs to.
    /// </summary>
    [Required(ErrorMessage = "World ID is required")]
    public int WorldId { get; set; }

    /// <summary>
    /// Gets or sets the campaign visibility level.
    /// </summary>
    public Visibility Visibility { get; set; } = Visibility.Private;

    /// <summary>
    /// Gets or sets the maximum number of players allowed in the campaign.
    /// </summary>
    [Range(1, 20, ErrorMessage = "Maximum players must be between 1 and 20")]
    public int MaxPlayers { get; set; } = 6;

    /// <summary>
    /// Gets or sets the campaign cover image as a Base64-encoded string.
    /// </summary>
    public string? CoverImageBase64 { get; set; }
}
