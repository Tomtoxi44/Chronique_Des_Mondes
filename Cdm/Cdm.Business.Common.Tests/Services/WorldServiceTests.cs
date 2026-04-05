// -----------------------------------------------------------------------
// <copyright file="WorldServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Common.Services;
using Cdm.Common.Enums;
using Cdm.Common.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="WorldService"/>.
/// </summary>
public class WorldServiceTests
{
    private readonly Mock<IImageStorageService> imageStorageServiceMock;
    private readonly Mock<ILogger<WorldService>> loggerMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldServiceTests"/> class.
    /// </summary>
    public WorldServiceTests()
    {
        this.imageStorageServiceMock = new Mock<IImageStorageService>();
        this.loggerMock = new Mock<ILogger<WorldService>>();
    }

    /// <summary>
    /// Tests that CreateWorldAsync successfully creates a world with valid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateWorldAsync_ValidData_Success()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CreateWorld_Success")
            .Options;

        using var context = new AppDbContext(options);

        var testUser = new User
        {
            Id = 1,
            Email = "gm@test.com",
            Nickname = "TestGM",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        var service = new WorldService(context, this.imageStorageServiceMock.Object, this.loggerMock.Object);

        var createDto = new CreateWorldDto
        {
            Name = "Fantasy Realm",
            Description = "A magical world",
            GameType = GameType.DnD5e,
            Visibility = Visibility.Public,
            MaxCampaigns = 3,
        };

        // Act
        var result = await service.CreateWorldAsync(createDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fantasy Realm", result!.Name);
        Assert.Equal("A magical world", result.Description);
        Assert.Equal(GameType.DnD5e, result.GameType);
        Assert.Equal(1, result.UserId);
    }

    /// <summary>
    /// Tests that GetMyWorldsAsync returns only worlds owned by the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetMyWorldsAsync_ReturnsOnlyUserWorlds()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetMyWorlds")
            .Options;

        using var context = new AppDbContext(options);

        var user1 = new User { Id = 1, Email = "user1@test.com", Nickname = "User1", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        var user2 = new User { Id = 2, Email = "user2@test.com", Nickname = "User2", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(user1, user2);

        context.Worlds.AddRange(
            new World { Id = 1, Name = "World 1", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow },
            new World { Id = 2, Name = "World 2", GameType = GameType.DnD5e, UserId = 1, CreatedAt = DateTime.UtcNow },
            new World { Id = 3, Name = "World 3", GameType = GameType.Custom, UserId = 2, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new WorldService(context, this.imageStorageServiceMock.Object, this.loggerMock.Object);

        // Act
        var result = await service.GetMyWorldsAsync(1);

        // Assert
        Assert.NotNull(result);
        var worlds = result.ToList();
        Assert.Equal(2, worlds.Count);
        Assert.All(worlds, w => Assert.Equal(1, w.UserId));
    }

    /// <summary>
    /// Tests that GetWorldByIdAsync returns null for non-existent world.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetWorldByIdAsync_NonExistent_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetWorld_NotFound")
            .Options;

        using var context = new AppDbContext(options);
        var service = new WorldService(context, this.imageStorageServiceMock.Object, this.loggerMock.Object);

        // Act
        var result = await service.GetWorldByIdAsync(999, 1);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that UpdateWorldAsync successfully updates world data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateWorldAsync_ValidData_Success()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UpdateWorld")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old Description",
            GameType = GameType.Generic,
            Visibility = Visibility.Private,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Worlds.Add(world);
        await context.SaveChangesAsync();

        var service = new WorldService(context, this.imageStorageServiceMock.Object, this.loggerMock.Object);

        var updateDto = new CreateWorldDto
        {
            Name = "New Name",
            Description = "New Description",
            GameType = GameType.DnD5e,
            Visibility = Visibility.Public,
            MaxCampaigns = 5
        };

        // Act
        var result = await service.UpdateWorldAsync(1, updateDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result!.Name);
        Assert.Equal("New Description", result.Description);
        Assert.Equal(GameType.DnD5e, result.GameType);
    }

    /// <summary>
    /// Tests that DeleteWorldAsync soft deletes the world.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteWorldAsync_ValidWorld_SoftDeletes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_DeleteWorld")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World
        {
            Id = 1,
            Name = "Test World",
            GameType = GameType.Generic,
            Visibility = Visibility.Private,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Worlds.Add(world);
        await context.SaveChangesAsync();

        var service = new WorldService(context, this.imageStorageServiceMock.Object, this.loggerMock.Object);

        // Act
        var result = await service.DeleteWorldAsync(1, 1);

        // Assert
        Assert.True(result);
        var deletedWorld = await context.Worlds.FindAsync(1);
        Assert.NotNull(deletedWorld);
        Assert.True(deletedWorld!.IsDeleted);
    }

    /// <summary>
    /// Tests that UpdateWorldAsync returns null when user is not the owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateWorldAsync_NotOwner_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UpdateWorld_NotOwner")
            .Options;

        using var context = new AppDbContext(options);

        var user1 = new User { Id = 1, Email = "user1@test.com", Nickname = "User1", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        var user2 = new User { Id = 2, Email = "user2@test.com", Nickname = "User2", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(user1, user2);

        var world = new World
        {
            Id = 1,
            Name = "User1's World",
            GameType = GameType.Generic,
            Visibility = Visibility.Private,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Worlds.Add(world);
        await context.SaveChangesAsync();

        var service = new WorldService(context, this.imageStorageServiceMock.Object, this.loggerMock.Object);

        var updateDto = new CreateWorldDto { Name = "Hacked Name", GameType = GameType.Custom };

        // Act
        var result = await service.UpdateWorldAsync(1, updateDto, 2); // User 2 trying to update User 1's world

        // Assert
        Assert.Null(result);
    }
}
