namespace Cdm.Common.Enums;

/// <summary>
/// Campaign status enumeration
/// </summary>
public enum CampaignStatus
{
    /// <summary>
    /// Campaign is in planning phase (not started yet)
    /// </summary>
    Planning = 0,

    /// <summary>
    /// Campaign is currently active
    /// </summary>
    Active = 1,

    /// <summary>
    /// Campaign is temporarily on hold
    /// </summary>
    OnHold = 2,

    /// <summary>
    /// Campaign has been completed
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Campaign has been cancelled
    /// </summary>
    Cancelled = 4
}
