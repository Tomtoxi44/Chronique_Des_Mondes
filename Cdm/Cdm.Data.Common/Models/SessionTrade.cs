// -----------------------------------------------------------------------
// <copyright file="SessionTrade.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using Cdm.Common.Enums;

/// <summary>
/// Represents a theory-based object trade proposed during a session
/// (GM → player or player → player). The exchange is described in text;
/// the record tracks its lifecycle so pending proposals survive reconnection.
/// </summary>
public class SessionTrade
{
    /// <summary>Gets or sets the trade ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the session this trade belongs to.</summary>
    public int SessionId { get; set; }

    /// <summary>Gets or sets the user ID of the proposer.</summary>
    public int FromUserId { get; set; }

    /// <summary>Gets or sets the display name of the proposer at proposal time.</summary>
    public string FromUserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user ID of the recipient.</summary>
    public int ToUserId { get; set; }

    /// <summary>Gets or sets the display name of the recipient at proposal time.</summary>
    public string ToUserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of what is offered.</summary>
    public string OfferDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of what is requested in return.</summary>
    public string RequestDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status of the trade.</summary>
    public TradeStatus Status { get; set; } = TradeStatus.Pending;

    /// <summary>Gets or sets when the trade was proposed (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets when the trade was answered or cancelled (UTC), if applicable.</summary>
    public DateTime? RespondedAt { get; set; }

    /// <summary>Gets or sets the session navigation property.</summary>
    public virtual Session Session { get; set; } = null!;
}
