// -----------------------------------------------------------------------
// <copyright file="Combat.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Represents a combat encounter linked to a session.
/// </summary>
public class Combat
{
    /// <summary>Gets or sets the combat ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the session this combat belongs to.</summary>
    public int SessionId { get; set; }

    /// <summary>Gets or sets the optional chapter this combat is part of.</summary>
    public int? ChapterId { get; set; }

    /// <summary>Gets or sets the combat status (0=Setup, 1=Initiative, 2=Active, 3=Ended).</summary>
    public int Status { get; set; }

    /// <summary>Gets or sets the user ID of the GM who started the combat.</summary>
    public int StartedById { get; set; }

    /// <summary>Gets or sets when the combat started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Gets or sets when the combat ended (null if still active).</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>Gets or sets the winning side description.</summary>
    public string? VictorySide { get; set; }

    /// <summary>Gets or sets the current turn order index.</summary>
    public int CurrentTurnOrder { get; set; }

    /// <summary>Gets or sets when the combat record was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the session navigation property.</summary>
    public Session Session { get; set; } = null!;

    /// <summary>Gets or sets the combat groups.</summary>
    public ICollection<CombatGroup> Groups { get; set; } = new List<CombatGroup>();

    /// <summary>Gets or sets the combat participants.</summary>
    public ICollection<CombatParticipant> Participants { get; set; } = new List<CombatParticipant>();

    /// <summary>Gets or sets the combat action log.</summary>
    public ICollection<CombatAction> Actions { get; set; } = new List<CombatAction>();
}
