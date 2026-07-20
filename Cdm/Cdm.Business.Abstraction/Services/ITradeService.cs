// -----------------------------------------------------------------------
// <copyright file="ITradeService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing theory-based object trades within a session.
/// </summary>
public interface ITradeService
{
    /// <summary>
    /// Proposes a trade from one session member to another. The proposer and the
    /// recipient must both belong to the session (as GM or participant).
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="fromUserId">The proposer user identifier.</param>
    /// <param name="toUserId">The recipient user identifier.</param>
    /// <param name="offerDescription">Description of what is offered.</param>
    /// <param name="requestDescription">Description of what is requested in return.</param>
    /// <returns>The created trade DTO, or null if invalid/unauthorized.</returns>
    Task<SessionTradeDto?> ProposeTradeAsync(int sessionId, int fromUserId, int toUserId, string offerDescription, string requestDescription);

    /// <summary>
    /// Responds to a pending trade. Only the recipient may accept or decline it.
    /// </summary>
    /// <param name="tradeId">The trade identifier.</param>
    /// <param name="userId">The responding user identifier (must be the recipient).</param>
    /// <param name="accept"><c>true</c> to accept, <c>false</c> to decline.</param>
    /// <returns>The updated trade DTO, or null if invalid/unauthorized/already answered.</returns>
    Task<SessionTradeDto?> RespondToTradeAsync(int tradeId, int userId, bool accept);

    /// <summary>
    /// Cancels a pending trade. Only the proposer may cancel it.
    /// </summary>
    /// <param name="tradeId">The trade identifier.</param>
    /// <param name="userId">The cancelling user identifier (must be the proposer).</param>
    /// <returns>The updated trade DTO, or null if invalid/unauthorized/already answered.</returns>
    Task<SessionTradeDto?> CancelTradeAsync(int tradeId, int userId);

    /// <summary>
    /// Gets the pending trades of a session. Only the GM or a participant can access them.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="userId">The requesting user identifier.</param>
    /// <returns>The pending trades, or null if not found/unauthorized.</returns>
    Task<IReadOnlyList<SessionTradeDto>?> GetPendingTradesAsync(int sessionId, int userId);
}
