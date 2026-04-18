// -----------------------------------------------------------------------
// <copyright file="ICampaignService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Services;

using Cdm.Web.Models;

/// <summary>
/// Service for managing campaigns from the client side.
/// </summary>
public interface ICampaignService
{
    /// <summary>
    /// Creates a new campaign.
    /// </summary>
    /// <param name="model">The campaign creation model.</param>
    /// <returns>The created campaign response, or null if creation failed.</returns>
    Task<CampaignResponse?> CreateCampaignAsync(CreateCampaignModel model);
}

/// <summary>
/// Response model for a created campaign.
/// </summary>
public class CampaignResponse
{
    /// <summary>
    /// Gets or sets the campaign identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the campaign name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the campaign description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the game type.
    /// </summary>
    public int GameType { get; set; }

    /// <summary>
    /// Gets or sets the visibility level.
    /// </summary>
    public int Visibility { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of players.
    /// </summary>
    public int MaxPlayers { get; set; }

    /// <summary>
    /// Gets or sets the cover image URL.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the creator user ID.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the campaign is active.
    /// </summary>
    public bool IsActive { get; set; }
}
