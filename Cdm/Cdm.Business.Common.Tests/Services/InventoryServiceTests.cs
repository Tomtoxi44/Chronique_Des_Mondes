// -----------------------------------------------------------------------
// <copyright file="InventoryServiceTests.cs" company="ANGIBAUD Tommy">
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
/// Unit tests for <see cref="InventoryService"/> — universal inventory, owner-scoped,
/// with per-game-system "specify" behaviour.
/// </summary>
public class InventoryServiceTests
{
    private const int OwnerId = 1;
    private const int OtherUserId = 2;

    private readonly Mock<ILogger<InventoryService>> loggerMock = new();

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"InventoryTests_{Guid.NewGuid()}")
            .Options);

    private static int SeedCharacter(AppDbContext ctx, int userId, int charId = 300, int worldCharId = 400, int worldId = 100)
    {
        ctx.Worlds.Add(new World { Id = worldId, UserId = userId, Name = "Monde", GameType = GameType.DnD5e, IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.Characters.Add(new Character { Id = charId, UserId = userId, Name = "Doe", FirstName = "Aria", IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.WorldCharacters.Add(new WorldCharacter { Id = worldCharId, CharacterId = charId, WorldId = worldId, IsActive = true });
        ctx.SaveChanges();
        return worldCharId;
    }

    private InventoryService NewService(AppDbContext ctx) => new(ctx, this.loggerMock.Object);

    [Fact]
    public async Task AddAsync_GenericItem_HasNoCombatFields()
    {
        using var ctx = NewContext();
        var wcId = SeedCharacter(ctx, OwnerId);
        var service = this.NewService(ctx);

        var dto = new CreateInventoryItemDto { Name = "Corde", Category = "Objet", GameType = GameType.Generic, DamageDice = "1d6" };
        var result = await service.AddAsync(wcId, dto, OwnerId);

        Assert.NotNull(result);
        Assert.Equal(GameType.Generic, result!.GameType);
        Assert.Null(result.DamageDice); // combat fields ignored for non-D&D
    }

    [Fact]
    public async Task AddAsync_DndItem_KeepsCombatFields()
    {
        using var ctx = NewContext();
        var wcId = SeedCharacter(ctx, OwnerId);
        var service = this.NewService(ctx);

        var dto = new CreateInventoryItemDto { Name = "Épée", Category = "Arme", GameType = GameType.DnD5e, DamageDice = "1d8", DamageType = "tranchant", AttackBonus = 5 };
        var result = await service.AddAsync(wcId, dto, OwnerId);

        Assert.NotNull(result);
        Assert.Equal("1d8", result!.DamageDice);
        Assert.Equal(5, result.AttackBonus);
    }

    [Fact]
    public async Task AddAsync_NonOwnerRejected()
    {
        using var ctx = NewContext();
        var wcId = SeedCharacter(ctx, OwnerId);
        var service = this.NewService(ctx);

        var result = await service.AddAsync(wcId, new CreateInventoryItemDto { Name = "X" }, OtherUserId);

        Assert.Null(result);
        Assert.Empty(ctx.DndInventoryItems);
    }

    [Fact]
    public async Task GetForCharacterAsync_ReturnsItemsForOwnerOnly()
    {
        using var ctx = NewContext();
        var wcId = SeedCharacter(ctx, OwnerId);
        var service = this.NewService(ctx);
        await service.AddAsync(wcId, new CreateInventoryItemDto { Name = "A" }, OwnerId);
        await service.AddAsync(wcId, new CreateInventoryItemDto { Name = "B" }, OwnerId);

        var forOwner = (await service.GetForCharacterAsync(wcId, OwnerId)).ToList();
        var forOther = (await service.GetForCharacterAsync(wcId, OtherUserId)).ToList();

        Assert.Equal(2, forOwner.Count);
        Assert.Empty(forOther);
    }

    [Fact]
    public async Task GetForCharacterAsGmAsync_WorldOwnerSeesInventory_StrangerDoesNot()
    {
        using var ctx = NewContext();
        // World owned by the GM (OwnerId); the character belongs to a different player.
        ctx.Worlds.Add(new World { Id = 100, UserId = OwnerId, Name = "W", GameType = GameType.DnD5e, IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.Characters.Add(new Character { Id = 300, UserId = OtherUserId, Name = "Joueur", IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.WorldCharacters.Add(new WorldCharacter { Id = 400, CharacterId = 300, WorldId = 100, IsActive = true });
        ctx.DndInventoryItems.Add(new DndInventoryItem { WorldCharacterId = 400, Name = "Loot", Category = "Objet", Quantity = 1, GameType = GameType.Generic, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();
        var service = this.NewService(ctx);

        var asGm = (await service.GetForCharacterAsGmAsync(400, OwnerId)).ToList();
        var asStranger = (await service.GetForCharacterAsGmAsync(400, 999)).ToList();

        Assert.Single(asGm);
        Assert.Empty(asStranger);
    }

    [Fact]
    public async Task UpdateAsync_SpecifyGenericItemToDnd()
    {
        using var ctx = NewContext();
        var wcId = SeedCharacter(ctx, OwnerId);
        var service = this.NewService(ctx);
        var generic = await service.AddAsync(wcId, new CreateInventoryItemDto { Name = "Bâton", GameType = GameType.Generic }, OwnerId);

        var dto = new CreateInventoryItemDto { Name = "Bâton", Category = "Arme", GameType = GameType.DnD5e, DamageDice = "1d6", DamageType = "contondant" };
        var result = await service.UpdateAsync(generic!.Id, dto, OwnerId);

        Assert.NotNull(result);
        Assert.Equal(GameType.DnD5e, result!.GameType);
        Assert.Equal("1d6", result.DamageDice);
    }

    [Fact]
    public async Task UpdateAsync_ChangingAwayFromDndClearsCombatFields()
    {
        using var ctx = NewContext();
        var wcId = SeedCharacter(ctx, OwnerId);
        var service = this.NewService(ctx);
        var dnd = await service.AddAsync(wcId, new CreateInventoryItemDto { Name = "Épée", GameType = GameType.DnD5e, DamageDice = "1d8" }, OwnerId);

        var dto = new CreateInventoryItemDto { Name = "Épée décorative", GameType = GameType.Generic };
        var result = await service.UpdateAsync(dnd!.Id, dto, OwnerId);

        Assert.NotNull(result);
        Assert.Null(result!.DamageDice);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedItem()
    {
        using var ctx = NewContext();
        var wcId = SeedCharacter(ctx, OwnerId);
        var service = this.NewService(ctx);
        var item = await service.AddAsync(wcId, new CreateInventoryItemDto { Name = "Jeter" }, OwnerId);

        var ok = await service.DeleteAsync(item!.Id, OwnerId);

        Assert.True(ok);
        Assert.Empty(ctx.DndInventoryItems);
    }

    [Fact]
    public async Task DeleteAsync_NonOwnerRejected()
    {
        using var ctx = NewContext();
        var wcId = SeedCharacter(ctx, OwnerId);
        var service = this.NewService(ctx);
        var item = await service.AddAsync(wcId, new CreateInventoryItemDto { Name = "X" }, OwnerId);

        var ok = await service.DeleteAsync(item!.Id, OtherUserId);

        Assert.False(ok);
        Assert.Single(ctx.DndInventoryItems);
    }
}
