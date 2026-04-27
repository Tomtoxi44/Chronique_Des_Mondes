// -----------------------------------------------------------------------
// <copyright file="DndRace.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a D&amp;D 5e playable race with its stat bonuses.
/// </summary>
[Table("DndRaces")]
public class DndRace
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Base walking speed in feet.</summary>
    public int Speed { get; set; } = 30;

    /// <summary>Stat bonuses stored as JSON: {"Strength":2,"Dexterity":1}</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? StatBonuses { get; set; }

    /// <summary>Racial traits stored as JSON array: ["Darkvision","Fey Ancestry"]</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Traits { get; set; }

    /// <summary>Available subraces stored as JSON array: ["High Elf","Wood Elf","Dark Elf"]</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Subraces { get; set; }

    /// <summary>Stat bonuses per subrace as JSON: {"High Elf":{"Intelligence":1}}</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? SubraceStatBonuses { get; set; }

    public bool IsActive { get; set; } = true;
}
