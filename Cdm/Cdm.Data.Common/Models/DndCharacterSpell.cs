// -----------------------------------------------------------------------
// <copyright file="DndCharacterSpell.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>A spell known/prepared by a D&amp;D 5e character in a specific world.</summary>
[Table("DndCharacterSpells")]
public class DndCharacterSpell
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int WorldCharacterId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int Level { get; set; }

    [MaxLength(50)]
    public string? School { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? DamageDice { get; set; }

    [MaxLength(50)]
    public string? DamageType { get; set; }

    public bool IsPrepared { get; set; }

    /// <summary>Optional reference to the D&amp;D spell template.</summary>
    public int? DndSpellId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(WorldCharacterId))]
    public virtual WorldCharacter WorldCharacter { get; set; } = null!;

    [ForeignKey(nameof(DndSpellId))]
    public virtual DndSpell? DndSpell { get; set; }
}
