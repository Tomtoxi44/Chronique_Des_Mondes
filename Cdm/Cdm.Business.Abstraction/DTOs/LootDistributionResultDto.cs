// -----------------------------------------------------------------------
// <copyright file="LootDistributionResultDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Describes a loot hand-out: used both as the distribution result and as the
/// SignalR "LootReceived" broadcast payload so players can react in real time.
/// </summary>
public class LootDistributionResultDto
{
    /// <summary>Gets or sets the loot identifier.</summary>
    public int LootId { get; set; }

    /// <summary>Gets or sets the loot name.</summary>
    public string LootName { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the optional item category.</summary>
    public string? ItemType { get; set; }

    /// <summary>Gets or sets the quantity handed out.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the recipient world character identifier.</summary>
    public int RecipientWorldCharacterId { get; set; }

    /// <summary>Gets or sets the recipient character display name.</summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient user identifier.</summary>
    public int RecipientUserId { get; set; }
}
