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

    // ── AddToCharacterInventoryAsync ─────────────────────────────────────

    /// <summary>Seeds a world character (world of <paramref name="worldType"/>) owned by <paramref name="ownerId"/>.</summary>
    private static void SeedWorldCharacter(AppDbContext ctx, int worldCharacterId, int ownerId, GameType worldType)
    {
        ctx.Users.Add(new User { Id = ownerId, Email = $"u{ownerId}@t", Nickname = $"U{ownerId}", PasswordHash = "h", CreatedAt = DateTime.UtcNow });
        ctx.Characters.Add(new Character { Id = worldCharacterId * 10, UserId = ownerId, Name = "Hero", IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.Worlds.Add(new World { Id = worldCharacterId * 10, Name = "W", GameType = worldType, UserId = ownerId, CreatedAt = DateTime.UtcNow });
        ctx.WorldCharacters.Add(new WorldCharacter { Id = worldCharacterId, CharacterId = worldCharacterId * 10, WorldId = worldCharacterId * 10, JoinedAt = DateTime.UtcNow, IsActive = true });
        ctx.SaveChanges();
    }

    [Fact]
    public async Task AddToCharacter_MatchingType_CreatesInventoryCopy()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId, GameType.DnD5e, name: "Potion de soin");
        SeedWorldCharacter(ctx, 50, OwnerId, GameType.DnD5e);
        var service = new CodexService(ctx, this.loggerMock.Object);

        var (ok, error) = await service.AddToCharacterInventoryAsync(1, 50, OwnerId);

        Assert.True(ok);
        Assert.Null(error);
        var inv = Assert.Single(ctx.DndInventoryItems);
        Assert.Equal("Potion de soin", inv.Name);
        Assert.Equal(50, inv.WorldCharacterId);
        Assert.Equal(1, inv.Quantity);
    }

    [Fact]
    public async Task AddToCharacter_DndItemWithStats_HydratesCombatColumns()
    {
        using var ctx = NewContext();
        ctx.CodexItems.Add(new CodexItem
        {
            Id = 1,
            UserId = OwnerId,
            Name = "Épée longue",
            GameType = GameType.DnD5e,
            ItemType = "Arme",
            GameSpecificData = "{\"damageDice\":\"1d8\",\"damageType\":\"tranchant\",\"attackBonus\":5}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        ctx.SaveChanges();
        SeedWorldCharacter(ctx, 50, OwnerId, GameType.DnD5e);
        var service = new CodexService(ctx, this.loggerMock.Object);

        var (ok, error) = await service.AddToCharacterInventoryAsync(1, 50, OwnerId);

        Assert.True(ok);
        Assert.Null(error);
        var inv = Assert.Single(ctx.DndInventoryItems);
        Assert.Equal("1d8", inv.DamageDice);
        Assert.Equal("tranchant", inv.DamageType);
        Assert.Equal(5, inv.AttackBonus);
    }

    [Fact]
    public async Task AddToCharacter_TypeMismatch_Fails()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId, GameType.Cyberpunk);
        SeedWorldCharacter(ctx, 50, OwnerId, GameType.DnD5e);
        var service = new CodexService(ctx, this.loggerMock.Object);

        var (ok, error) = await service.AddToCharacterInventoryAsync(1, 50, OwnerId);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Empty(ctx.DndInventoryItems);
    }

    [Fact]
    public async Task AddToCharacter_GenericItem_GoesToAnyCharacter()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId, GameType.Generic, name: "Corde");
        SeedWorldCharacter(ctx, 50, OwnerId, GameType.DnD5e);
        var service = new CodexService(ctx, this.loggerMock.Object);

        var (ok, error) = await service.AddToCharacterInventoryAsync(1, 50, OwnerId);

        Assert.True(ok);
        Assert.Null(error);
        var inv = Assert.Single(ctx.DndInventoryItems);
        Assert.Equal(GameType.Generic, inv.GameType);
    }

    [Fact]
    public async Task AddToCharacter_NotOwnerOfCharacter_Fails()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId, GameType.DnD5e);
        SeedWorldCharacter(ctx, 50, OtherUserId, GameType.DnD5e);
        var service = new CodexService(ctx, this.loggerMock.Object);

        var (ok, _) = await service.AddToCharacterInventoryAsync(1, 50, OwnerId);

        Assert.False(ok);
        Assert.Empty(ctx.DndInventoryItems);
    }

    [Fact]
    public async Task AddToCharacter_ItemNotFound_Fails()
    {
        using var ctx = NewContext();
        SeedWorldCharacter(ctx, 50, OwnerId, GameType.DnD5e);
        var service = new CodexService(ctx, this.loggerMock.Object);

        var (ok, _) = await service.AddToCharacterInventoryAsync(999, 50, OwnerId);

        Assert.False(ok);
    }

    // ── Marketplace (partage / import) ───────────────────────────────────

    private static void SeedUser(AppDbContext ctx, int id, string nickname)
    {
        ctx.Users.Add(new User { Id = id, Email = $"u{id}@t", Nickname = nickname, PasswordHash = "h", CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();
    }

    [Fact]
    public async Task SetSharedAsync_Owner_TogglesFlag()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId);
        var service = new CodexService(ctx, this.loggerMock.Object);

        Assert.True(await service.SetSharedAsync(1, OwnerId, true));
        Assert.True((await ctx.CodexItems.FindAsync(1))!.IsShared);
    }

    [Fact]
    public async Task SetSharedAsync_NotOwner_ReturnsFalse()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OwnerId);
        var service = new CodexService(ctx, this.loggerMock.Object);

        Assert.False(await service.SetSharedAsync(1, OtherUserId, true));
        Assert.False((await ctx.CodexItems.FindAsync(1))!.IsShared);
    }

    [Fact]
    public async Task GetMarketplaceItems_ReturnsOnlySharedActive_FromAnyUser_WithOwnerName()
    {
        using var ctx = NewContext();
        SeedUser(ctx, OtherUserId, "Alice");
        ctx.CodexItems.AddRange(
            new CodexItem { Id = 1, UserId = OtherUserId, Name = "Shared", GameType = GameType.DnD5e, IsShared = true, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CodexItem { Id = 2, UserId = OtherUserId, Name = "Private", GameType = GameType.DnD5e, IsShared = false, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CodexItem { Id = 3, UserId = OtherUserId, Name = "SharedButDeleted", GameType = GameType.DnD5e, IsShared = true, IsActive = false, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();

        var service = new CodexService(ctx, this.loggerMock.Object);
        var result = (await service.GetMarketplaceItemsAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Shared", result[0].Name);
        Assert.Equal("Alice", result[0].SharedByName);
    }

    [Fact]
    public async Task GetMarketplaceItems_FiltersByTypeAndSearch()
    {
        using var ctx = NewContext();
        SeedUser(ctx, OtherUserId, "Bob");
        ctx.CodexItems.AddRange(
            new CodexItem { Id = 1, UserId = OtherUserId, Name = "Épée longue", GameType = GameType.DnD5e, IsShared = true, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CodexItem { Id = 2, UserId = OtherUserId, Name = "Cyberdeck", GameType = GameType.Cyberpunk, IsShared = true, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CodexItem { Id = 3, UserId = OtherUserId, Name = "Épée courte", GameType = GameType.DnD5e, IsShared = true, IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();

        var service = new CodexService(ctx, this.loggerMock.Object);

        var byType = (await service.GetMarketplaceItemsAsync(GameType.Cyberpunk)).ToList();
        Assert.Single(byType);
        Assert.Equal("Cyberdeck", byType[0].Name);

        var bySearch = (await service.GetMarketplaceItemsAsync(null, "Épée")).ToList();
        Assert.Equal(2, bySearch.Count);
    }

    [Fact]
    public async Task ImportToMyCodex_SharedItem_CreatesIndependentCopyForImporter()
    {
        using var ctx = NewContext();
        SeedUser(ctx, OtherUserId, "Owner");
        ctx.CodexItems.Add(new CodexItem { Id = 1, UserId = OtherUserId, Name = "Grimoire", GameType = GameType.CallOfCthulhu, IsShared = true, IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();

        var service = new CodexService(ctx, this.loggerMock.Object);
        var result = await service.ImportToMyCodexAsync(1, OwnerId);

        Assert.NotNull(result);
        Assert.Equal("Grimoire", result!.Name);
        var copy = await ctx.CodexItems.FirstOrDefaultAsync(c => c.UserId == OwnerId);
        Assert.NotNull(copy);
        Assert.False(copy!.IsShared); // la copie n'est pas partagée
        Assert.NotEqual(1, copy.Id); // nouvelle ligne
    }

    [Fact]
    public async Task ImportToMyCodex_NotShared_ReturnsNull()
    {
        using var ctx = NewContext();
        Seed(ctx, 1, OtherUserId, active: true); // active but IsShared=false by default
        var service = new CodexService(ctx, this.loggerMock.Object);

        var result = await service.ImportToMyCodexAsync(1, OwnerId);

        Assert.Null(result);
    }
}
