// -----------------------------------------------------------------------
// <copyright file="DndMonsterTemplate.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a D&amp;D 5e monster template that a GM can use as a starting point for NPCs.
/// </summary>
[Table("DndMonsterTemplates")]
public class DndMonsterTemplate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Monster type: Humanoid, Undead, Beast, Dragon, Fiend, etc.</summary>
    [MaxLength(50)]
    public string? MonsterType { get; set; }

    /// <summary>Challenge Rating (e.g., "1/4", "1", "5", "17").</summary>
    [MaxLength(10)]
    public string? ChallengeRating { get; set; }

    [MaxLength(2000)]
    [Column(TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

    public int HitPoints { get; set; }
    public string? HitDice { get; set; }
    public int ArmorClass { get; set; }
    public int Speed { get; set; } = 30;

    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    /// <summary>Typical actions/attacks as JSON: [{"name":"Longsword","attackBonus":5,"damageDice":"1d8+3","damageType":"Slashing"}]</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Actions { get; set; }

    /// <summary>Special abilities as JSON array: ["Pack Tactics","Darkvision 60ft"]</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? SpecialAbilities { get; set; }

    /// <summary>Alignment (e.g., "Chaotic Evil", "Neutral").</summary>
    [MaxLength(50)]
    public string? Alignment { get; set; }

    public bool IsActive { get; set; } = true;
}
