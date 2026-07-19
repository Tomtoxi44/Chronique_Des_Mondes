// -----------------------------------------------------------------------
// <copyright file="StatisticsServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Common.Services;
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
        new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(name).Options;

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
}
