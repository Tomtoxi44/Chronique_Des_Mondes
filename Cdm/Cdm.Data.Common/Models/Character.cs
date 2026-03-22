// -----------------------------------------------------------------------
// <copyright file="Character.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Represents a base character (minimal universal model).
/// </summary>
public class Character
{
    /// <summary>
    /// Gets or sets the character ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID who owns this character.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the character's name (last name).
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
    /// Gets or sets the character's avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the character is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the owner user.
    /// </summary>
    public virtual User Owner { get; set; } = null!;

    /// <summary>
    /// Gets or sets the character game profiles.
    /// </summary>
    public virtual ICollection<CharacterGameProfile> GameProfiles { get; set; } = new List<CharacterGameProfile>();
}
