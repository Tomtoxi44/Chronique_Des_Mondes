namespace Cdm.Common.Enums;

/// <summary>
/// Status of a game session.
/// </summary>
public enum SessionStatus
{
    /// <summary>Session is scheduled but not yet started.</summary>
    Scheduled = 0,

    /// <summary>Session is currently active.</summary>
    Active = 1,

    /// <summary>Session is paused.</summary>
    Paused = 2,

    /// <summary>Session has ended normally.</summary>
    Ended = 3,

    /// <summary>Session was cancelled.</summary>
    Cancelled = 4
}
