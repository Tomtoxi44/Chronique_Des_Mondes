// -----------------------------------------------------------------------
// <copyright file="CampaignServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
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
/// Unit tests for <see cref="CampaignService"/>.
/// </summary>
public class CampaignServiceTests
{
    private readonly Mock<IImageStorage> imageStorageMock;
    private readonly Mock<INotificationService> notificationServiceMock;
    private readonly Mock<ILogger<CampaignService>> loggerMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="CampaignServiceTests"/> class.
    /// </summary>
    public CampaignServiceTests()
    {
        this.imageStorageMock = new Mock<IImageStorage>();
        this.notificationServiceMock = new Mock<INotificationService>();
        this.loggerMock = new Mock<ILogger<CampaignService>>();
    }

    private CampaignService CreateService(AppDbContext context) =>
        new(context, this.imageStorageMock.Object, this.notificationServiceMock.Object, this.loggerMock.Object);

    private static World SeedWorld(AppDbContext context, int id = 1, int ownerId = 1, GameType gameType = GameType.DnD5e)
    {
        var world = new World { Id = id, Name = $"World {id}", GameType = gameType, UserId = ownerId, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);
        return world;
    }

    /// <summary>
    /// Tests that CreateCampaignAsync successfully creates a campaign with valid data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateCampaignAsync_ValidData_Success()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CreateCampaign_Success")
            .Options;

        using var context = new AppDbContext(options);

        var testUser = new User
        {
            Id = 1,
            Email = "gamemaster@test.com",
            Nickname = "GameMaster",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(testUser);
        SeedWorld(context);
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        var createDto = new CreateCampaignDto
        {
            Name = "Test Campaign",
            Description = "A test campaign description",
            WorldId = 1,
            Visibility = Visibility.Private,
            MaxPlayers = 5,
        };

        // Act
        var result = await service.CreateCampaignAsync(createDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Campaign", result!.Name);
        Assert.Equal("A test campaign description", result.Description);
        Assert.Equal(Visibility.Private, result.Visibility);
        Assert.Equal(5, result.MaxPlayers);
        Assert.Equal(1, result.CreatedBy);
        Assert.True(result.IsActive);

        // Verify campaign was saved to database
        var savedCampaign = await context.Campaigns.FirstOrDefaultAsync(c => c.Name == "Test Campaign");
        Assert.NotNull(savedCampaign);
    }

    /// <summary>
    /// Tests that CreateCampaignAsync calls image storage service when image is provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task CreateCampaignAsync_ImageUpload_CallsImageService()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CreateCampaign_WithImage")
            .Options;

        using var context = new AppDbContext(options);

        var testUser = new User
        {
            Id = 1,
            Email = "gamemaster@test.com",
            Nickname = "GameMaster",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(testUser);
        SeedWorld(context);
        await context.SaveChangesAsync();

        // Setup mock to return a valid image URL
        this.imageStorageMock
            .Setup(x => x.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(ImageUploadResult.Ok("/uploads/campaigns/test-image.jpg"));

        var service = this.CreateService(context);

        var createDto = new CreateCampaignDto
        {
            Name = "Campaign With Image",
            WorldId = 1,
            CoverImageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==", // 1x1 transparent PNG
        };

        // Act
        var result = await service.CreateCampaignAsync(createDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.CoverImageUrl);

        // Cover is uploaded once, after the campaign has its real id.
        this.imageStorageMock.Verify(
            x => x.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetMyCampaignsAsync filters campaigns by user ID correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetMyCampaignsAsync_FiltersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetMyCampaigns")
            .Options;

        using var context = new AppDbContext(options);

        // Add test users
        var user1 = new User { Id = 1, Email = "user1@test.com", Nickname = "User1", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        var user2 = new User { Id = 2, Email = "user2@test.com", Nickname = "User2", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(user1, user2);
        SeedWorld(context);

        // Add campaigns for different users
        var campaign1 = new Campaign
        {
            Name = "User1 Campaign 1",
            WorldId = 1,
            Visibility = Visibility.Private,
            MaxPlayers = 6,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false,
        };

        var campaign2 = new Campaign
        {
            Name = "User1 Campaign 2",
            WorldId = 1,
            Visibility = Visibility.Public,
            MaxPlayers = 4,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false,
        };

        var campaign3 = new Campaign
        {
            Name = "User2 Campaign",
            WorldId = 1,
            Visibility = Visibility.Private,
            MaxPlayers = 8,
            CreatedBy = 2,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false,
        };

        context.Campaigns.AddRange(campaign1, campaign2, campaign3);
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        // Act
        var result = await service.GetMyCampaignsAsync(1);

        // Assert
        var campaigns = result.ToList();
        Assert.Equal(2, campaigns.Count);
        Assert.All(campaigns, c => Assert.Equal(1, c.CreatedBy));
        Assert.All(campaigns, c => Assert.Equal(GameType.DnD5e, c.GameType));
        Assert.Contains(campaigns, c => c.Name == "User1 Campaign 1");
        Assert.Contains(campaigns, c => c.Name == "User1 Campaign 2");
    }

    /// <summary>
    /// Tests that GetCampaignByIdAsync checks authorization correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCampaignByIdAsync_ChecksAuthorization()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetCampaign_Authorization")
            .Options;

        using var context = new AppDbContext(options);

        var user1 = new User { Id = 1, Email = "user1@test.com", Nickname = "User1", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        var user2 = new User { Id = 2, Email = "user2@test.com", Nickname = "User2", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(user1, user2);
        SeedWorld(context);

        var campaign = new Campaign
        {
            Id = 1,
            Name = "Private Campaign",
            WorldId = 1,
            Visibility = Visibility.Private,
            MaxPlayers = 6,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false,
        };

        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        // Act - authorized user (campaign creator)
        var authorizedResult = await service.GetCampaignByIdAsync(1, 1);

        // Act - unauthorized user
        var unauthorizedResult = await service.GetCampaignByIdAsync(1, 2);

        // Assert
        Assert.NotNull(authorizedResult);
        Assert.Equal("Private Campaign", authorizedResult!.Name);
        Assert.Null(unauthorizedResult);
    }
}
