// -----------------------------------------------------------------------
// <copyright file="ISessionService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing game sessions.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Starts a new session for a campaign. Only the campaign GM can do this.
    /// </summary>
    /// <param name="dto">The session start data.</param>
    /// <param name="userId">The GM user identifier.</param>
    /// <returns>The created session DTO, or null if failed/unauthorized.</returns>
    Task<SessionDto?> StartSessionAsync(StartSessionDto dto, int userId);

    /// <summary>
    /// Gets a session by its ID. Only participants and the GM can access it.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="userId">The requesting user identifier.</param>
    /// <returns>The session DTO, or null if not found/unauthorized.</returns>
    Task<SessionDto?> GetSessionAsync(int sessionId, int userId);

    /// <summary>
    /// Gets the active session for a campaign, if any.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <param name="userId">The requesting user identifier.</param>
    /// <returns>The active session DTO, or null if none.</returns>
    Task<SessionDto?> GetActiveSessionByCampaignAsync(int campaignId, int userId);

    /// <summary>
    /// Gets all sessions the user has participated in (as GM or player).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>List of session DTOs.</returns>
    Task<IEnumerable<SessionDto>> GetMySessionsAsync(int userId);

    /// <summary>
    /// Ends a session. Only the GM who started it can end it.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="userId">The GM user identifier.</param>
    /// <returns>True if ended successfully, false otherwise.</returns>
    Task<bool> EndSessionAsync(int sessionId, int userId);

    /// <summary>
    /// Marks a participant as joined (player self-join action).
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="worldCharacterId">The world character ID of the player.</param>
    /// <param name="userId">The player user identifier.</param>
    /// <returns>The updated session DTO, or null if failed.</returns>
    Task<SessionDto?> JoinSessionAsync(int sessionId, int worldCharacterId, int userId);

    /// <summary>
    /// Updates the current chapter being played. Only the GM can do this.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="chapterId">The new current chapter ID (null to clear).</param>
    /// <param name="userId">The GM user identifier.</param>
    /// <returns>The updated session DTO, or null if failed.</returns>
    Task<SessionDto?> UpdateCurrentChapterAsync(int sessionId, int? chapterId, int userId);

    /// <summary>
    /// Marks the player as having left the session voluntarily.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="userId">The player user identifier.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> LeaveSessionAsync(int sessionId, int userId);
}
