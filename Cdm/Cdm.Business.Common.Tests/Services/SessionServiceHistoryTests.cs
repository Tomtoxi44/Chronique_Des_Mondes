// -----------------------------------------------------------------------
// <copyright file="SessionServiceHistoryTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Abstraction.Services;
using Cdm.Business.Common.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="SessionService.GetSessionHistoryAsync"/>.
/// </summary>
public class SessionServiceHistoryTests
{
    private readonly Mock<ILogger<SessionService>> loggerMock = new();
    private readonly Mock<INotificationService> notificationServiceMock = new();

    private static DbContextOptions<AppDbContext> NewOptions(string name) =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;

    private SessionService CreateService(AppDbContext context) =>
        new(context, this.loggerMock.Object, this.notificationServiceMock.Object);

    /// <summary>
    /// The GM of a session gets its chat and dice history, merged and ordered chronologically,
    /// with the comma-separated die results parsed back into integers.
    /// </summary>
    [Fact]
    public async Task GetSessionHistoryAsync_AsGameMaster_ReturnsOrderedHistory()
    {
        // Arrange
        using var context = new AppDbContext(NewOptions(nameof(GetSessionHistoryAsync_AsGameMaster_ReturnsOrderedHistory)));

        const int gmUserId = 1;
        var baseTime = new DateTime(2026, 07, 19, 10, 0, 0, DateTimeKind.Utc);

        context.Sessions.Add(new Session { Id = 10, CampaignId = 5, StartedById = gmUserId, StartedAt = baseTime });
        context.SessionMessages.Add(new SessionMessage
        {
            SessionId = 10, UserId = gmUserId, UserName = "MJ", Message = "Bienvenue", SentAt = baseTime.AddMinutes(1)
        });
        context.SessionDiceRolls.Add(new SessionDiceRoll
        {
            SessionId = 10, UserId = gmUserId, UserName = "MJ", DiceType = "D20", Count = 1,
            Results = "18", Modifier = 2, Total = 20, RolledAt = baseTime.AddMinutes(2)
        });
        context.SessionMessages.Add(new SessionMessage
        {
            SessionId = 10, UserId = gmUserId, UserName = "MJ", Message = "À toi de jouer", SentAt = baseTime.AddMinutes(3)
        });
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        // Act
        var history = await service.GetSessionHistoryAsync(10, gmUserId);

        // Assert
        Assert.NotNull(history);
        Assert.Equal(2, history!.Messages.Count);
        Assert.Single(history.DiceRolls);
        Assert.Equal("Bienvenue", history.Messages[0].Message);
        Assert.Equal("À toi de jouer", history.Messages[1].Message);
        Assert.Equal(new[] { 18 }, history.DiceRolls[0].Results);
        Assert.Equal(20, history.DiceRolls[0].Total);
    }

    /// <summary>
    /// A user who is neither the GM nor a participant is denied the history (null).
    /// </summary>
    [Fact]
    public async Task GetSessionHistoryAsync_UnauthorizedUser_ReturnsNull()
    {
        // Arrange
        using var context = new AppDbContext(NewOptions(nameof(GetSessionHistoryAsync_UnauthorizedUser_ReturnsNull)));

        context.Sessions.Add(new Session { Id = 20, CampaignId = 5, StartedById = 1, StartedAt = DateTime.UtcNow });
        context.SessionMessages.Add(new SessionMessage
        {
            SessionId = 20, UserId = 1, UserName = "MJ", Message = "Secret", SentAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        // Act — user 99 is neither GM nor participant
        var history = await service.GetSessionHistoryAsync(20, 99);

        // Assert
        Assert.Null(history);
    }

    /// <summary>
    /// A history request for a session that does not exist returns null rather than throwing.
    /// </summary>
    [Fact]
    public async Task GetSessionHistoryAsync_MissingSession_ReturnsNull()
    {
        // Arrange
        using var context = new AppDbContext(NewOptions(nameof(GetSessionHistoryAsync_MissingSession_ReturnsNull)));
        var service = this.CreateService(context);

        // Act
        var history = await service.GetSessionHistoryAsync(999, 1);

        // Assert
        Assert.Null(history);
    }
}
