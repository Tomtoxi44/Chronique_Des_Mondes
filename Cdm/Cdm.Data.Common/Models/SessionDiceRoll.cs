// -----------------------------------------------------------------------
// <copyright file="SessionDiceRoll.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Represents a dice roll performed during a session.
/// Stored separately from chat messages to allow statistical queries on rolls.
/// </summary>
public class SessionDiceRoll
{
    /// <summary>Gets or sets the dice roll ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the chapter ID where the roll occurred.</summary>
    public int ChapterId { get; set; }

    /// <summary>Gets or sets the user ID of the player who rolled.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the display name of the roller at the time of rolling.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the dice type (e.g. "d20", "d6").</summary>
    public string DiceType { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of dice rolled.</summary>
    public int Count { get; set; }

    /// <summary>Gets or sets the individual die results, stored as a comma-separated string.</summary>
    public string Results { get; set; } = string.Empty;

    /// <summary>Gets or sets the modifier applied to the roll.</summary>
    public int Modifier { get; set; }

    /// <summary>Gets or sets the total result (sum of results + modifier).</summary>
    public int Total { get; set; }

    /// <summary>Gets or sets the optional reason for the roll (e.g. "Attack roll", "Perception check").</summary>
    public string? Reason { get; set; }

    /// <summary>Gets or sets when this roll occurred.</summary>
    public DateTime RolledAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the user navigation property.</summary>
    public virtual User User { get; set; } = null!;

    /// <summary>Gets or sets the chapter navigation property.</summary>
    public virtual Chapter Chapter { get; set; } = null!;
}
