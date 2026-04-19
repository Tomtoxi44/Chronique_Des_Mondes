namespace Cdm.Business.Abstraction.DTOs;

using System;
using System.Collections.Generic;
using Cdm.Common.Enums;

/// <summary>
/// DTO representing a game session.
/// </summary>
public class SessionDto
{
    /// <summary>Gets or sets the session ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the campaign ID.</summary>
    public int CampaignId { get; set; }

    /// <summary>Gets or sets the campaign name.</summary>
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>Gets or sets the world ID.</summary>
    public int WorldId { get; set; }

    /// <summary>Gets or sets the GM user ID.</summary>
    public int StartedById { get; set; }

    /// <summary>Gets or sets the GM display name.</summary>
    public string StartedByName { get; set; } = string.Empty;

    /// <summary>Gets or sets when the session started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Gets or sets when the session ended.</summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>Gets or sets the session status.</summary>
    public SessionStatus Status { get; set; }

    /// <summary>Gets or sets the current chapter ID.</summary>
    public int? CurrentChapterId { get; set; }

    /// <summary>Gets or sets the current chapter title.</summary>
    public string? CurrentChapterTitle { get; set; }

    /// <summary>Gets or sets the welcome message.</summary>
    public string? WelcomeMessage { get; set; }

    /// <summary>Gets or sets the list of participants.</summary>
    public IEnumerable<SessionParticipantDto> Participants { get; set; } = [];
}

/// <summary>
/// DTO for starting a new session.
/// </summary>
public class StartSessionDto
{
    /// <summary>Gets or sets the campaign ID.</summary>
    public int CampaignId { get; set; }

    /// <summary>Gets or sets an optional welcome message.</summary>
    public string? WelcomeMessage { get; set; }

    /// <summary>Gets or sets the world character IDs to invite.</summary>
    public List<int> WorldCharacterIds { get; set; } = [];
}

/// <summary>
/// DTO representing a session participant.
/// </summary>
public class SessionParticipantDto
{
    /// <summary>Gets or sets the participant record ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the session ID.</summary>
    public int SessionId { get; set; }

    /// <summary>Gets or sets the world character ID.</summary>
    public int WorldCharacterId { get; set; }

    /// <summary>Gets or sets the character display name.</summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>Gets or sets the player user ID.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the player display name.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets when the participant joined.</summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>Gets or sets the participant status.</summary>
    public SessionParticipantStatus Status { get; set; }
}
