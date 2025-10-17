// <copyright file="UserProfileServiceTests.cs" company="Chronique Des Mondes">
// Copyright (c) Chronique Des Mondes. All rights reserved.
// </copyright>

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Common.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="UserProfileService"/>.
/// </summary>
public class UserProfileServiceTests
{
    private readonly Mock<ILogger<UserProfileService>> loggerMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileServiceTests"/> class.
    /// </summary>
    public UserProfileServiceTests()
    {
        this.loggerMock = new Mock<ILogger<UserProfileService>>();
    }

    /// <summary>
    /// Tests that GetProfileAsync returns profile response when user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetProfileAsync_UserExists_ReturnsProfileResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetProfile_Success")
            .Options;

        using var context = new AppDbContext(options);
        
        var testUser = new User
        {
            Id = 1,
            Email = "test@example.com",
            Nickname = "TestUser",
            Username = "testusername",
            AvatarUrl = "/uploads/avatars/1_avatar.jpg",
            Preferences = "{\"theme\":\"dark\"}",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
        };
        
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        var service = new UserProfileService(context, this.loggerMock.Object);

        // Act
        var result = await service.GetProfileAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("TestUser", result.Nickname);
        Assert.Equal("testusername", result.Username);
        Assert.Equal("/uploads/avatars/1_avatar.jpg", result.AvatarUrl);
        Assert.Equal("{\"theme\":\"dark\"}", result.Preferences);
    }

    /// <summary>
    /// Tests that GetProfileAsync returns null when user does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetProfileAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetProfile_NotFound")
            .Options;

        using var context = new AppDbContext(options);
        var service = new UserProfileService(context, this.loggerMock.Object);

        // Act
        var result = await service.GetProfileAsync(999);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that UpdateProfileAsync updates user profile successfully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateProfileAsync_ValidRequest_UpdatesProfile()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UpdateProfile_Success")
            .Options;

        using var context = new AppDbContext(options);
        
        var testUser = new User
        {
            Id = 1,
            Email = "test@example.com",
            Nickname = "TestUser",
            Username = "oldusername",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
        };
        
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        var service = new UserProfileService(context, this.loggerMock.Object);

        var updateRequest = new UpdateProfileRequest
        {
            Username = "newusername",
            Preferences = "{\"theme\":\"light\"}",
        };

        // Act
        var result = await service.UpdateProfileAsync(1, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newusername", result!.Username);
        Assert.Equal("{\"theme\":\"light\"}", result.Preferences);
    }

    /// <summary>
    /// Tests that UpdateProfileAsync returns null when user does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateProfileAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UpdateProfile_NotFound")
            .Options;

        using var context = new AppDbContext(options);
        var service = new UserProfileService(context, this.loggerMock.Object);

        var updateRequest = new UpdateProfileRequest
        {
            Username = "testusername",
        };

        // Act
        var result = await service.UpdateProfileAsync(999, updateRequest);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that UpdateProfileAsync returns null when username is already taken.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateProfileAsync_UsernameAlreadyTaken_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UpdateProfile_UsernameTaken")
            .Options;

        using var context = new AppDbContext(options);
        
        var user1 = new User
        {
            Id = 1,
            Email = "user1@example.com",
            Nickname = "User1",
            Username = "user1",
            PasswordHash = "hash1",
            CreatedAt = DateTime.UtcNow,
        };

        var user2 = new User
        {
            Id = 2,
            Email = "user2@example.com",
            Nickname = "User2",
            Username = "existingusername",
            PasswordHash = "hash2",
            CreatedAt = DateTime.UtcNow,
        };
        
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var service = new UserProfileService(context, this.loggerMock.Object);

        var updateRequest = new UpdateProfileRequest
        {
            Username = "existingusername", // Already taken by user2
        };

        // Act
        var result = await service.UpdateProfileAsync(1, updateRequest);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that IsUsernameAvailableAsync returns true when username is available.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsUsernameAvailableAsync_UsernameAvailable_ReturnsTrue()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UsernameAvailable_True")
            .Options;

        using var context = new AppDbContext(options);
        var service = new UserProfileService(context, this.loggerMock.Object);

        // Act
        var result = await service.IsUsernameAvailableAsync("availableusername", 1);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that IsUsernameAvailableAsync returns false when username is taken by another user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsUsernameAvailableAsync_UsernameTaken_ReturnsFalse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UsernameAvailable_False")
            .Options;

        using var context = new AppDbContext(options);
        
        var existingUser = new User
        {
            Id = 2,
            Email = "existing@example.com",
            Nickname = "ExistingUser",
            Username = "takenusername",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
        };
        
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var service = new UserProfileService(context, this.loggerMock.Object);

        // Act
        var result = await service.IsUsernameAvailableAsync("takenusername", 1);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that IsUsernameAvailableAsync returns true when username is taken by the same user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsUsernameAvailableAsync_UsernameTakenBySameUser_ReturnsTrue()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UsernameAvailable_SameUser")
            .Options;

        using var context = new AppDbContext(options);
        
        var currentUser = new User
        {
            Id = 1,
            Email = "current@example.com",
            Nickname = "CurrentUser",
            Username = "myusername",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
        };
        
        context.Users.Add(currentUser);
        await context.SaveChangesAsync();

        var service = new UserProfileService(context, this.loggerMock.Object);

        // Act
        var result = await service.IsUsernameAvailableAsync("myusername", 1);

        // Assert
        Assert.True(result);
    }
}
