// -----------------------------------------------------------------------
// <copyright file="NonPlayerCharacter.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a Non-Player Character (NPC) associated with a chapter.
/// NPCs are visible only to the Game Master.
/// </summary>
[Table("NonPlayerCharacters")]
public class NonPlayerCharacter
{
    /// <summary>
    /// Gets or sets the NPC unique identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the chapter ID this NPC belongs to.
    /// </summary>
    [Required]
    public int ChapterId { get; set; }

    /// <summary>
    /// Gets or sets the NPC's last name (nullable).
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the NPC's first name (nullable).
    /// </summary>
    [MaxLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the NPC's background or personality description (nullable).
    /// </summary>
    [MaxLength(2000)]
    [Column(TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the NPC's physical description (nullable).
    /// </summary>
    [MaxLength(2000)]
    [Column(TypeName = "nvarchar(max)")]
    public string? PhysicalDescription { get; set; }

    /// <summary>
    /// Gets or sets the NPC's age (nullable).
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the NPC is active (soft delete).
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp (UTC).
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update timestamp (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the chapter.
    /// </summary>
    [ForeignKey(nameof(ChapterId))]
    public virtual Chapter Chapter { get; set; } = null!;
}
