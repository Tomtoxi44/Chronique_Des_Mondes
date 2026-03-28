// -----------------------------------------------------------------------
// <copyright file="WorldCharacterDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for a WorldCharacter (character adapted to a specific world).
/// </summary>
public class WorldCharacterDto
{
    /// <summary>
    /// Gets or sets the world character identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the base character ID.
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// Gets or sets the base character name.
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the world ID this character belongs to.
    /// </summary>
    public int WorldId { get; set; }

    /// <summary>
    /// Gets or sets the world name.
    /// </summary>
    public string WorldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the game type of the world.
    /// </summary>
    public GameType GameType { get; set; }

    /// <summary>
    /// Gets or sets game-specific data stored as JSON.
    /// </summary>
    public string? GameSpecificData { get; set; }

    /// <summary>
    /// Gets or sets the character level (if applicable for this game).
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// Gets or sets the current health points.
    /// </summary>
    public int? CurrentHealth { get; set; }

    /// <summary>
    /// Gets or sets the maximum health points.
    /// </summary>
    public int? MaxHealth { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this character is active in the world.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets when the character joined this world.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID who owns the base character.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL of the base character.
    /// </summary>
    public string? AvatarUrl { get; set; }
}
