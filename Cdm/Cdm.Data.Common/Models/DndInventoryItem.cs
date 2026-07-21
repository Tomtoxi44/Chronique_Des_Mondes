// -----------------------------------------------------------------------
// <copyright file="DndInventoryItem.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cdm.Common.Enums;

/// <summary>
/// A world character's inventory item. Despite the historical <c>DndInventoryItems</c> table name,
/// this is the <b>unified</b> inventory store for every game system: <see cref="GameType"/> tells
/// which system an item belongs to. The D&amp;D combat columns (<see cref="AttackBonus"/>,
/// <see cref="DamageDice"/>, <see cref="DamageType"/>, <see cref="DndItemId"/>) are only meaningful
/// when <see cref="GameType"/> is <see cref="Cdm.Common.Enums.GameType.DnD5e"/>; other systems put
/// their own structured stats in <see cref="GameSpecificData"/>. A <see cref="Cdm.Common.Enums.GameType.Generic"/>
/// item carries no rules and can sit on any character.
/// </summary>
[Table("DndInventoryItems")]
public class DndInventoryItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int WorldCharacterId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    /// <summary>The game system this item belongs to (Generic = no rules, usable anywhere).</summary>
    public GameType GameType { get; set; } = GameType.Generic;

    /// <summary>Optional theme-specific structured stats (JSON) for systems other than D&amp;D.</summary>
    public string? GameSpecificData { get; set; }

    /// <summary>Optional image URL.</summary>
    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    public int? AttackBonus { get; set; }

    [MaxLength(20)]
    public string? DamageDice { get; set; }

    [MaxLength(50)]
    public string? DamageType { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>Optional reference to the D&amp;D item template.</summary>
    public int? DndItemId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(WorldCharacterId))]
    public virtual WorldCharacter WorldCharacter { get; set; } = null!;

    [ForeignKey(nameof(DndItemId))]
    public virtual DndItem? DndItem { get; set; }
}
