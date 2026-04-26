// -----------------------------------------------------------------------
// <copyright file="NpcDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Data transfer object for a Non-Player Character (NPC).
/// </summary>
public class NpcDto
{
    /// <summary>
    /// Gets or sets the NPC identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the chapter ID this NPC belongs to.
    /// </summary>
    public int ChapterId { get; set; }

    /// <summary>
    /// Gets or sets the NPC's last name (nullable).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the NPC's first name (nullable).
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the NPC's background or personality description (nullable).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the NPC's physical description (nullable).
    /// </summary>
    public string? PhysicalDescription { get; set; }

    /// <summary>
    /// Gets or sets the NPC's age (nullable).
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets game-specific data as a JSON string (e.g. D&amp;D 5e stat block).
    /// </summary>
    public string? GameSpecificData { get; set; }

    /// <summary>
    /// Gets the NPC's display name (FirstName + Name, or fallback to either, or "Inconnu").
    /// </summary>
    public string DisplayName
    {
        get
        {
            var parts = new[] { FirstName, Name }.Where(s => !string.IsNullOrWhiteSpace(s));
            var name = string.Join(" ", parts);
            return string.IsNullOrWhiteSpace(name) ? "Inconnu" : name;
        }
    }
}
