// -----------------------------------------------------------------------
// <copyright file="CharacterGameProfile.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using Cdm.Common.Enums;

/// <summary>
/// Represents a character's profile in a specific game/campaign.
/// Contains game-specific data (D&D stats, Skyrim attributes, etc.) in JSON.
/// </summary>
public class CharacterGameProfile
{
    /// <summary>
    /// Gets or sets the profile ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the character ID.
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// Gets or sets the campaign ID.
    /// </summary>
    public int CampaignId { get; set; }

    /// <summary>
    /// Gets or sets the game type (Generic, D&D, Skyrim, etc.).
    /// </summary>
    public GameType GameType { get; set; }

    /// <summary>
    /// Gets or sets game-specific data stored as JSON.
    /// For D&D: {"Strength": 18, "Dexterity": 14, ...}
    /// For Skyrim: {"Health": 200, "Magicka": 150, ...}
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
    /// Gets or sets a value indicating whether this profile is active in the campaign.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets when the character joined this campaign.
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
    /// Gets or sets the campaign.
    /// </summary>
    public virtual Campaign Campaign { get; set; } = null!;
}
