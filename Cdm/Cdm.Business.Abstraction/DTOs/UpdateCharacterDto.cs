// -----------------------------------------------------------------------
// <copyright file="UpdateCharacterDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// DTO for updating an existing character.
/// </summary>
public class UpdateCharacterDto
{
    /// <summary>
    /// Gets or sets the character's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the character's description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the character's age.
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }
}
