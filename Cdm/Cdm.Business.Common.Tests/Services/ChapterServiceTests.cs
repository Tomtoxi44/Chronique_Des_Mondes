// -----------------------------------------------------------------------
// <copyright file="ChapterServiceTests.cs" company="ANGIBAUD Tommy">
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
/// Unit tests for <see cref="ChapterService"/>.
/// </summary>
public class ChapterServiceTests
{
    private readonly Mock<ILogger<ChapterService>> loggerMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterServiceTests"/> class.
    /// </summary>
    public ChapterServiceTests()
    {
        this.loggerMock = new Mock<ILogger<ChapterService>>();
    }

    /// <summary>
    /// Tests that CreateChapterAsync successfully creates a chapter with auto-increment number.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateChapterAsync_AutoIncrementNumber_Success()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CreateChapter_AutoIncrement")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var campaign = new Campaign
        {
            Id = 1,
            Name = "Campaign",
            WorldId = 1,
            CreatedBy = 1,
            Visibility = Visibility.Private,
            CreatedAt = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);

        // Add existing chapters
        context.Chapters.AddRange(
            new Chapter { Id = 1, CampaignId = 1, ChapterNumber = 1, Title = "Chapter 1", CreatedAt = DateTime.UtcNow },
            new Chapter { Id = 2, CampaignId = 1, ChapterNumber = 2, Title = "Chapter 2", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ChapterService(context, this.loggerMock.Object);

        var createDto = new CreateChapterDto
        {
            CampaignId = 1,
            Title = "New Chapter",
            Content = "Chapter content",
            ChapterNumber = 0 // Auto-increment
        };

        // Act
        var result = await service.CreateChapterAsync(createDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result!.ChapterNumber); // Should be auto-incremented to 3
        Assert.Equal("New Chapter", result.Title);
    }

    /// <summary>
    /// Tests that GetChaptersByCampaignAsync returns chapters in order.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetChaptersByCampaignAsync_ReturnsOrdered()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetChapters_Ordered")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var campaign = new Campaign
        {
            Id = 1,
            Name = "Campaign",
            WorldId = 1,
            CreatedBy = 1,
            Visibility = Visibility.Private,
            CreatedAt = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);

        context.Chapters.AddRange(
            new Chapter { Id = 3, CampaignId = 1, ChapterNumber = 3, Title = "Chapter 3", CreatedAt = DateTime.UtcNow },
            new Chapter { Id = 1, CampaignId = 1, ChapterNumber = 1, Title = "Chapter 1", CreatedAt = DateTime.UtcNow },
            new Chapter { Id = 2, CampaignId = 1, ChapterNumber = 2, Title = "Chapter 2", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ChapterService(context, this.loggerMock.Object);

        // Act
        var result = await service.GetChaptersByCampaignAsync(1, 1);

        // Assert
        var chapters = result.ToList();
        Assert.Equal(3, chapters.Count);
        Assert.Equal(1, chapters[0].ChapterNumber);
        Assert.Equal(2, chapters[1].ChapterNumber);
        Assert.Equal(3, chapters[2].ChapterNumber);
    }

    /// <summary>
    /// Tests that CompleteChapterAsync marks chapter as completed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task CompleteChapterAsync_MarksAsCompleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CompleteChapter")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var campaign = new Campaign
        {
            Id = 1,
            Name = "Campaign",
            WorldId = 1,
            CreatedBy = 1,
            Visibility = Visibility.Private,
            CreatedAt = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);

        var chapter = new Chapter
        {
            Id = 1,
            CampaignId = 1,
            ChapterNumber = 1,
            Title = "Chapter 1",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var service = new ChapterService(context, this.loggerMock.Object);

        // Act
        var result = await service.CompleteChapterAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result!.IsCompleted);
        
        var dbChapter = await context.Chapters.FindAsync(1);
        Assert.True(dbChapter!.IsCompleted);
    }

    /// <summary>
    /// Tests that DeleteChapterAsync soft deletes the chapter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteChapterAsync_SoftDeletes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_DeleteChapter")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var campaign = new Campaign
        {
            Id = 1,
            Name = "Campaign",
            WorldId = 1,
            CreatedBy = 1,
            Visibility = Visibility.Private,
            CreatedAt = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);

        var chapter = new Chapter
        {
            Id = 1,
            CampaignId = 1,
            ChapterNumber = 1,
            Title = "Chapter to Delete",
            CreatedAt = DateTime.UtcNow
        };
        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();

        var service = new ChapterService(context, this.loggerMock.Object);

        // Act
        var result = await service.DeleteChapterAsync(1, 1);

        // Assert
        Assert.True(result);
        
        var dbChapter = await context.Chapters.FindAsync(1);
        Assert.False(dbChapter!.IsActive);
    }

    /// <summary>
    /// Tests that non-GM cannot create chapters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateChapterAsync_NonGM_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CreateChapter_NonGM")
            .Options;

        using var context = new AppDbContext(options);

        var gm = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        var player = new User { Id = 2, Email = "player@test.com", Nickname = "Player", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(gm, player);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var campaign = new Campaign
        {
            Id = 1,
            Name = "Campaign",
            WorldId = 1,
            CreatedBy = 1, // GM is creator
            Visibility = Visibility.Private,
            CreatedAt = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        var service = new ChapterService(context, this.loggerMock.Object);

        var createDto = new CreateChapterDto
        {
            CampaignId = 1,
            Title = "Unauthorized Chapter",
            ChapterNumber = 1
        };

        // Act
        var result = await service.CreateChapterAsync(createDto, 2); // Player (non-GM) trying to create

        // Assert
        Assert.Null(result); // Should fail authorization
    }
}
