// -----------------------------------------------------------------------
// <copyright file="CodexServiceTests.cs" company="ANGIBAUD Tommy">
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
/// Unit tests for <see cref="CodexService"/> — CRUD + ownership scoping.
/// </summary>
public class CodexServiceTests
{
    private const int OwnerId = 1;
    private const int OtherUserId = 2;

    private readonly Mock<ILogger<CodexService>> loggerMock = new();

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"CodexTests_{Guid.NewGuid()}")
            .Options);

    private static CreateCodexItemDto NewDto(string name = "Épée courte", GameType type = GameType.DnD5e) => new()
    {
        Name = name,
        Description = "Une lame fiable.",
        ImageUrl = "/uploads/items/x.png",
        GameType = type,
        ItemType = "Arme",
        GameSpecificData = "{\"damage\":\"1d6\"}",
    };

    private static CodexItem Seed(AppDbContext ctx, int id, int userId, GameType type = GameType.DnD5e, bool active = true, string name = "Item")
    {
        var item = new CodexItem { Id = id, UserId = userId, Name = name, GameType = type, IsActive = active, CreatedAt = DateTime.UtcNow };
        ctx.CodexItems.Add(item);
        ctx.SaveChanges();
        return item;
    }

    [Fact]
    public async Task CreateAsync_PersistsOwnedItem()
    {
        using var ctx = NewContext();
        var service = new CodexService(ctx, this.loggerMock.Object);

        var result = await service.CreateAsync(NewDto(), OwnerId);

        Assert.NotNull(result);
        Assert.Equal("Épée courte", result!.Name);
        var stored = Assert.Single(ctx.CodexItems);
        Assert.Equal(OwnerId, stored.UserId);
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task GetMyItemsAsync_ReturnsOnlyOwnActive()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId, name: "Mine");
        Seed(ctx, 2, OwnerId, active: false, name: "Deleted");
        Seed(ctx, 3, OtherUserId, name: "Theirs");

        var service = new CodexService(ctx, this.loggerMock.Object);
        var result = (await service.GetMyItemsAsync(OwnerId)).ToList();

        Assert.Single(result);
        Assert.Equal("Mine", result[0].Name);
    }

    [Fact]
    public async Task GetMyItemsAsync_FiltersByGameType()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId, GameType.DnD5e, name: "Sword");
        Seed(ctx, 2, OwnerId, GameType.Cyberpunk, name: "Cyberdeck");

        var service = new CodexService(ctx, this.loggerMock.Object);
        var result = (await service.GetMyItemsAsync(OwnerId, GameType.Cyberpunk)).ToList();

        Assert.Single(result);
        Assert.Equal("Cyberdeck", result[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_NotOwner_ReturnsNull()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId);
        var service = new CodexService(ctx, this.loggerMock.Object);

        Assert.Null(await service.GetByIdAsync(1, OtherUserId));
    }

    [Fact]
    public async Task UpdateAsync_Owner_UpdatesFields()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId, name: "Old");
        var service = new CodexService(ctx, this.loggerMock.Object);

        var dto = NewDto(name: "New", type: GameType.Warhammer);
        var result = await service.UpdateAsync(1, dto, OwnerId);

        Assert.NotNull(result);
        Assert.Equal("New", result!.Name);
        Assert.Equal(GameType.Warhammer, result.GameType);
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ReturnsNull()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId, name: "Untouched");
        var service = new CodexService(ctx, this.loggerMock.Object);

        var result = await service.UpdateAsync(1, NewDto(name: "Hacked"), OtherUserId);

        Assert.Null(result);
        Assert.Equal("Untouched", (await ctx.CodexItems.FindAsync(1))!.Name);
    }

    [Fact]
    public async Task DeleteAsync_Owner_SoftDeletes()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId);
        var service = new CodexService(ctx, this.loggerMock.Object);

        var ok = await service.DeleteAsync(1, OwnerId);

        Assert.True(ok);
        Assert.False((await ctx.CodexItems.FindAsync(1))!.IsActive);
        Assert.Null(await service.GetByIdAsync(1, OwnerId));
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ReturnsFalse()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId);
        var service = new CodexService(ctx, this.loggerMock.Object);

        var ok = await service.DeleteAsync(1, OtherUserId);

        Assert.False(ok);
        Assert.True((await ctx.CodexItems.FindAsync(1))!.IsActive);
    }
}
