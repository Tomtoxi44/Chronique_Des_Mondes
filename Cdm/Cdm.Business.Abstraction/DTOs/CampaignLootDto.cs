// -----------------------------------------------------------------------
// <copyright file="CampaignLootDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>A loot item prepared by the GM for a campaign (optionally a chapter).</summary>
public class CampaignLootDto
{
    /// <summary>Gets or sets the loot identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the campaign this loot belongs to.</summary>
    public int CampaignId { get; set; }

    /// <summary>Gets or sets the optional chapter scope (null = whole campaign).</summary>
    public int? ChapterId { get; set; }

    /// <summary>Gets or sets the loot name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the optional image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the game system / theme.</summary>
    public GameType GameType { get; set; }

    /// <summary>Gets or sets the optional item category.</summary>
    public string? ItemType { get; set; }

    /// <summary>Gets or sets the optional game-specific data (JSON).</summary>
    public string? GameSpecificData { get; set; }

    /// <summary>Gets or sets the quantity handed out per distribution.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }
}
