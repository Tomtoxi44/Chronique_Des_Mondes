// -----------------------------------------------------------------------
// <copyright file="DndSkill.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a D&amp;D 5e skill.
/// </summary>
[Table("DndSkills")]
public class DndSkill
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>English ability name for consistency with stat system: "Strength","Dexterity","Constitution","Intelligence","Wisdom","Charisma"</summary>
    [MaxLength(50)]
    public string LinkedAbility { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
