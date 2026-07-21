// -----------------------------------------------------------------------
// <copyright file="LootDistribution.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Records that a piece of <see cref="CampaignLoot"/> was handed out to a player's character
/// (usually during a session). One row per recipient, so the same loot can be given to several players.
/// </summary>
public class LootDistribution
{
    /// <summary>Gets or sets the distribution identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the loot that was distributed.</summary>
    public int LootId { get; set; }

    /// <summary>Gets or sets the recipient world character.</summary>
    public int WorldCharacterId { get; set; }

    /// <summary>Gets or sets the session during which the loot was distributed (optional).</summary>
    public int? SessionId { get; set; }

    /// <summary>Gets or sets the user ID of the GM who distributed the loot.</summary>
    public int DistributedByUserId { get; set; }

    /// <summary>Gets or sets when the loot was distributed.</summary>
    public DateTime DistributedAt { get; set; }

    /// <summary>Gets or sets the loot navigation property.</summary>
    public virtual CampaignLoot Loot { get; set; } = null!;

    /// <summary>Gets or sets the recipient world character navigation property.</summary>
    public virtual WorldCharacter WorldCharacter { get; set; } = null!;
}
