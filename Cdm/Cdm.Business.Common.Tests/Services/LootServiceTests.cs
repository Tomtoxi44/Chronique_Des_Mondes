// -----------------------------------------------------------------------
// <copyright file="LootServiceTests.cs" company="ANGIBAUD Tommy">
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
/// Unit tests for <see cref="LootService"/> — GM-scoped CRUD + in-session distribution.
/// </summary>
public class LootServiceTests
{
    private const int GmId = 1;
    private const int PlayerId = 2;
    private const int OtherUserId = 3;

    private readonly Mock<ILogger<LootService>> loggerMock = new();

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"LootTests_{Guid.NewGuid()}")
            .Options);

    /// <summary>Seeds a world + campaign owned by the GM. Returns the campaign id.</summary>
    private static int SeedCampaign(AppDbContext ctx, GameType type = GameType.DnD5e, int worldId = 100, int campaignId = 200)
    {
        ctx.Worlds.Add(new World { Id = worldId, UserId = GmId, Name = "Monde", GameType = type, IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.Campaigns.Add(new Campaign { Id = campaignId, Name = "Campagne", WorldId = worldId, CreatedBy = GmId, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();
        return campaignId;
    }

    /// <summary>Seeds a player's world character in the given world. Returns its id.</summary>
    private static int SeedWorldCharacter(AppDbContext ctx, int worldId, int userId, int charId = 300, int worldCharId = 400)
    {
        ctx.Characters.Add(new Character { Id = charId, UserId = userId, Name = "Doe", FirstName = "Aria", IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.WorldCharacters.Add(new WorldCharacter { Id = worldCharId, CharacterId = charId, WorldId = worldId, IsActive = true });
        ctx.SaveChanges();
        return worldCharId;
    }

    private LootService NewService(AppDbContext ctx) => new(ctx, this.loggerMock.Object);

    private static CreateLootDto NewLootDto(string name = "Épée +1", int qty = 1) => new()
    {
        Name = name,
        Description = "Une lame enchantée.",
        ItemType = "Arme",
        Quantity = qty,
    };

    // ── CRUD ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ThemedLoot_FollowsWorldGameType()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx, GameType.CallOfCthulhu);
        var service = this.NewService(ctx);

        // GM picks the campaign's system (not generic) → loot follows the world's game type.
        var dto = NewLootDto();
        dto.GameType = GameType.CallOfCthulhu;
        var result = await service.CreateAsync(campaignId, dto, GmId);

        Assert.NotNull(result);
        Assert.Equal("Épée +1", result!.Name);
        Assert.Equal(GameType.CallOfCthulhu, result.GameType);
        Assert.Equal(campaignId, result.CampaignId);
    }

    [Fact]
    public async Task CreateAsync_GenericLoot_StaysGeneric()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx, GameType.CallOfCthulhu);
        var service = this.NewService(ctx);

        // GM marks the loot generic → it stays generic even in a themed campaign.
        var dto = NewLootDto();
        dto.GameType = GameType.Generic;
        var result = await service.CreateAsync(campaignId, dto, GmId);

        Assert.NotNull(result);
        Assert.Equal(GameType.Generic, result!.GameType);
    }

    [Fact]
    public async Task CreateAsync_IncompatibleThemeForcedToWorldType()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx, GameType.CallOfCthulhu);
        var service = this.NewService(ctx);

        // GM sends a mismatched theme → server forces the campaign world's type (never a wrong theme).
        var dto = NewLootDto();
        dto.GameType = GameType.DnD5e;
        var result = await service.CreateAsync(campaignId, dto, GmId);

        Assert.NotNull(result);
        Assert.Equal(GameType.CallOfCthulhu, result!.GameType);
    }

    [Fact]
    public async Task CreateAsync_NonGmRejected()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx);
        var service = this.NewService(ctx);

        var result = await service.CreateAsync(campaignId, NewLootDto(), OtherUserId);

        Assert.Null(result);
        Assert.Empty(ctx.CampaignLoots);
    }

    [Fact]
    public async Task CreateAsync_SeedsFromCodexItem()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx);
        ctx.CodexItems.Add(new CodexItem { Id = 500, UserId = GmId, Name = "Potion", Description = "Soigne 2d4", ItemType = "Potion", GameType = GameType.DnD5e, IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();
        var service = this.NewService(ctx);

        var dto = new CreateLootDto { Name = string.Empty, SourceCodexItemId = 500 };
        var result = await service.CreateAsync(campaignId, dto, GmId);

        Assert.NotNull(result);
        Assert.Equal("Potion", result!.Name);
        Assert.Equal("Potion", result.ItemType);
    }

    [Fact]
    public async Task CreateAsync_RejectsChapterFromAnotherCampaign()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx);
        ctx.Chapters.Add(new Chapter { Id = 700, CampaignId = 999, ChapterNumber = 1, Title = "Ailleurs", IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();
        var service = this.NewService(ctx);

        var dto = NewLootDto();
        dto.ChapterId = 700;
        var result = await service.CreateAsync(campaignId, dto, GmId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCampaignLootAsync_ReturnsActiveForGmOnly()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx);
        var service = this.NewService(ctx);
        await service.CreateAsync(campaignId, NewLootDto("A"), GmId);
        await service.CreateAsync(campaignId, NewLootDto("B"), GmId);

        var forGm = (await service.GetCampaignLootAsync(campaignId, GmId)).ToList();
        var forOther = (await service.GetCampaignLootAsync(campaignId, OtherUserId)).ToList();

        Assert.Equal(2, forGm.Count);
        Assert.Empty(forOther);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesAndHides()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx);
        var service = this.NewService(ctx);
        var loot = await service.CreateAsync(campaignId, NewLootDto(), GmId);

        var ok = await service.DeleteAsync(loot!.Id, GmId);

        Assert.True(ok);
        Assert.Empty(await service.GetCampaignLootAsync(campaignId, GmId));
    }

    // ── Distribution ─────────────────────────────────────────────────────

    [Fact]
    public async Task DistributeAsync_CopiesToInventoryAndRecords()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx);
        var worldCharId = SeedWorldCharacter(ctx, worldId: 100, userId: PlayerId);
        var service = this.NewService(ctx);
        var loot = await service.CreateAsync(campaignId, NewLootDto("Bouclier", qty: 2), GmId);

        var (success, error, result) = await service.DistributeAsync(loot!.Id, worldCharId, sessionId: 55, GmId);

        Assert.True(success);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal("Bouclier", result!.LootName);
        Assert.Equal(PlayerId, result.RecipientUserId);
        Assert.Equal("Aria", result.RecipientName);

        var inv = Assert.Single(ctx.DndInventoryItems);
        Assert.Equal(worldCharId, inv.WorldCharacterId);
        Assert.Equal(2, inv.Quantity);

        var dist = Assert.Single(ctx.LootDistributions);
        Assert.Equal(55, dist.SessionId);
        Assert.Equal(GmId, dist.DistributedByUserId);
    }

    [Fact]
    public async Task DistributeAsync_NonGmRejected()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx);
        var worldCharId = SeedWorldCharacter(ctx, worldId: 100, userId: PlayerId);
        var service = this.NewService(ctx);
        var loot = await service.CreateAsync(campaignId, NewLootDto(), GmId);

        var (success, _, _) = await service.DistributeAsync(loot!.Id, worldCharId, null, OtherUserId);

        Assert.False(success);
        Assert.Empty(ctx.DndInventoryItems);
    }

    [Fact]
    public async Task DistributeAsync_RejectsCharacterOutsideCampaignWorld()
    {
        using var ctx = NewContext();
        var campaignId = SeedCampaign(ctx, worldId: 100);
        // Character lives in a different world (101).
        ctx.Worlds.Add(new World { Id = 101, UserId = OtherUserId, Name = "Autre", GameType = GameType.DnD5e, IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();
        var worldCharId = SeedWorldCharacter(ctx, worldId: 101, userId: PlayerId);
        var service = this.NewService(ctx);
        var loot = await service.CreateAsync(campaignId, NewLootDto(), GmId);

        var (success, error, _) = await service.DistributeAsync(loot!.Id, worldCharId, null, GmId);

        Assert.False(success);
        Assert.NotNull(error);
        Assert.Empty(ctx.DndInventoryItems);
    }
}
