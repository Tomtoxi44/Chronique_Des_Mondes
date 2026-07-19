// -----------------------------------------------------------------------
// <copyright file="IStatisticsService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service producing gameplay statistics and reports.
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Computes aggregated dice-roll statistics for a user from their persisted session rolls.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The dice statistics (empty stats if the user has never rolled).</returns>
    Task<DiceStatsDto> GetDiceStatsForUserAsync(int userId);
}
