// -----------------------------------------------------------------------
// <copyright file="CreateCharacterDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// DTO for creating a new character (minimal/generic).
/// </summary>
public class CreateCharacterDto
{
    /// <summary>
    /// Gets or sets the character's name (required).
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
