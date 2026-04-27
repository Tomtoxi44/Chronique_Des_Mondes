// -----------------------------------------------------------------------
// <copyright file="CombatAction.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Represents a logged action that occurred during a combat encounter.
/// </summary>
public class CombatAction
{
    /// <summary>Gets or sets the action ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the combat this action belongs to.</summary>
    public int CombatId { get; set; }

    /// <summary>Gets or sets the name of the participant who performed the action.</summary>
    public string ParticipantName { get; set; } = string.Empty;

    /// <summary>Gets or sets the action type (e.g. "dice", "text", "pass", "damage", "hp_update").</summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional free-text description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the dice expression (e.g. "1d20+5").</summary>
    public string? DiceExpression { get; set; }

    /// <summary>Gets or sets the numeric result of a dice roll.</summary>
    public int? DiceResult { get; set; }

    /// <summary>Gets or sets whether this action is only visible to the GM.</summary>
    public bool IsPrivate { get; set; }

    /// <summary>Gets or sets the user ID who performed the action.</summary>
    public int? PerformedByUserId { get; set; }

    /// <summary>Gets or sets when the action was recorded.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the combat navigation property.</summary>
    public Combat Combat { get; set; } = null!;
}
