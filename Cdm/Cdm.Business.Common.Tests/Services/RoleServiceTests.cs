// <copyright file="RoleServiceTests.cs" company="Chronique Des Mondes">
// Copyright (c) Chronique Des Mondes. All rights reserved.
// </copyright>

namespace Cdm.Business.Common.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cdm.Business.Common.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for RoleService.
/// </summary>
public class RoleServiceTests
{
    private readonly Mock<ILogger<RoleService>> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleServiceTests"/> class.
    /// </summary>
    public RoleServiceTests()
    {
        this.mockLogger = new Mock<ILogger<RoleService>>();
    }

    /// <summary>
    /// Tests that GetUserRolesAsync returns all roles for a user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetUserRolesAsync_WithMultipleRoles_ReturnsAllRoles()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hash",
            Nickname = "TestUser",
            CreatedAt = DateTime.UtcNow,
        };

        var playerRole = new Role
        {
            Name = "Player",
            CreatedAt = DateTime.UtcNow,
        };

        var gmRole = new Role
        {
            Name = "GameMaster",
            CreatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        context.Roles.AddRange(playerRole, gmRole);
        await context.SaveChangesAsync();

        context.UserRoles.AddRange(
            new UserRole { UserId = user.Id, RoleId = playerRole.Id, AssignedAt = DateTime.UtcNow },
            new UserRole { UserId = user.Id, RoleId = gmRole.Id, AssignedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new RoleService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetUserRolesAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.RoleName == "Player");
        Assert.Contains(result, r => r.RoleName == "GameMaster");
    }

    /// <summary>
    /// Tests that GetUserRolesAsync returns empty list for user with no roles.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetUserRolesAsync_WithNoRoles_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hash",
            Nickname = "TestUser",
            CreatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new RoleService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetUserRolesAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that HasRoleAsync returns true when user has the role.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasRoleAsync_WhenUserHasRole_ReturnsTrue()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hash",
            Nickname = "TestUser",
            CreatedAt = DateTime.UtcNow,
        };

        var playerRole = new Role
        {
            Name = "Player",
            CreatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        context.Roles.Add(playerRole);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = playerRole.Id,
            AssignedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var service = new RoleService(context, this.mockLogger.Object);

        // Act
        var result = await service.HasRoleAsync(user.Id, "Player");

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that HasRoleAsync returns false when user does not have the role.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasRoleAsync_WhenUserDoesNotHaveRole_ReturnsFalse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hash",
            Nickname = "TestUser",
            CreatedAt = DateTime.UtcNow,
        };

        var playerRole = new Role
        {
            Name = "Player",
            CreatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        context.Roles.Add(playerRole);
        await context.SaveChangesAsync();

        var service = new RoleService(context, this.mockLogger.Object);

        // Act
        var result = await service.HasRoleAsync(user.Id, "GameMaster");

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that RequestGameMasterRoleAsync assigns the role successfully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RequestGameMasterRoleAsync_WhenUserDoesNotHaveRole_AssignsRole()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hash",
            Nickname = "TestUser",
            CreatedAt = DateTime.UtcNow,
        };

        var gmRole = new Role
        {
            Name = "GameMaster",
            CreatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        context.Roles.Add(gmRole);
        await context.SaveChangesAsync();

        var service = new RoleService(context, this.mockLogger.Object);

        // Act
        var result = await service.RequestGameMasterRoleAsync(user.Id);

        // Assert
        Assert.True(result);

        var hasRole = await context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == gmRole.Id);
        Assert.True(hasRole);
    }

    /// <summary>
    /// Tests that RequestGameMasterRoleAsync returns false when user already has the role.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task RequestGameMasterRoleAsync_WhenUserAlreadyHasRole_ReturnsFalse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hash",
            Nickname = "TestUser",
            CreatedAt = DateTime.UtcNow,
        };

        var gmRole = new Role
        {
            Name = "GameMaster",
            CreatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        context.Roles.Add(gmRole);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = gmRole.Id,
            AssignedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var service = new RoleService(context, this.mockLogger.Object);

        // Act
        var result = await service.RequestGameMasterRoleAsync(user.Id);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that AssignRoleAsync assigns role to user successfully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AssignRoleAsync_WithValidRole_AssignsToUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hash",
            Nickname = "TestUser",
            CreatedAt = DateTime.UtcNow,
        };

        var adminRole = new Role
        {
            Name = "Admin",
            CreatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        context.Roles.Add(adminRole);
        await context.SaveChangesAsync();

        var service = new RoleService(context, this.mockLogger.Object);

        // Act
        var result = await service.AssignRoleAsync(user.Id, adminRole.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(adminRole.Id, result.RoleId);

        var hasRole = await context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == adminRole.Id);
        Assert.True(hasRole);
    }

    /// <summary>
    /// Tests that AssignRoleAsync returns null when user already has the role.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AssignRoleAsync_WhenUserAlreadyHasRole_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hash",
            Nickname = "TestUser",
            CreatedAt = DateTime.UtcNow,
        };

        var adminRole = new Role
        {
            Name = "Admin",
            CreatedAt = DateTime.UtcNow,
        };

        context.Users.Add(user);
        context.Roles.Add(adminRole);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id,
            AssignedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var service = new RoleService(context, this.mockLogger.Object);

        // Act
        var result = await service.AssignRoleAsync(user.Id, adminRole.Id);

        // Assert
        Assert.Null(result);
    }
}
