// -----------------------------------------------------------------------
// <copyright file="SessionParticipant.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using Cdm.Common.Enums;

/// <summary>
/// Represents a player participating in a game session.
/// </summary>
public class SessionParticipant
{
    /// <summary>Gets or sets the participant ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the session ID.</summary>
    public int SessionId { get; set; }

    /// <summary>Gets or sets the world character ID of this participant.</summary>
    public int WorldCharacterId { get; set; }

    /// <summary>Gets or sets when the participant joined.</summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>Gets or sets the participant status.</summary>
    public SessionParticipantStatus Status { get; set; } = SessionParticipantStatus.Invited;

    /// <summary>Gets or sets the session navigation property.</summary>
    public virtual Session Session { get; set; } = null!;

    /// <summary>Gets or sets the world character navigation property.</summary>
    public virtual WorldCharacter WorldCharacter { get; set; } = null!;
}
