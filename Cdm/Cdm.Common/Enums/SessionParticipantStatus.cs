namespace Cdm.Common.Enums;

/// <summary>
/// Status of a session participant.
/// </summary>
public enum SessionParticipantStatus
{
    /// <summary>Participant has been invited but not yet joined.</summary>
    Invited = 0,

    /// <summary>Participant has joined the session.</summary>
    Joined = 1,

    /// <summary>Participant has left the session.</summary>
    Left = 2
}
