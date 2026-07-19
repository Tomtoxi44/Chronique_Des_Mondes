// -----------------------------------------------------------------------
// <copyright file="SessionTradeDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System;
using Cdm.Common.Enums;

/// <summary>
/// DTO representing a theory-based object trade in a session.
/// </summary>
public class SessionTradeDto
{
    /// <summary>Gets or sets the trade ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the session ID.</summary>
    public int SessionId { get; set; }

    /// <summary>Gets or sets the proposer user ID.</summary>
    public int FromUserId { get; set; }

    /// <summary>Gets or sets the proposer display name.</summary>
    public string FromUserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient user ID.</summary>
    public int ToUserId { get; set; }

    /// <summary>Gets or sets the recipient display name.</summary>
    public string ToUserName { get; set; } = string.Empty;

    /// <summary>Gets or sets what is offered.</summary>
    public string OfferDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets what is requested in return.</summary>
    public string RequestDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status.</summary>
    public TradeStatus Status { get; set; }

    /// <summary>Gets or sets when the trade was proposed (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets when the trade was answered or cancelled (UTC), if any.</summary>
    public DateTime? RespondedAt { get; set; }
}
