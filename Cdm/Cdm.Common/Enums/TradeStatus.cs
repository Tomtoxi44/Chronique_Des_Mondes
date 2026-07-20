namespace Cdm.Common.Enums;

/// <summary>
/// Status of a theory-based object trade between two session members.
/// </summary>
public enum TradeStatus
{
    /// <summary>The trade has been proposed and awaits a response.</summary>
    Pending = 0,

    /// <summary>The recipient accepted the trade.</summary>
    Accepted = 1,

    /// <summary>The recipient declined the trade.</summary>
    Declined = 2,

    /// <summary>The proposer cancelled the trade before it was answered.</summary>
    Cancelled = 3
}
