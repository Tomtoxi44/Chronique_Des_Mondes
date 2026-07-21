// -----------------------------------------------------------------------
// <copyright file="CampaignLoot.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using Cdm.Common.Enums;

/// <summary>
/// A loot item the Game Master prepared for a campaign (optionally scoped to a chapter),
/// ready to be handed out to players during a session. Item data is a self-contained snapshot
/// (same shape as <see cref="CodexItem"/>) so editing the source codex later does not change it.
/// </summary>
public class CampaignLoot
{
    /// <summary>Gets or sets the loot identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the campaign this loot belongs to.</summary>
    public int CampaignId { get; set; }

    /// <summary>Gets or sets the optional chapter this loot is scoped to (null = whole campaign).</summary>
    public int? ChapterId { get; set; }

    /// <summary>Gets or sets the loot name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the optional image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the game system / theme of the item.</summary>
    public GameType GameType { get; set; }

    /// <summary>Gets or sets the optional item category (e.g. "Arme", "Potion").</summary>
    public string? ItemType { get; set; }

    /// <summary>Gets or sets the optional game-specific data (JSON).</summary>
    public string? GameSpecificData { get; set; }

    /// <summary>Gets or sets the quantity handed out per distribution.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Gets or sets the user ID of the GM who created this loot.</summary>
    public int CreatedBy { get; set; }

    /// <summary>Gets or sets a value indicating whether the loot is active (soft delete).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update date.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Gets or sets the campaign navigation property.</summary>
    public virtual Campaign Campaign { get; set; } = null!;

    /// <summary>Gets or sets the chapter navigation property (optional).</summary>
    public virtual Chapter? Chapter { get; set; }

    /// <summary>Gets or sets the distributions of this loot to players.</summary>
    public virtual ICollection<LootDistribution> Distributions { get; set; } = new List<LootDistribution>();
}
