// -----------------------------------------------------------------------
// <copyright file="DndSpell.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a D&amp;D 5e spell.
/// </summary>
[Table("DndSpells")]
public class DndSpell
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Spell level (0 = cantrip, 1-9).</summary>
    public int Level { get; set; }

    /// <summary>Spell school: Abjuration, Conjuration, Divination, Enchantment, Evocation, Illusion, Necromancy, Transmutation.</summary>
    [MaxLength(50)]
    public string? School { get; set; }

    [MaxLength(2000)]
    [Column(TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

    /// <summary>Casting time (e.g., "1 action", "1 bonus action", "1 reaction").</summary>
    [MaxLength(100)]
    public string? CastingTime { get; set; }

    /// <summary>Spell range (e.g., "Self", "60 feet", "Touch").</summary>
    [MaxLength(50)]
    public string? Range { get; set; }

    /// <summary>Duration (e.g., "Instantaneous", "1 minute", "Concentration, up to 1 hour").</summary>
    [MaxLength(100)]
    public string? Duration { get; set; }

    /// <summary>Damage or healing dice (e.g., "8d6", "2d8+4"). Null if no dice.</summary>
    [MaxLength(30)]
    public string? DamageDice { get; set; }

    /// <summary>Damage type (Fire, Cold, Radiant, etc.). Null if none.</summary>
    [MaxLength(50)]
    public string? DamageType { get; set; }

    /// <summary>Classes that can use this spell (JSON array): ["Wizard","Sorcerer"]</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Classes { get; set; }

    /// <summary>Spell components: V (verbal), S (somatic), M (material), or combinations.</summary>
    [MaxLength(10)]
    public string? Components { get; set; }

    /// <summary>Whether the spell requires concentration.</summary>
    public bool RequiresConcentration { get; set; }

    /// <summary>Whether the spell is a ritual.</summary>
    public bool IsRitual { get; set; }

    public bool IsActive { get; set; } = true;
}
