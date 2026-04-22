// -----------------------------------------------------------------------
// <copyright file="DndClass.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a D&amp;D 5e character class.
/// </summary>
[Table("DndClasses")]
public class DndClass
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Hit die type (6, 8, 10, 12).</summary>
    public int HitDie { get; set; }

    /// <summary>Whether this class has spellcasting abilities.</summary>
    public bool IsSpellcaster { get; set; }

    /// <summary>Primary ability scores for this class (JSON array): ["Strength","Constitution"]</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? PrimaryAbilities { get; set; }

    /// <summary>Saving throw proficiencies (JSON array): ["Strength","Constitution"]</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? SavingThrows { get; set; }

    /// <summary>Available subclasses (JSON array): ["Champion","Battle Master","Eldritch Knight"]</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Subclasses { get; set; }

    public bool IsActive { get; set; } = true;
}
