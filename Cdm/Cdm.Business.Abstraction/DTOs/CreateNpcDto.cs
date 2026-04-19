// -----------------------------------------------------------------------
// <copyright file="CreateNpcDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Data transfer object for creating or updating a Non-Player Character (NPC).
/// </summary>
public class CreateNpcDto
{
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
}
