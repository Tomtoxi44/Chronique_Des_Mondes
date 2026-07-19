// -----------------------------------------------------------------------
// <copyright file="AchievementEvaluationServiceTests.cs" company="ANGIBAUD Tommy">
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
/// Unit tests for <see cref="AchievementEvaluationService"/>.
/// </summary>
public class AchievementEvaluationServiceTests
{
    private readonly Mock<INotificationService> notificationServiceMock = new();
    private readonly Mock<ILogger<AchievementEvaluationService>> loggerMock = new();

    private const int PlayerUserId = 2;
    private const int WorldId = 1;
    private const int CampaignId = 5;
    private const int SessionId = 10;

    private static DbContextOptions<AppDbContext> NewOptions(string name) =>
        new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(name).Options;

    private AchievementEvaluationService CreateService(AppDbContext context) =>
        new(context, this.notificationServiceMock.Object, this.loggerMock.Object);

    /// <summary>
    /// Seeds a world/campaign/session and one automatic World-level achievement with the given condition.
    /// </summary>
    private static async Task<int> SeedAsync(AppDbContext context, string condition)
    {
        context.Users.Add(new User { Id = PlayerUserId, Email = "p@test.com", Nickname = "Joueur", PasswordHash = "h", CreatedAt = DateTime.UtcNow });
        context.Worlds.Add(new World { Id = WorldId, Name = "Monde", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow });
        context.Campaigns.Add(new Campaign { Id = CampaignId, Name = "Campagne", WorldId = WorldId, CreatedBy = 1, CreatedAt = DateTime.UtcNow });
        context.Sessions.Add(new Session { Id = SessionId, CampaignId = CampaignId, StartedById = 1, StartedAt = DateTime.UtcNow });

        var achievement = new Achievement
        {
            Id = 50,
            Name = "Succès auto",
            Description = "Test",
            Level = AchievementLevel.World,
            WorldId = WorldId,
            Rarity = AchievementRarity.Common,
            IsActive = true,
            IsAutomatic = true,
            AutomaticCondition = condition,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Achievements.Add(achievement);
        await context.SaveChangesAsync();
        return achievement.Id;
    }

    /// <summary>
    /// A natural 20 on a d20 unlocks a DiceCritical automatic achievement.
    /// </summary>
    [Fact]
    public async Task OnDiceRolledAsync_NaturalTwenty_UnlocksCritical()
    {
        using var context = new AppDbContext(NewOptions(nameof(OnDiceRolledAsync_NaturalTwenty_UnlocksCritical)));
        var achievementId = await SeedAsync(context, "DiceCritical");
        var service = this.CreateService(context);

        await service.OnDiceRolledAsync(PlayerUserId, SessionId, "D20", new[] { 20 });

        var unlocked = await context.UserAchievements.AnyAsync(ua => ua.UserId == PlayerUserId && ua.AchievementId == achievementId);
        Assert.True(unlocked);
    }

    /// <summary>
    /// A roll without a natural 20 does not unlock a DiceCritical achievement.
    /// </summary>
    [Fact]
    public async Task OnDiceRolledAsync_NoTwenty_DoesNotUnlock()
    {
        using var context = new AppDbContext(NewOptions(nameof(OnDiceRolledAsync_NoTwenty_DoesNotUnlock)));
        await SeedAsync(context, "DiceCritical");
        var service = this.CreateService(context);

        await service.OnDiceRolledAsync(PlayerUserId, SessionId, "D20", new[] { 14 });

        Assert.False(await context.UserAchievements.AnyAsync());
    }

    /// <summary>
    /// The critical achievement is only awarded once, even on repeated crits.
    /// </summary>
    [Fact]
    public async Task OnDiceRolledAsync_TwiceCritical_UnlocksOnce()
    {
        using var context = new AppDbContext(NewOptions(nameof(OnDiceRolledAsync_TwiceCritical_UnlocksOnce)));
        var achievementId = await SeedAsync(context, "DiceCritical");
        var service = this.CreateService(context);

        await service.OnDiceRolledAsync(PlayerUserId, SessionId, "D20", new[] { 20 });
        await service.OnDiceRolledAsync(PlayerUserId, SessionId, "D20", new[] { 20 });

        var count = await context.UserAchievements.CountAsync(ua => ua.UserId == PlayerUserId && ua.AchievementId == achievementId);
        Assert.Equal(1, count);
    }

    /// <summary>
    /// Attending the first session unlocks a "SessionsAttended:1" achievement.
    /// </summary>
    [Fact]
    public async Task OnSessionAttendedAsync_ThresholdMet_Unlocks()
    {
        using var context = new AppDbContext(NewOptions(nameof(OnSessionAttendedAsync_ThresholdMet_Unlocks)));
        var achievementId = await SeedAsync(context, "SessionsAttended:1");

        // The player must be an actual participant for the attendance count.
        context.Characters.Add(new Character { Id = 100, UserId = PlayerUserId, Name = "Perso", IsActive = true });
        context.WorldCharacters.Add(new WorldCharacter { Id = 200, CharacterId = 100, WorldId = WorldId, IsActive = true });
        context.SessionParticipants.Add(new SessionParticipant { Id = 300, SessionId = SessionId, WorldCharacterId = 200, JoinedAt = DateTime.UtcNow, Status = SessionParticipantStatus.Joined });
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        await service.OnSessionAttendedAsync(PlayerUserId, SessionId);

        Assert.True(await context.UserAchievements.AnyAsync(ua => ua.UserId == PlayerUserId && ua.AchievementId == achievementId));
    }

    /// <summary>
    /// A "SessionsAttended:3" achievement stays locked when only one session was attended.
    /// </summary>
    [Fact]
    public async Task OnSessionAttendedAsync_ThresholdNotMet_StaysLocked()
    {
        using var context = new AppDbContext(NewOptions(nameof(OnSessionAttendedAsync_ThresholdNotMet_StaysLocked)));
        await SeedAsync(context, "SessionsAttended:3");

        context.Characters.Add(new Character { Id = 100, UserId = PlayerUserId, Name = "Perso", IsActive = true });
        context.WorldCharacters.Add(new WorldCharacter { Id = 200, CharacterId = 100, WorldId = WorldId, IsActive = true });
        context.SessionParticipants.Add(new SessionParticipant { Id = 300, SessionId = SessionId, WorldCharacterId = 200, JoinedAt = DateTime.UtcNow, Status = SessionParticipantStatus.Joined });
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        await service.OnSessionAttendedAsync(PlayerUserId, SessionId);

        Assert.False(await context.UserAchievements.AnyAsync());
    }

    /// <summary>
    /// Completing an accepted trade unlocks a "TradesCompleted:1" achievement.
    /// </summary>
    [Fact]
    public async Task OnTradeAcceptedAsync_ThresholdMet_Unlocks()
    {
        using var context = new AppDbContext(NewOptions(nameof(OnTradeAcceptedAsync_ThresholdMet_Unlocks)));
        var achievementId = await SeedAsync(context, "TradesCompleted:1");
        context.SessionTrades.Add(new SessionTrade
        {
            Id = 400, SessionId = SessionId, FromUserId = 1, ToUserId = PlayerUserId,
            Status = TradeStatus.Accepted, CreatedAt = DateTime.UtcNow, RespondedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        await service.OnTradeAcceptedAsync(PlayerUserId, SessionId);

        Assert.True(await context.UserAchievements.AnyAsync(ua => ua.UserId == PlayerUserId && ua.AchievementId == achievementId));
    }
}
