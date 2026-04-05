// -----------------------------------------------------------------------
// <copyright file="EventServiceTests.cs" company="ANGIBAUD Tommy">
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
/// Unit tests for <see cref="EventService"/>.
/// </summary>
public class EventServiceTests
{
    private readonly Mock<ILogger<EventService>> loggerMock;

    public EventServiceTests()
    {
        this.loggerMock = new Mock<ILogger<EventService>>();
    }

    [Fact]
    public async Task CreateEventAsync_WorldLevel_Success()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CreateEvent_World")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);
        await context.SaveChangesAsync();

        var service = new EventService(context, this.loggerMock.Object);

        var createDto = new CreateEventDto
        {
            Name = "Great Plague",
            Description = "A disease sweeps the land",
            Level = EventLevel.World,
            WorldId = 1,
            EffectType = EventEffectType.StatModifier,
            TargetStat = "Constitution",
            ModifierValue = -2,
            IsPermanent = true
        };

        // Act
        var result = await service.CreateEventAsync(createDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Great Plague", result!.Name);
        Assert.Equal(EventLevel.World, result.Level);
        Assert.Equal(1, result.WorldId);
        Assert.Equal(-2, result.ModifierValue);
    }

    [Fact]
    public async Task GetEventsByWorldAsync_ReturnsOnlyWorldEvents()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetEvents_World")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        context.Events.AddRange(
            new Event { Id = 1, Name = "World Event 1", Level = EventLevel.World, WorldId = 1, EffectType = EventEffectType.Narrative, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
            new Event { Id = 2, Name = "World Event 2", Level = EventLevel.World, WorldId = 1, EffectType = EventEffectType.Narrative, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
            new Event { Id = 3, Name = "Campaign Event", Level = EventLevel.Campaign, CampaignId = 1, EffectType = EventEffectType.Narrative, CreatedBy = 1, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new EventService(context, this.loggerMock.Object);

        // Act
        var result = await service.GetEventsByWorldAsync(1, 1);

        // Assert
        var events = result.ToList();
        Assert.Equal(2, events.Count);
        Assert.All(events, e => Assert.Equal(EventLevel.World, e.Level));
    }

    [Fact]
    public async Task MarkAsPermanentAsync_UpdatesEvent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_MarkPermanent")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var tempEvent = new Event
        {
            Id = 1,
            Name = "Temporary Event",
            Level = EventLevel.World,
            WorldId = 1,
            EffectType = EventEffectType.Narrative,
            IsPermanent = false,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Events.Add(tempEvent);
        await context.SaveChangesAsync();

        var service = new EventService(context, this.loggerMock.Object);

        // Act
        var result = await service.MarkAsPermanentAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result!.IsPermanent);
        Assert.Null(result.ExpiresAt);
    }

    [Fact]
    public async Task SetEventActiveAsync_TogglesActiveStatus()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_ToggleActive")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var eventEntity = new Event
        {
            Id = 1,
            Name = "Test Event",
            Level = EventLevel.World,
            WorldId = 1,
            EffectType = EventEffectType.Narrative,
            IsActive = true,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        var service = new EventService(context, this.loggerMock.Object);

        // Act
        var result = await service.SetEventActiveAsync(1, false, 1);

        // Assert
        Assert.NotNull(result);
        Assert.False(result!.IsActive);
    }

    [Fact]
    public async Task DeleteEventAsync_RemovesEvent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_DeleteEvent")
            .Options;

        using var context = new AppDbContext(options);

        var user = new User { Id = 1, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        var world = new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = 1, CreatedAt = DateTime.UtcNow };
        context.Worlds.Add(world);

        var eventEntity = new Event
        {
            Id = 1,
            Name = "Event to Delete",
            Level = EventLevel.World,
            WorldId = 1,
            EffectType = EventEffectType.Narrative,
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        var service = new EventService(context, this.loggerMock.Object);

        // Act
        var result = await service.DeleteEventAsync(1, 1);

        // Assert
        Assert.True(result);
        var deletedEvent = await context.Events.FindAsync(1);
        Assert.Null(deletedEvent); // Hard delete in EventService
    }
}
