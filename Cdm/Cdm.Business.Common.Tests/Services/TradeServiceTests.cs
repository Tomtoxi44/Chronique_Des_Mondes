// -----------------------------------------------------------------------
// <copyright file="TradeServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Abstraction.Services;
using Cdm.Business.Common.Services;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="TradeService"/>.
/// </summary>
public class TradeServiceTests
{
    private readonly Mock<INotificationService> notificationServiceMock = new();
    private readonly Mock<IAchievementEvaluationService> achievementEvaluationMock = new();
    private readonly Mock<ILogger<TradeService>> loggerMock = new();

    private const int GmUserId = 1;
    private const int PlayerUserId = 2;

    private static DbContextOptions<AppDbContext> NewOptions(string name) =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;

    private TradeService CreateService(AppDbContext context) =>
        new(context, this.notificationServiceMock.Object, this.achievementEvaluationMock.Object, this.loggerMock.Object);

    /// <summary>
    /// Seeds a session (GM = user 1) with one joined participant (player = user 2).
    /// </summary>
    private static async Task SeedSessionWithParticipantAsync(AppDbContext context, int sessionId = 10)
    {
        context.Users.AddRange(
            new User { Id = GmUserId, Email = "gm@test.com", Nickname = "MJ", PasswordHash = "h", CreatedAt = DateTime.UtcNow },
            new User { Id = PlayerUserId, Email = "player@test.com", Nickname = "Joueur", PasswordHash = "h", CreatedAt = DateTime.UtcNow });

        context.Sessions.Add(new Session { Id = sessionId, CampaignId = 5, StartedById = GmUserId, StartedAt = DateTime.UtcNow });

        var character = new Character { Id = 100, UserId = PlayerUserId, Name = "Aragorn", IsActive = true };
        context.Characters.Add(character);
        var worldCharacter = new WorldCharacter { Id = 200, CharacterId = 100, WorldId = 1, IsActive = true };
        context.WorldCharacters.Add(worldCharacter);
        context.SessionParticipants.Add(new SessionParticipant
        {
            Id = 300, SessionId = sessionId, WorldCharacterId = 200, JoinedAt = DateTime.UtcNow, Status = SessionParticipantStatus.Joined
        });

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// The GM can propose a trade to a participant; the trade is persisted as Pending.
    /// </summary>
    [Fact]
    public async Task ProposeTradeAsync_GmToParticipant_CreatesPendingTrade()
    {
        using var context = new AppDbContext(NewOptions(nameof(ProposeTradeAsync_GmToParticipant_CreatesPendingTrade)));
        await SeedSessionWithParticipantAsync(context);
        var service = this.CreateService(context);

        var trade = await service.ProposeTradeAsync(10, GmUserId, PlayerUserId, "Une épée", "50 pièces d'or");

        Assert.NotNull(trade);
        Assert.Equal(TradeStatus.Pending, trade!.Status);
        Assert.Equal(GmUserId, trade.FromUserId);
        Assert.Equal(PlayerUserId, trade.ToUserId);
        Assert.Equal("Une épée", trade.OfferDescription);

        var persisted = await context.SessionTrades.FirstOrDefaultAsync(t => t.Id == trade.Id);
        Assert.NotNull(persisted);
        Assert.Equal(TradeStatus.Pending, persisted!.Status);

        // The recipient is notified.
        this.notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(It.Is<Cdm.Business.Abstraction.DTOs.CreateNotificationDto>(d => d.UserId == PlayerUserId)),
            Times.Once);
    }

    /// <summary>
    /// Proposing a trade to a user who is not a member of the session is rejected.
    /// </summary>
    [Fact]
    public async Task ProposeTradeAsync_TargetNotMember_ReturnsNull()
    {
        using var context = new AppDbContext(NewOptions(nameof(ProposeTradeAsync_TargetNotMember_ReturnsNull)));
        await SeedSessionWithParticipantAsync(context);
        var service = this.CreateService(context);

        var trade = await service.ProposeTradeAsync(10, GmUserId, 999, "X", "Y");

        Assert.Null(trade);
    }

    /// <summary>
    /// Proposing a trade to oneself is rejected.
    /// </summary>
    [Fact]
    public async Task ProposeTradeAsync_ToSelf_ReturnsNull()
    {
        using var context = new AppDbContext(NewOptions(nameof(ProposeTradeAsync_ToSelf_ReturnsNull)));
        await SeedSessionWithParticipantAsync(context);
        var service = this.CreateService(context);

        var trade = await service.ProposeTradeAsync(10, GmUserId, GmUserId, "X", "Y");

        Assert.Null(trade);
    }

    /// <summary>
    /// The recipient can accept a pending trade, moving it to Accepted.
    /// </summary>
    [Fact]
    public async Task RespondToTradeAsync_ByRecipient_Accepts()
    {
        using var context = new AppDbContext(NewOptions(nameof(RespondToTradeAsync_ByRecipient_Accepts)));
        await SeedSessionWithParticipantAsync(context);
        var service = this.CreateService(context);

        var proposed = await service.ProposeTradeAsync(10, GmUserId, PlayerUserId, "Une potion", "Une carte");
        Assert.NotNull(proposed);

        var result = await service.RespondToTradeAsync(proposed!.Id, PlayerUserId, accept: true);

        Assert.NotNull(result);
        Assert.Equal(TradeStatus.Accepted, result!.Status);
        Assert.NotNull(result.RespondedAt);
    }

    /// <summary>
    /// A user who is not the recipient cannot respond to a trade.
    /// </summary>
    [Fact]
    public async Task RespondToTradeAsync_ByNonRecipient_ReturnsNull()
    {
        using var context = new AppDbContext(NewOptions(nameof(RespondToTradeAsync_ByNonRecipient_ReturnsNull)));
        await SeedSessionWithParticipantAsync(context);
        var service = this.CreateService(context);

        var proposed = await service.ProposeTradeAsync(10, GmUserId, PlayerUserId, "Une potion", "Une carte");
        Assert.NotNull(proposed);

        // The GM (proposer) is not the recipient and cannot answer.
        var result = await service.RespondToTradeAsync(proposed!.Id, GmUserId, accept: true);

        Assert.Null(result);
    }

    /// <summary>
    /// Only members can read the pending trades; an outsider gets null.
    /// </summary>
    [Fact]
    public async Task GetPendingTradesAsync_Outsider_ReturnsNull()
    {
        using var context = new AppDbContext(NewOptions(nameof(GetPendingTradesAsync_Outsider_ReturnsNull)));
        await SeedSessionWithParticipantAsync(context);
        var service = this.CreateService(context);

        await service.ProposeTradeAsync(10, GmUserId, PlayerUserId, "A", "B");

        var asOutsider = await service.GetPendingTradesAsync(10, 999);
        Assert.Null(asOutsider);

        var asGm = await service.GetPendingTradesAsync(10, GmUserId);
        Assert.NotNull(asGm);
        Assert.Single(asGm!);
    }
}
