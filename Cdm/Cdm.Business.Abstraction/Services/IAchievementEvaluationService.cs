// -----------------------------------------------------------------------
// <copyright file="IAchievementEvaluationService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Threading.Tasks;

/// <summary>
/// Evaluates automatic achievement conditions when gameplay events occur and
/// awards any matching, not-yet-unlocked achievements in scope. All methods are
/// best-effort: a failure to evaluate must never break the triggering action.
/// </summary>
public interface IAchievementEvaluationService
{
    /// <summary>
    /// Evaluates dice-related conditions after a player shares a roll in a session
    /// (critical / fumble on a d20, and cumulative dice count).
    /// </summary>
    /// <param name="userId">The rolling user.</param>
    /// <param name="sessionId">The session the roll happened in.</param>
    /// <param name="diceType">The dice type (e.g. "D20").</param>
    /// <param name="results">The individual die results.</param>
    Task OnDiceRolledAsync(int userId, int sessionId, string diceType, int[] results);

    /// <summary>
    /// Evaluates session-attendance conditions when a player joins a session.
    /// </summary>
    /// <param name="userId">The joining user.</param>
    /// <param name="sessionId">The session joined.</param>
    Task OnSessionAttendedAsync(int userId, int sessionId);

    /// <summary>
    /// Evaluates trade conditions when an object trade is accepted.
    /// </summary>
    /// <param name="userId">A user party to the accepted trade.</param>
    /// <param name="sessionId">The session the trade belongs to.</param>
    Task OnTradeAcceptedAsync(int userId, int sessionId);

    /// <summary>
    /// Evaluates combat conditions when a combat ends (won / survived), for every
    /// player that took part in it.
    /// </summary>
    /// <param name="combatId">The combat that just ended.</param>
    Task OnCombatEndedAsync(int combatId);
}
