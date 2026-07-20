// -----------------------------------------------------------------------
// <copyright file="NotificationServiceTests.cs" company="ANGIBAUD Tommy">
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
/// Unit tests for <see cref="NotificationService"/> — per-user isolation, read state and counts.
/// </summary>
public class NotificationServiceTests
{
    private const int UserId = 1;
    private const int OtherUserId = 2;

    private readonly Mock<ILogger<NotificationService>> loggerMock = new();

    private static AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"NotifTests_{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options);
    }

    private static Notification Notif(int id, int userId, bool read, DateTime? created = null) => new()
    {
        Id = id,
        UserId = userId,
        Type = NotificationType.WorldInvite,
        Title = $"N{id}",
        Message = "msg",
        IsRead = read,
        CreatedAt = created ?? DateTime.UtcNow
    };

    [Fact]
    public async Task CreateNotificationAsync_PersistsAsUnread()
    {
        using var ctx = NewContext();
        var service = new NotificationService(ctx, this.loggerMock.Object);

        var dto = new CreateNotificationDto
        {
            UserId = UserId,
            Type = NotificationType.AchievementUnlocked,
            Title = "Succès",
            Message = "Débloqué !"
        };

        var result = await service.CreateNotificationAsync(dto);

        Assert.NotNull(result);
        var stored = Assert.Single(ctx.Notifications);
        Assert.False(stored.IsRead);
        Assert.Equal(UserId, stored.UserId);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsOnlyOwnNewestFirst()
    {
        using var ctx = NewContext();
        ctx.Notifications.AddRange(
            Notif(1, UserId, false, DateTime.UtcNow.AddMinutes(-5)),
            Notif(2, UserId, true, DateTime.UtcNow.AddMinutes(-1)),
            Notif(3, OtherUserId, false, DateTime.UtcNow));
        await ctx.SaveChangesAsync();

        var service = new NotificationService(ctx, this.loggerMock.Object);
        var result = (await service.GetUserNotificationsAsync(UserId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].Id); // most recent first
        Assert.Equal(1, result[1].Id);
    }

    [Fact]
    public async Task GetUnreadCountAsync_CountsOnlyUnreadForUser()
    {
        using var ctx = NewContext();
        ctx.Notifications.AddRange(
            Notif(1, UserId, false),
            Notif(2, UserId, false),
            Notif(3, UserId, true),
            Notif(4, OtherUserId, false));
        await ctx.SaveChangesAsync();

        var service = new NotificationService(ctx, this.loggerMock.Object);

        Assert.Equal(2, await service.GetUnreadCountAsync(UserId));
    }

    [Fact]
    public async Task MarkAsReadAsync_Owner_MarksReadWithTimestamp()
    {
        using var ctx = NewContext();
        ctx.Notifications.Add(Notif(1, UserId, false));
        await ctx.SaveChangesAsync();

        var service = new NotificationService(ctx, this.loggerMock.Object);
        var ok = await service.MarkAsReadAsync(1, UserId);

        Assert.True(ok);
        var n = await ctx.Notifications.FindAsync(1);
        Assert.True(n!.IsRead);
        Assert.NotNull(n.ReadAt);
    }

    [Fact]
    public async Task MarkAsReadAsync_NotOwner_ReturnsFalse()
    {
        using var ctx = NewContext();
        ctx.Notifications.Add(Notif(1, UserId, false));
        await ctx.SaveChangesAsync();

        var service = new NotificationService(ctx, this.loggerMock.Object);
        var ok = await service.MarkAsReadAsync(1, OtherUserId);

        Assert.False(ok);
        Assert.False((await ctx.Notifications.FindAsync(1))!.IsRead);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_MarksEveryUnread_ReturnsCount()
    {
        using var ctx = NewContext();
        ctx.Notifications.AddRange(
            Notif(1, UserId, false),
            Notif(2, UserId, false),
            Notif(3, UserId, true),
            Notif(4, OtherUserId, false));
        await ctx.SaveChangesAsync();

        var service = new NotificationService(ctx, this.loggerMock.Object);
        var count = await service.MarkAllAsReadAsync(UserId);

        Assert.Equal(2, count);
        Assert.Equal(0, await service.GetUnreadCountAsync(UserId));
        Assert.False((await ctx.Notifications.FindAsync(4))!.IsRead); // other user untouched
    }

    [Fact]
    public async Task DeleteNotificationAsync_Owner_Deletes()
    {
        using var ctx = NewContext();
        ctx.Notifications.Add(Notif(1, UserId, false));
        await ctx.SaveChangesAsync();

        var service = new NotificationService(ctx, this.loggerMock.Object);
        var ok = await service.DeleteNotificationAsync(1, UserId);

        Assert.True(ok);
        Assert.Empty(ctx.Notifications);
    }

    [Fact]
    public async Task DeleteNotificationAsync_NotOwner_ReturnsFalseAndKeeps()
    {
        using var ctx = NewContext();
        ctx.Notifications.Add(Notif(1, UserId, false));
        await ctx.SaveChangesAsync();

        var service = new NotificationService(ctx, this.loggerMock.Object);
        var ok = await service.DeleteNotificationAsync(1, OtherUserId);

        Assert.False(ok);
        Assert.Single(ctx.Notifications);
    }
}
