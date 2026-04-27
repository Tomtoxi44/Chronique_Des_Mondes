// -----------------------------------------------------------------------
// <copyright file="CharacterDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Character data transfer object (base minimal model).
/// </summary>
public class CharacterDto
{
    /// <summary>
    /// Gets or sets the character ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public int UserId { get; set; }

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

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the character is locked (already in a world).
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a base character (template that can be used in multiple worlds).
    /// </summary>
    public bool IsBaseCharacter { get; set; }

    /// <summary>
    /// Gets or sets the source character ID for world copies (null for base characters).
    /// </summary>
    public int? SourceCharacterId { get; set; }
}
