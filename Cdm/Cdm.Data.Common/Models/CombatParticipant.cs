// -----------------------------------------------------------------------
// <copyright file="CombatParticipant.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Represents a single combatant (player character or NPC) inside a combat encounter.
/// </summary>
public class CombatParticipant
{
    /// <summary>Gets or sets the participant ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the combat this participant belongs to.</summary>
    public int CombatId { get; set; }

    /// <summary>Gets or sets the group this participant belongs to.</summary>
    public int GroupId { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets whether this participant is a player character.</summary>
    public bool IsPlayerCharacter { get; set; }

    /// <summary>Gets or sets the optional FK to WorldCharacters (nullable).</summary>
    public int? CharacterId { get; set; }

    /// <summary>Gets or sets the optional FK to NonPlayerCharacters (nullable).</summary>
    public int? NpcId { get; set; }

    /// <summary>Gets or sets the current hit points.</summary>
    public int CurrentHp { get; set; }

    /// <summary>Gets or sets the maximum hit points.</summary>
    public int MaxHp { get; set; }

    /// <summary>Gets or sets the initiative value (null until rolled/declared).</summary>
    public int? Initiative { get; set; }

    /// <summary>Gets or sets the turn order position (assigned after initiative is sorted).</summary>
    public int TurnOrder { get; set; }

    /// <summary>Gets or sets whether this participant is still active in combat.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the user ID that controls this participant (player characters).</summary>
    public int? UserId { get; set; }

    /// <summary>Gets or sets the combat navigation property.</summary>
    public Combat Combat { get; set; } = null!;

    /// <summary>Gets or sets the group navigation property.</summary>
    public CombatGroup Group { get; set; } = null!;
}
