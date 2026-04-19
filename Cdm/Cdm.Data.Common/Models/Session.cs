// -----------------------------------------------------------------------
// <copyright file="Session.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using Cdm.Common.Enums;

/// <summary>
/// Represents an active game session for a campaign.
/// </summary>
public class Session
{
    /// <summary>Gets or sets the session ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the campaign this session belongs to.</summary>
    public int CampaignId { get; set; }

    /// <summary>Gets or sets the user ID of the GM who started the session.</summary>
    public int StartedById { get; set; }

    /// <summary>Gets or sets when the session started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Gets or sets when the session ended (null if still active).</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>Gets or sets the session status.</summary>
    public SessionStatus Status { get; set; } = SessionStatus.Active;

    /// <summary>Gets or sets the ID of the chapter currently being played.</summary>
    public int? CurrentChapterId { get; set; }

    /// <summary>Gets or sets an optional welcome message shown to players.</summary>
    public string? WelcomeMessage { get; set; }

    /// <summary>Gets or sets the campaign navigation property.</summary>
    public virtual Campaign Campaign { get; set; } = null!;

    /// <summary>Gets or sets the GM user navigation property.</summary>
    public virtual User StartedBy { get; set; } = null!;

    /// <summary>Gets or sets the current chapter navigation property.</summary>
    public virtual Chapter? CurrentChapter { get; set; }

    /// <summary>Gets or sets the participants in this session.</summary>
    public virtual ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
}
