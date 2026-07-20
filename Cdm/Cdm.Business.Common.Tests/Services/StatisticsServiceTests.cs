// -----------------------------------------------------------------------
// <copyright file="StatisticsServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Common.Services;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="StatisticsService"/>.
/// </summary>
public class StatisticsServiceTests
{
    private readonly Mock<ILogger<StatisticsService>> loggerMock = new();

    private static DbContextOptions<AppDbContext> NewOptions(string name) =>
        new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase($"{name}-{System.Guid.NewGuid()}").Options;

    private StatisticsService CreateService(AppDbContext context) => new(context, this.loggerMock.Object);

    private static SessionDiceRoll Roll(int userId, int sessionId, string diceType, string results) => new()
    {
        UserId = userId,
        SessionId = sessionId,
        UserName = "Joueur",
        DiceType = diceType,
        Count = results.Split(',').Length,
        Results = results,
        Total = 0,
        RolledAt = DateTime.UtcNow
    };

    /// <summary>
    /// A user with no rolls gets empty statistics.
    /// </summary>
    [Fact]
    public async Task GetDiceStatsForUserAsync_NoRolls_ReturnsEmpty()
    {
        using var context = new AppDbContext(NewOptions(nameof(GetDiceStatsForUserAsync_NoRolls_ReturnsEmpty)));
        var service = this.CreateService(context);

        var stats = await service.GetDiceStatsForUserAsync(1);

        Assert.Equal(0, stats.TotalRolls);
        Assert.Equal(0, stats.TotalDiceThrown);
        Assert.Equal(0, stats.OverallAverage);
        Assert.Equal(0, stats.CritRate);
    }

    /// <summary>
    /// Dice statistics aggregate counts, average, natural 20/1 on d20, sessions and per-type breakdown,
    /// and ignore other users' rolls.
    /// </summary>
    [Fact]
    public async Task GetDiceStatsForUserAsync_AggregatesCorrectly()
    {
        using var context = new AppDbContext(NewOptions(nameof(GetDiceStatsForUserAsync_AggregatesCorrectly)));

        // User 1: two d20 rolls (20, 1) across two sessions + one 2d6 (3,5). User 2's roll must be ignored.
        context.SessionDiceRolls.AddRange(
            Roll(1, 10, "D20", "20"),
            Roll(1, 11, "D20", "1"),
            Roll(1, 10, "D6", "3,5"),
            Roll(2, 10, "D20", "20"));
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        var stats = await service.GetDiceStatsForUserAsync(1);

        Assert.Equal(3, stats.TotalRolls);           // three roll rows for user 1
        Assert.Equal(4, stats.TotalDiceThrown);      // 1 + 1 + 2
        Assert.Equal(2, stats.D20Count);
        Assert.Equal(1, stats.Natural20Count);
        Assert.Equal(1, stats.Natural1Count);
        Assert.Equal(50.0, stats.CritRate);          // 1 of 2 d20
        Assert.Equal(50.0, stats.FumbleRate);
        Assert.Equal(2, stats.SessionsWithRolls);    // sessions 10 and 11
        // Overall average over faces 20,1,3,5 = 29/4 = 7.25
        Assert.Equal(7.25, stats.OverallAverage);
        // Per-type: D20 (2 dice) and D6 (2 dice)
        Assert.Contains(stats.PerDiceType, t => t.DiceType == "D20" && t.DiceThrown == 2);
        Assert.Contains(stats.PerDiceType, t => t.DiceType == "D6" && t.DiceThrown == 2 && t.Average == 4.0);
    }

    /// <summary>
    /// Dice types are normalised so "d20" and "D20" aggregate together.
    /// </summary>
    [Fact]
    public async Task GetDiceStatsForUserAsync_NormalisesDiceTypeCase()
    {
        using var context = new AppDbContext(NewOptions(nameof(GetDiceStatsForUserAsync_NormalisesDiceTypeCase)));
        context.SessionDiceRolls.AddRange(
            Roll(1, 10, "d20", "15"),
            Roll(1, 10, "D20", "20"));
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        var stats = await service.GetDiceStatsForUserAsync(1);

        Assert.Single(stats.PerDiceType);
        Assert.Equal("D20", stats.PerDiceType[0].DiceType);
        Assert.Equal(2, stats.D20Count);
        Assert.Equal(1, stats.Natural20Count);
    }

    // ── Participation stats ──────────────────────────────────────────────

    /// <summary>Seeds a session run by <paramref name="gmUserId"/> with the given players as participants.</summary>
    private static void SeedSession(
        AppDbContext ctx, int sessionId, int gmUserId, DateTime startedAt, DateTime? endedAt,
        int campaignId, params int[] playerUserIds)
    {
        EnsureUser(ctx, gmUserId);
        ctx.Sessions.Add(new Session
        {
            Id = sessionId,
            CampaignId = campaignId,
            StartedById = gmUserId,
            StartedAt = startedAt,
            EndedAt = endedAt,
        });

        foreach (var uid in playerUserIds)
        {
            EnsureUser(ctx, uid);
            var charId = (uid * 1000) + sessionId;
            var wcId = (uid * 1000) + sessionId;
            ctx.Characters.Add(new Character { Id = charId, UserId = uid, Name = "C", IsActive = true, CreatedAt = DateTime.UtcNow });
            ctx.WorldCharacters.Add(new WorldCharacter { Id = wcId, CharacterId = charId, WorldId = 1, JoinedAt = DateTime.UtcNow });
            ctx.SessionParticipants.Add(new SessionParticipant { Id = wcId, SessionId = sessionId, WorldCharacterId = wcId, JoinedAt = DateTime.UtcNow, Status = SessionParticipantStatus.Joined });
        }

        ctx.SaveChanges();
    }

    private static void EnsureUser(AppDbContext ctx, int userId)
    {
        if (!ctx.Users.Any(u => u.Id == userId))
            ctx.Users.Add(new User { Id = userId, Email = $"u{userId}@t", Nickname = $"U{userId}", PasswordHash = "h", CreatedAt = DateTime.UtcNow });
    }

    [Fact]
    public async Task GetParticipationStats_NoSessions_ReturnsZero()
    {
        using var context = new AppDbContext(NewOptions(nameof(GetParticipationStats_NoSessions_ReturnsZero)));
        var stats = await this.CreateService(context).GetParticipationStatsForUserAsync(1);

        Assert.Equal(0, stats.TotalSessions);
        Assert.Equal(0, stats.SessionsAsGm);
        Assert.Equal(0, stats.TotalHoursPlayed);
        Assert.Empty(stats.ByMonth);
    }

    [Fact]
    public async Task GetParticipationStats_AsGm_CountsSessionHoursAndGroupSize()
    {
        using var context = new AppDbContext(NewOptions(nameof(GetParticipationStats_AsGm_CountsSessionHoursAndGroupSize)));
        var start = new DateTime(2026, 5, 10, 20, 0, 0, DateTimeKind.Utc);
        // GM = user 1, players 2 and 3, session lasted 2h.
        SeedSession(context, 100, gmUserId: 1, startedAt: start, endedAt: start.AddHours(2), campaignId: 7, playerUserIds: new[] { 2, 3 });

        var stats = await this.CreateService(context).GetParticipationStatsForUserAsync(1);

        Assert.Equal(1, stats.TotalSessions);
        Assert.Equal(1, stats.SessionsAsGm);
        Assert.Equal(0, stats.SessionsAsPlayer);
        Assert.Equal(2, stats.TotalHoursPlayed);
        Assert.Equal(2, stats.AverageGroupSize);
        Assert.Equal(1, stats.CampaignsPlayed);
    }

    [Fact]
    public async Task GetParticipationStats_AsPlayer_CountsViaCharacterOwnership()
    {
        using var context = new AppDbContext(NewOptions(nameof(GetParticipationStats_AsPlayer_CountsViaCharacterOwnership)));
        var start = new DateTime(2026, 5, 10, 20, 0, 0, DateTimeKind.Utc);
        SeedSession(context, 100, gmUserId: 1, startedAt: start, endedAt: start.AddHours(3), campaignId: 7, playerUserIds: new[] { 2 });

        var stats = await this.CreateService(context).GetParticipationStatsForUserAsync(2);

        Assert.Equal(1, stats.TotalSessions);
        Assert.Equal(0, stats.SessionsAsGm);
        Assert.Equal(1, stats.SessionsAsPlayer);
        Assert.Equal(3, stats.TotalHoursPlayed);
    }

    [Fact]
    public async Task GetParticipationStats_OngoingSession_NotCountedInHours()
    {
        using var context = new AppDbContext(NewOptions(nameof(GetParticipationStats_OngoingSession_NotCountedInHours)));
        var start = new DateTime(2026, 6, 1, 20, 0, 0, DateTimeKind.Utc);
        SeedSession(context, 100, gmUserId: 1, startedAt: start, endedAt: null, campaignId: 7, playerUserIds: new[] { 2 });

        var stats = await this.CreateService(context).GetParticipationStatsForUserAsync(1);

        Assert.Equal(1, stats.TotalSessions);
        Assert.Equal(0, stats.TotalHoursPlayed);
    }

    [Fact]
    public async Task GetParticipationStats_GroupsByMonthAndIgnoresOtherUsers()
    {
        using var context = new AppDbContext(NewOptions(nameof(GetParticipationStats_GroupsByMonthAndIgnoresOtherUsers)));
        var may = new DateTime(2026, 5, 10, 20, 0, 0, DateTimeKind.Utc);
        var june = new DateTime(2026, 6, 12, 20, 0, 0, DateTimeKind.Utc);
        SeedSession(context, 100, gmUserId: 1, startedAt: may, endedAt: may.AddHours(2), campaignId: 7, playerUserIds: new[] { 2 });
        SeedSession(context, 101, gmUserId: 1, startedAt: june, endedAt: june.AddHours(1), campaignId: 8, playerUserIds: new[] { 2 });
        // A session that does NOT involve user 1 (GM = user 9, player = user 8) must be ignored.
        SeedSession(context, 102, gmUserId: 9, startedAt: june, endedAt: june.AddHours(5), campaignId: 9, playerUserIds: new[] { 8 });

        var stats = await this.CreateService(context).GetParticipationStatsForUserAsync(1);

        Assert.Equal(2, stats.TotalSessions);
        Assert.Equal(2, stats.CampaignsPlayed);
        Assert.Equal(3, stats.TotalHoursPlayed); // 2 + 1, the 5h foreign session ignored
        Assert.Equal(2, stats.ByMonth.Count);
        Assert.Equal(5, stats.ByMonth[0].Month); // chronological
        Assert.Equal(6, stats.ByMonth[1].Month);
    }
}
