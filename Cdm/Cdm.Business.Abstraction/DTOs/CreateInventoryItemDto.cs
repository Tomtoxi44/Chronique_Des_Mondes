// -----------------------------------------------------------------------
// <copyright file="CreateInventoryItemDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>Payload to create or update a unified inventory item.</summary>
public class CreateInventoryItemDto
{
    /// <summary>Gets or sets the item name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category.</summary>
    public string Category { get; set; } = "Objet";

    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Gets or sets the free-text description / notes.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the optional image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the game system this item belongs to. Set to a specific system
    /// (e.g. DnD5e) to "specify" a generic item so that system's calculations apply.
    /// </summary>
    public GameType GameType { get; set; } = GameType.Generic;

    /// <summary>Gets or sets the optional theme-specific structured stats (JSON).</summary>
    public string? GameSpecificData { get; set; }

    // ── D&D 5e combat fields (used only when GameType == DnD5e) ──
    public int? AttackBonus { get; set; }

    public string? DamageDice { get; set; }

    public string? DamageType { get; set; }
}
