// -----------------------------------------------------------------------
// <copyright file="MarketplaceServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Common.Services;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="MarketplaceService"/> — sharing, browsing and importing.
/// </summary>
public class MarketplaceServiceTests
{
    private const int OwnerId = 1;
    private const int OtherUserId = 2;

    private readonly Mock<ILogger<MarketplaceService>> loggerMock = new();

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"MarketTests_{Guid.NewGuid()}")
            .Options);

    private static void SeedUser(AppDbContext ctx, int id, string nickname)
    {
        ctx.Users.Add(new User { Id = id, Nickname = nickname, Email = $"u{id}@x.io", PasswordHash = "x" });
        ctx.SaveChanges();
    }

    private static World SeedWorld(AppDbContext ctx, int id, int userId, GameType type = GameType.DnD5e, bool shared = false, bool active = true, string name = "Monde")
    {
        var w = new World { Id = id, UserId = userId, Name = name, GameType = type, IsShared = shared, IsActive = active, CreatedAt = DateTime.UtcNow };
        ctx.Worlds.Add(w);
        ctx.SaveChanges();
        return w;
    }

    private static Campaign SeedCampaign(AppDbContext ctx, int id, int worldId, int createdBy, bool shared = false, string name = "Campagne")
    {
        var c = new Campaign { Id = id, Name = name, WorldId = worldId, CreatedBy = createdBy, IsShared = shared, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        ctx.Campaigns.Add(c);
        ctx.SaveChanges();
        return c;
    }

    private static Character SeedCharacter(AppDbContext ctx, int id, int userId, bool baseChar = true, bool shared = false, string name = "Perso")
    {
        var ch = new Character { Id = id, UserId = userId, Name = name, FirstName = "Aria", IsBaseCharacter = baseChar, IsShared = shared, IsActive = true, CreatedAt = DateTime.UtcNow };
        ctx.Characters.Add(ch);
        ctx.SaveChanges();
        return ch;
    }

    private MarketplaceService NewService(AppDbContext ctx) => new(ctx, this.loggerMock.Object);

    // ── Sharing toggles ──────────────────────────────────────────────────

    [Fact]
    public async Task SetWorldSharedAsync_OwnerCanShare()
    {
        using var ctx = NewContext();
        SeedWorld(ctx, 10, OwnerId);
        var service = this.NewService(ctx);

        var ok = await service.SetWorldSharedAsync(10, OwnerId, true);

        Assert.True(ok);
        Assert.True((await ctx.Worlds.FindAsync(10))!.IsShared);
    }

    [Fact]
    public async Task SetWorldSharedAsync_NonOwnerRejected()
    {
        using var ctx = NewContext();
        SeedWorld(ctx, 10, OwnerId);
        var service = this.NewService(ctx);

        var ok = await service.SetWorldSharedAsync(10, OtherUserId, true);

        Assert.False(ok);
        Assert.False((await ctx.Worlds.FindAsync(10))!.IsShared);
    }

    [Fact]
    public async Task SetCharacterSharedAsync_RejectsNonBaseCharacter()
    {
        using var ctx = NewContext();
        SeedCharacter(ctx, 10, OwnerId, baseChar: false);
        var service = this.NewService(ctx);

        var ok = await service.SetCharacterSharedAsync(10, OwnerId, true);

        Assert.False(ok);
    }

    // ── Browse ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSharedWorldsAsync_ReturnsOnlySharedActiveWithOwnerName()
    {
        using var ctx = NewContext();
        SeedUser(ctx, OwnerId, "Tommy");
        SeedWorld(ctx, 10, OwnerId, shared: true, name: "Shared");
        SeedWorld(ctx, 11, OwnerId, shared: false, name: "Private");
        SeedWorld(ctx, 12, OwnerId, shared: true, active: false, name: "Deleted");
        var service = this.NewService(ctx);

        var result = (await service.GetSharedWorldsAsync()).ToList();

        var entry = Assert.Single(result);
        Assert.Equal("Shared", entry.Name);
        Assert.Equal("Tommy", entry.SharedByName);
    }

    [Fact]
    public async Task GetSharedWorldsAsync_FiltersByGameType()
    {
        using var ctx = NewContext();
        SeedUser(ctx, OwnerId, "Tommy");
        SeedWorld(ctx, 10, OwnerId, type: GameType.DnD5e, shared: true);
        SeedWorld(ctx, 11, OwnerId, type: GameType.CallOfCthulhu, shared: true);
        var service = this.NewService(ctx);

        var result = (await service.GetSharedWorldsAsync(GameType.CallOfCthulhu)).ToList();

        var entry = Assert.Single(result);
        Assert.Equal(GameType.CallOfCthulhu, entry.GameType);
    }

    [Fact]
    public async Task GetSharedCampaignsAsync_UsesWorldGameType()
    {
        using var ctx = NewContext();
        SeedUser(ctx, OwnerId, "Tommy");
        SeedWorld(ctx, 10, OwnerId, type: GameType.Pathfinder);
        SeedCampaign(ctx, 20, worldId: 10, createdBy: OwnerId, shared: true);
        var service = this.NewService(ctx);

        var result = (await service.GetSharedCampaignsAsync()).ToList();

        var entry = Assert.Single(result);
        Assert.Equal(GameType.Pathfinder, entry.GameType);
        Assert.Equal("Tommy", entry.SharedByName);
    }

    [Fact]
    public async Task GetSharedCharactersAsync_ReturnsOnlySharedBaseCharacters()
    {
        using var ctx = NewContext();
        SeedUser(ctx, OwnerId, "Tommy");
        SeedCharacter(ctx, 30, OwnerId, baseChar: true, shared: true, name: "Base");
        SeedCharacter(ctx, 31, OwnerId, baseChar: false, shared: true, name: "Copy");
        SeedCharacter(ctx, 32, OwnerId, baseChar: true, shared: false, name: "Private");
        var service = this.NewService(ctx);

        var result = (await service.GetSharedCharactersAsync()).ToList();

        var entry = Assert.Single(result);
        Assert.Equal(GameType.Generic, entry.GameType);
        Assert.Equal("Aria", entry.Name);
    }

    // ── Import ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ImportWorldAsync_CreatesIndependentCopyOwnedByUser()
    {
        using var ctx = NewContext();
        SeedWorld(ctx, 10, OwnerId, type: GameType.DnD5e, shared: true, name: "Faerûn");
        var service = this.NewService(ctx);

        var (success, error) = await service.ImportWorldAsync(10, OtherUserId);

        Assert.True(success);
        Assert.Null(error);
        var copy = await ctx.Worlds.SingleAsync(w => w.UserId == OtherUserId);
        Assert.Equal("Faerûn", copy.Name);
        Assert.False(copy.IsShared);
        Assert.NotEqual(10, copy.Id);
    }

    [Fact]
    public async Task ImportWorldAsync_RejectsUnsharedWorld()
    {
        using var ctx = NewContext();
        SeedWorld(ctx, 10, OwnerId, shared: false);
        var service = this.NewService(ctx);

        var (success, _) = await service.ImportWorldAsync(10, OtherUserId);

        Assert.False(success);
    }

    [Fact]
    public async Task ImportCampaignAsync_RejectsIncompatibleWorldType()
    {
        using var ctx = NewContext();
        SeedWorld(ctx, 10, OwnerId, type: GameType.DnD5e);
        SeedCampaign(ctx, 20, worldId: 10, createdBy: OwnerId, shared: true);
        var target = SeedWorld(ctx, 11, OtherUserId, type: GameType.CallOfCthulhu);
        var service = this.NewService(ctx);

        var (success, error) = await service.ImportCampaignAsync(20, target.Id, OtherUserId);

        Assert.False(success);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ImportCampaignAsync_CopiesCampaignAndChapters()
    {
        using var ctx = NewContext();
        SeedWorld(ctx, 10, OwnerId, type: GameType.DnD5e);
        SeedCampaign(ctx, 20, worldId: 10, createdBy: OwnerId, shared: true, name: "La Malédiction");
        ctx.Chapters.Add(new Chapter { Id = 100, CampaignId = 20, ChapterNumber = 1, Title = "Prologue", Content = "…", IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.Chapters.Add(new Chapter { Id = 101, CampaignId = 20, ChapterNumber = 2, Title = "Le donjon", Content = "…", IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();
        var target = SeedWorld(ctx, 11, OtherUserId, type: GameType.DnD5e);
        var service = this.NewService(ctx);

        var (success, error) = await service.ImportCampaignAsync(20, target.Id, OtherUserId);

        Assert.True(success);
        Assert.Null(error);
        var copy = await ctx.Campaigns.SingleAsync(c => c.CreatedBy == OtherUserId);
        Assert.Equal("La Malédiction", copy.Name);
        Assert.Equal(target.Id, copy.WorldId);
        Assert.False(copy.IsShared);
        var chapters = await ctx.Chapters.Where(ch => ch.CampaignId == copy.Id).ToListAsync();
        Assert.Equal(2, chapters.Count);
        Assert.Contains(chapters, ch => ch.Title == "Prologue");
    }

    [Fact]
    public async Task ImportCampaignAsync_RejectsWorldNotOwnedByUser()
    {
        using var ctx = NewContext();
        SeedWorld(ctx, 10, OwnerId, type: GameType.DnD5e);
        SeedCampaign(ctx, 20, worldId: 10, createdBy: OwnerId, shared: true);
        SeedWorld(ctx, 11, OwnerId, type: GameType.DnD5e); // target owned by someone else
        var service = this.NewService(ctx);

        var (success, _) = await service.ImportCampaignAsync(20, 11, OtherUserId);

        Assert.False(success);
    }

    [Fact]
    public async Task ImportCharacterAsync_CopiesBaseCharacter()
    {
        using var ctx = NewContext();
        SeedCharacter(ctx, 30, OwnerId, baseChar: true, shared: true, name: "Legolas");
        var service = this.NewService(ctx);

        var (success, error) = await service.ImportCharacterAsync(30, OtherUserId);

        Assert.True(success);
        Assert.Null(error);
        var copy = await ctx.Characters.SingleAsync(c => c.UserId == OtherUserId);
        Assert.Equal("Legolas", copy.Name);
        Assert.True(copy.IsBaseCharacter);
        Assert.False(copy.IsShared);
    }

    [Fact]
    public async Task ImportCharacterAsync_RejectsUnsharedCharacter()
    {
        using var ctx = NewContext();
        SeedCharacter(ctx, 30, OwnerId, baseChar: true, shared: false);
        var service = this.NewService(ctx);

        var (success, _) = await service.ImportCharacterAsync(30, OtherUserId);

        Assert.False(success);
    }
}
