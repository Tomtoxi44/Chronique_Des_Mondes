// -----------------------------------------------------------------------
// <copyright file="StatisticsService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Produces gameplay statistics from persisted session data.
/// </summary>
public class StatisticsService(AppDbContext dbContext, ILogger<StatisticsService> logger) : IStatisticsService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<StatisticsService> logger = logger;

    /// <inheritdoc/>
    public async Task<DiceStatsDto> GetDiceStatsForUserAsync(int userId)
    {
        var stats = new DiceStatsDto();

        try
        {
            var rolls = await this.dbContext.SessionDiceRolls
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .Select(r => new { r.SessionId, r.DiceType, r.Results })
                .ToListAsync();

            if (rolls.Count == 0)
            {
                return stats;
            }

            stats.TotalRolls = rolls.Count;
            stats.SessionsWithRolls = rolls.Select(r => r.SessionId).Distinct().Count();

            long overallSum = 0;
            var perType = new Dictionary<string, (int Count, long Sum)>(StringComparer.OrdinalIgnoreCase);

            foreach (var roll in rolls)
            {
                var faces = ParseResults(roll.Results);
                if (faces.Length == 0)
                {
                    continue;
                }

                var type = NormalizeDiceType(roll.DiceType);
                stats.TotalDiceThrown += faces.Length;
                overallSum += faces.Sum();

                var existing = perType.TryGetValue(type, out var acc) ? acc : (0, 0L);
                perType[type] = (existing.Item1 + faces.Length, existing.Item2 + faces.Sum());

                if (string.Equals(type, "D20", StringComparison.OrdinalIgnoreCase))
                {
                    stats.D20Count += faces.Length;
                    stats.Natural20Count += faces.Count(f => f == 20);
                    stats.Natural1Count += faces.Count(f => f == 1);
                }
            }

            stats.OverallAverage = stats.TotalDiceThrown > 0
                ? Math.Round((double)overallSum / stats.TotalDiceThrown, 2)
                : 0;

            stats.PerDiceType = perType
                .Select(kv => new DiceTypeStatDto
                {
                    DiceType = kv.Key,
                    DiceThrown = kv.Value.Count,
                    Average = kv.Value.Count > 0 ? Math.Round((double)kv.Value.Sum / kv.Value.Count, 2) : 0
                })
                .OrderByDescending(t => t.DiceThrown)
                .ToList();

            return stats;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error computing dice statistics for user {UserId}", userId);
            return stats;
        }
    }

    /// <summary>
    /// Parses the comma-separated individual die results, ignoring malformed entries.
    /// </summary>
    private static int[] ParseResults(string? results)
    {
        if (string.IsNullOrWhiteSpace(results))
        {
            return [];
        }

        return results
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => int.TryParse(part, out var value) ? value : (int?)null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToArray();
    }

    /// <inheritdoc/>
    public async Task<ParticipationStatsDto> GetParticipationStatsForUserAsync(int userId)
    {
        var stats = new ParticipationStatsDto();

        try
        {
            var gmSessionIds = await this.dbContext.Sessions
                .Where(s => s.StartedById == userId)
                .Select(s => s.Id)
                .ToListAsync();

            var playerSessionIds = await this.dbContext.SessionParticipants
                .Where(p => p.WorldCharacter.Character.UserId == userId)
                .Select(p => p.SessionId)
                .Distinct()
                .ToListAsync();

            var involvedIds = gmSessionIds.Union(playerSessionIds).ToList();
            if (involvedIds.Count == 0)
            {
                return stats;
            }

            var sessions = await this.dbContext.Sessions
                .Where(s => involvedIds.Contains(s.Id))
                .Select(s => new
                {
                    s.CampaignId,
                    s.StartedAt,
                    s.EndedAt,
                    ParticipantCount = s.Participants.Count,
                })
                .ToListAsync();

            var gmSet = gmSessionIds.ToHashSet();
            stats.SessionsAsGm = gmSessionIds.Count;
            stats.SessionsAsPlayer = playerSessionIds.Count(id => !gmSet.Contains(id));
            stats.TotalSessions = involvedIds.Count;
            stats.CampaignsPlayed = sessions.Select(s => s.CampaignId).Distinct().Count();
            stats.AverageGroupSize = sessions.Count > 0
                ? Math.Round(sessions.Average(s => s.ParticipantCount), 1)
                : 0;
            stats.TotalHoursPlayed = Math.Round(
                sessions.Where(s => s.EndedAt.HasValue)
                        .Sum(s => Math.Max(0, (s.EndedAt!.Value - s.StartedAt).TotalHours)),
                1);

            stats.ByMonth = sessions
                .GroupBy(s => new { s.StartedAt.Year, s.StartedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyActivityDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Sessions = g.Count(),
                    Hours = Math.Round(
                        g.Where(s => s.EndedAt.HasValue)
                         .Sum(s => Math.Max(0, (s.EndedAt!.Value - s.StartedAt).TotalHours)),
                        1),
                })
                .ToList();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error computing participation stats for user {UserId}", userId);
        }

        return stats;
    }

    /// <summary>
    /// Normalizes a dice type to the "D&lt;faces&gt;" upper-case form (e.g. "d20" → "D20").
    /// </summary>
    private static string NormalizeDiceType(string? diceType)
    {
        if (string.IsNullOrWhiteSpace(diceType))
        {
            return "?";
        }

        var trimmed = diceType.Trim().ToUpperInvariant();
        return trimmed.StartsWith('D') ? trimmed : $"D{trimmed}";
    }
}
