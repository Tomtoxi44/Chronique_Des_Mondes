// -----------------------------------------------------------------------
// <copyright file="DndInventoryItemDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>An item in a character's D&amp;D inventory (world-specific).</summary>
public class DndInventoryItemDto
{
    public int Id { get; set; }
    public int WorldCharacterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public int? AttackBonus { get; set; }
    public string? DamageDice { get; set; }
    public string? DamageType { get; set; }
    public string? Notes { get; set; }
    /// <summary>Reference to a DndItem template, if selected from the list.</summary>
    public int? DndItemId { get; set; }
}
