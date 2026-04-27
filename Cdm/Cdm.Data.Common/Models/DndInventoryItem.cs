// -----------------------------------------------------------------------
// <copyright file="DndInventoryItem.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>An item in a D&amp;D 5e character's inventory (world-specific).</summary>
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
