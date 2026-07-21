// -----------------------------------------------------------------------
// <copyright file="InventoryItemDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>
/// A unified inventory item for a world character (any game system). The D&amp;D combat fields
/// are only meaningful when <see cref="GameType"/> is <see cref="Cdm.Common.Enums.GameType.DnD5e"/>.
/// </summary>
public class InventoryItemDto
{
    /// <summary>Gets or sets the item identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the owning world character identifier.</summary>
    public int WorldCharacterId { get; set; }

    /// <summary>Gets or sets the item name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category (e.g. "Arme", "Potion", "Objet").</summary>
    public string Category { get; set; } = "Objet";

    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Gets or sets the free-text description / notes.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the optional image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the game system this item belongs to.</summary>
    public GameType GameType { get; set; } = GameType.Generic;

    /// <summary>Gets or sets the optional theme-specific structured stats (JSON).</summary>
    public string? GameSpecificData { get; set; }

    // ── D&D 5e combat fields (used only when GameType == DnD5e) ──
    public int? AttackBonus { get; set; }

    public string? DamageDice { get; set; }

    public string? DamageType { get; set; }
}
