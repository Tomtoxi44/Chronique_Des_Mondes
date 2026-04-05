// -----------------------------------------------------------------------
// <copyright file="AchievementServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Common.Services;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="AchievementService"/>.
/// </summary>
public class AchievementServiceTests
{
    private readonly Mock<ILogger<AchievementService>> loggerMock;

    public AchievementServiceTests()
    {
        this.loggerMock = new Mock<ILogger<AchievementService>>();
    }

    [Fact]
    public async Task CreateAchievementAsync_ValidData_Success()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CreateAchievement")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);
        await context.SaveChangesAsync();

        var service = new AchievementService(context, this.loggerMock.Object);

        var createDto = new CreateAchievementDto
        {
            Name = "First Blood",
            Description = "Defeat your first enemy",
            Level = AchievementLevel.World,
            WorldId = 1,
            Rarity = AchievementRarity.Common,
            IsAutomatic = false,
            IsSecret = false
        };

        // Act
        var result = await service.CreateAchievementAsync(createDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("First Blood", result!.Name);
        Assert.Equal(AchievementRarity.Common, result.Rarity);
        Assert.Equal(AchievementLevel.World, result.Level);
    }

    [Fact]
    public async Task AwardAchievementAsync_ValidAward_Success()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_AwardAchievement")
            .Options;

        using var context = new AppDbContext(options);

        var gm = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        var player = new User { Id = 2, Email = "player@test.com", Nickname = "Player", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(gm, player);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var achievement = new Achievement
        {
            Id = 1,
            Name = "Test Achievement",
            Description = "Test Description",
            Level = AchievementLevel.World,
            WorldId = 1,
            Rarity = AchievementRarity.Rare,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Achievements.Add(achievement);
        await context.SaveChangesAsync();

        var service = new AchievementService(context, this.loggerMock.Object);

        // Act
        var result = await service.AwardAchievementAsync(1, 2, 1, "Well done!");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.AchievementId);
        Assert.Equal(2, result.UserId);
        Assert.Equal("Well done!", result.Message);
    }

    [Fact]
    public async Task GetUserAchievementsAsync_ReturnsUserAchievements()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetUserAchievements")
            .Options;

        using var context = new AppDbContext(options);

        var user1 = new User { Id = 1, Email = "user1@test.com", Nickname = "User1", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        var user2 = new User { Id = 2, Email = "user2@test.com", Nickname = "User2", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(user1, user2);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var achievement1 = new Achievement
        {
            Id = 1,
            Name = "Achievement 1",
            Description = "Desc",
            Level = AchievementLevel.World,
            WorldId = 1,
            Rarity = AchievementRarity.Common,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
        var achievement2 = new Achievement
        {
            Id = 2,
            Name = "Achievement 2",
            Description = "Desc",
            Level = AchievementLevel.World,
            WorldId = 1,
            Rarity = AchievementRarity.Rare,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Achievements.AddRange(achievement1, achievement2);

        context.UserAchievements.AddRange(
            new UserAchievement { AchievementId = 1, UserId = 1, UnlockedAt = DateTime.UtcNow },
            new UserAchievement { AchievementId = 2, UserId = 1, UnlockedAt = DateTime.UtcNow },
            new UserAchievement { AchievementId = 1, UserId = 2, UnlockedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new AchievementService(context, this.loggerMock.Object);

        // Act
        var result = await service.GetUserAchievementsAsync(1);

        // Assert
        var achievements = result.ToList();
        Assert.Equal(2, achievements.Count);
        Assert.All(achievements, a => Assert.Equal(1, a.UserId));
    }

    [Fact]
    public async Task RevokeAchievementAsync_RemovesAchievement()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_RevokeAchievement")
            .Options;

        using var context = new AppDbContext(options);

        var gm = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        var player = new User { Id = 2, Email = "player@test.com", Nickname = "Player", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(gm, player);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var achievement = new Achievement
        {
            Id = 1,
            Name = "Test",
            Description = "Test",
            Level = AchievementLevel.World,
            WorldId = 1,
            Rarity = AchievementRarity.Common,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Achievements.Add(achievement);

        var userAchievement = new UserAchievement
        {
            Id = 1,
            AchievementId = 1,
            UserId = 2,
            UnlockedAt = DateTime.UtcNow
        };
        context.UserAchievements.Add(userAchievement);
        await context.SaveChangesAsync();

        var service = new AchievementService(context, this.loggerMock.Object);

        // Act
        var result = await service.RevokeAchievementAsync(1, 1);

        // Assert
        Assert.True(result);
        var revoked = await context.UserAchievements.FindAsync(1);
        Assert.Null(revoked); // Should be deleted
    }

    [Fact]
    public async Task GetAchievementsByWorldAsync_FiltersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetAchievements_World")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world1 = new World { Id = 1, Name = "World 1", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        var world2 = new World { Id = 2, Name = "World 2", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.AddRange(world1, world2);

        context.Achievements.AddRange(
            new Achievement { Id = 1, Name = "W1 Achievement 1", Description = "D", Level = AchievementLevel.World, WorldId = 1, Rarity = AchievementRarity.Common, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
            new Achievement { Id = 2, Name = "W1 Achievement 2", Description = "D", Level = AchievementLevel.World, WorldId = 1, Rarity = AchievementRarity.Rare, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
            new Achievement { Id = 3, Name = "W2 Achievement", Description = "D", Level = AchievementLevel.World, WorldId = 2, Rarity = AchievementRarity.Epic, CreatedBy = 1, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new AchievementService(context, this.loggerMock.Object);

        // Act
        var result = await service.GetAchievementsByWorldAsync(1, 1);

        // Assert
        var achievements = result.ToList();
        Assert.Equal(2, achievements.Count);
        Assert.All(achievements, a => Assert.Equal(1, a.WorldId));
    }
}
