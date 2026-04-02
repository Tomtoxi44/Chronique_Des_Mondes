// -----------------------------------------------------------------------
// <copyright file="WorldCharacter.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using Cdm.Common.Enums;

/// <summary>
/// Represents a character adapted to a specific world.
/// This is a copy of the base character with world-specific rules applied.
/// Contains world-specific data (D&D stats, Skyrim attributes, etc.) in JSON.
/// </summary>
public class WorldCharacter
{
    /// <summary>
    /// Gets or sets the world character ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the base character ID.
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// Gets or sets the world ID this character belongs to.
    /// </summary>
    public int WorldId { get; set; }

    /// <summary>
    /// Gets or sets game-specific data stored as JSON.
    /// For D&D: {"Strength": 18, "Dexterity": 14, "Class": "Fighter", "Race": "Human", ...}
    /// For Skyrim: {"Health": 200, "Magicka": 150, "Stamina": 180, "Race": "Nord", ...}
    /// For Custom: Free-form data defined by the GM.
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
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets when the character joined this world.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the base character.
    /// </summary>
    public virtual Character Character { get; set; } = null!;

    /// <summary>
    /// Gets or sets the world this character belongs to.
    /// </summary>
    public virtual World World { get; set; } = null!;
}
