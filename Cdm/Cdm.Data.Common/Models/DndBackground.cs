// -----------------------------------------------------------------------
// <copyright file="DndBackground.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a D&amp;D 5e character background.
/// </summary>
[Table("DndBackgrounds")]
public class DndBackground
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>JSON array: ["Acrobaties","Arcanes"] - 2 skill proficiencies</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? SkillProficiencies { get; set; }

    /// <summary>JSON array: ["Outil de voleur"] or null</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? ToolProficiencies { get; set; }

    /// <summary>Extra languages granted.</summary>
    [MaxLength(200)]
    public string? Languages { get; set; }

    /// <summary>Feature name (e.g. "Abri des fidèles").</summary>
    [MaxLength(200)]
    public string? Feature { get; set; }

    /// <summary>Feature description.</summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? FeatureDescription { get; set; }

    public bool IsActive { get; set; } = true;
}
