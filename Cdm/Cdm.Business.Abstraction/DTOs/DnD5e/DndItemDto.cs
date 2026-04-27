// -----------------------------------------------------------------------
// <copyright file="DndItemDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>D&amp;D 5e item reference data (weapon, armor, potion, etc.).</summary>
public class DndItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DamageDice { get; set; }
    public string? DamageType { get; set; }
    public string? WeaponRange { get; set; }
    public int? ArmorClassBonus { get; set; }
    public decimal? Weight { get; set; }
    public decimal? CostGp { get; set; }
    public List<string> Properties { get; set; } = new();
    public string? HealingDice { get; set; }
}
