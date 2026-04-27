// -----------------------------------------------------------------------
// <copyright file="DndItem.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a D&amp;D 5e item (weapon, armor, potion, tool, etc.).
/// </summary>
[Table("DndItems")]
public class DndItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Item category: Weapon, Armor, Potion, Tool, MagicItem, Other.</summary>
    [Required, MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Damage dice notation for weapons (e.g., "1d6", "2d4", "1d8"). Null for non-weapons.</summary>
    [MaxLength(20)]
    public string? DamageDice { get; set; }

    /// <summary>Damage type (Slashing, Piercing, Bludgeoning, Fire, etc.). Null for non-weapons.</summary>
    [MaxLength(50)]
    public string? DamageType { get; set; }

    /// <summary>Weapon range: Melee or Ranged. Null for non-weapons.</summary>
    [MaxLength(20)]
    public string? WeaponRange { get; set; }

    /// <summary>Armor Class bonus. Null for non-armor.</summary>
    public int? ArmorClassBonus { get; set; }

    /// <summary>Weight in lb.</summary>
    public decimal? Weight { get; set; }

    /// <summary>Cost in gold pieces.</summary>
    public decimal? CostGp { get; set; }

    /// <summary>Item properties (JSON array): ["Light","Finesse","Thrown"]</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Properties { get; set; }

    /// <summary>Healing dice for potions (e.g., "2d4+2"). Null for non-potions.</summary>
    [MaxLength(20)]
    public string? HealingDice { get; set; }

    public bool IsActive { get; set; } = true;
}
