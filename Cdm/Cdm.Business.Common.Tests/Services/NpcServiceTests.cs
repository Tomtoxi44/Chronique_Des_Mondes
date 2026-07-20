// -----------------------------------------------------------------------
// <copyright file="NpcServiceTests.cs" company="ANGIBAUD Tommy">
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
/// Unit tests for <see cref="NpcService"/>, focusing on GM authorization and CRUD behavior.
/// </summary>
public class NpcServiceTests
{
    private const int GmId = 1;
    private const int OtherUserId = 2;
    private const int ChapterId = 10;

    private readonly Mock<ILogger<NpcService>> loggerMock = new();

    /// <summary>
    /// Creates an in-memory context seeded with a world (owned by <see cref="GmId"/>),
    /// a campaign and an active chapter.
    /// </summary>
    private static AppDbContext NewSeededContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"NpcTests_{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);
        context.Users.Add(new User { Id = GmId, Email = "gm@test.com", Nickname = "GM", PasswordHash = "h", CreatedAt = DateTime.UtcNow });
        context.Users.Add(new User { Id = OtherUserId, Email = "other@test.com", Nickname = "Other", PasswordHash = "h", CreatedAt = DateTime.UtcNow });
        context.Worlds.Add(new World { Id = 1, Name = "World", GameType = GameType.Generic, UserId = GmId, CreatedAt = DateTime.UtcNow });
        context.Campaigns.Add(new Campaign { Id = 1, Name = "Campaign", WorldId = 1, CreatedBy = GmId, Visibility = Visibility.Private, CreatedAt = DateTime.UtcNow });
        context.Chapters.Add(new Chapter { Id = ChapterId, CampaignId = 1, ChapterNumber = 1, Title = "Chapter", IsActive = true, CreatedAt = DateTime.UtcNow });
        context.SaveChanges();
        return context;
    }

    private static CreateNpcDto NewNpcDto() => new()
    {
        ChapterId = ChapterId,
        Name = "Vann",
        FirstName = "Old",
        Description = "A mysterious merchant.",
        PhysicalDescription = "Tall, hooded.",
        Age = 52,
        GameSpecificData = "{\"hp\":8}"
    };

    [Fact]
    public async Task CreateNpcAsync_AsGm_CreatesNpc()
    {
        using var context = NewSeededContext();
        var service = new NpcService(context, this.loggerMock.Object);

        var result = await service.CreateNpcAsync(NewNpcDto(), GmId);

        Assert.NotNull(result);
        Assert.Equal("Vann", result!.Name);
        Assert.Equal(ChapterId, result.ChapterId);
        Assert.Single(context.NonPlayerCharacters);
    }

    [Fact]
    public async Task CreateNpcAsync_NotGm_ReturnsNullAndPersistsNothing()
    {
        using var context = NewSeededContext();
        var service = new NpcService(context, this.loggerMock.Object);

        var result = await service.CreateNpcAsync(NewNpcDto(), OtherUserId);

        Assert.Null(result);
        Assert.Empty(context.NonPlayerCharacters);
    }

    [Fact]
    public async Task CreateNpcAsync_AsCampaignCreatorNotWorldOwner_Succeeds()
    {
        using var context = NewSeededContext();

        // A world owned by someone else, but a campaign created by OtherUserId.
        context.Worlds.Add(new World { Id = 2, Name = "Foreign", GameType = GameType.Generic, UserId = GmId, CreatedAt = DateTime.UtcNow });
        context.Campaigns.Add(new Campaign { Id = 2, Name = "Co-run", WorldId = 2, CreatedBy = OtherUserId, Visibility = Visibility.Private, CreatedAt = DateTime.UtcNow });
        context.Chapters.Add(new Chapter { Id = 20, CampaignId = 2, ChapterNumber = 1, Title = "Ch", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new NpcService(context, this.loggerMock.Object);
        var dto = NewNpcDto();
        dto.ChapterId = 20;

        var result = await service.CreateNpcAsync(dto, OtherUserId);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetNpcsByChapterAsync_ReturnsOnlyActiveNpcs()
    {
        using var context = NewSeededContext();
        context.NonPlayerCharacters.AddRange(
            new NonPlayerCharacter { Id = 1, ChapterId = ChapterId, Name = "Active", IsActive = true, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new NonPlayerCharacter { Id = 2, ChapterId = ChapterId, Name = "Deleted", IsActive = false, CreatedAt = DateTime.UtcNow.AddMinutes(-1) });
        await context.SaveChangesAsync();

        var service = new NpcService(context, this.loggerMock.Object);

        var result = (await service.GetNpcsByChapterAsync(ChapterId, GmId)).ToList();

        Assert.Single(result);
        Assert.Equal("Active", result[0].Name);
    }

    [Fact]
    public async Task GetNpcsByChapterAsync_NotGm_ReturnsEmpty()
    {
        using var context = NewSeededContext();
        context.NonPlayerCharacters.Add(new NonPlayerCharacter { Id = 1, ChapterId = ChapterId, Name = "Secret", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new NpcService(context, this.loggerMock.Object);

        var result = await service.GetNpcsByChapterAsync(ChapterId, OtherUserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateNpcAsync_AsGm_UpdatesFields()
    {
        using var context = NewSeededContext();
        context.NonPlayerCharacters.Add(new NonPlayerCharacter { Id = 1, ChapterId = ChapterId, Name = "Old", Age = 20, IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new NpcService(context, this.loggerMock.Object);
        var dto = NewNpcDto();
        dto.Name = "Renamed";
        dto.Age = 99;

        var result = await service.UpdateNpcAsync(1, dto, GmId);

        Assert.NotNull(result);
        Assert.Equal("Renamed", result!.Name);
        Assert.Equal(99, result.Age);
    }

    [Fact]
    public async Task UpdateNpcAsync_NotGm_ReturnsNull()
    {
        using var context = NewSeededContext();
        context.NonPlayerCharacters.Add(new NonPlayerCharacter { Id = 1, ChapterId = ChapterId, Name = "Old", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new NpcService(context, this.loggerMock.Object);

        var result = await service.UpdateNpcAsync(1, NewNpcDto(), OtherUserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteNpcAsync_AsGm_SoftDeletes()
    {
        using var context = NewSeededContext();
        context.NonPlayerCharacters.Add(new NonPlayerCharacter { Id = 1, ChapterId = ChapterId, Name = "Doomed", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new NpcService(context, this.loggerMock.Object);

        var ok = await service.DeleteNpcAsync(1, GmId);

        Assert.True(ok);
        var npc = await context.NonPlayerCharacters.FindAsync(1);
        Assert.False(npc!.IsActive);
        Assert.Null(await service.GetNpcByIdAsync(1, GmId));
    }

    [Fact]
    public async Task DeleteNpcAsync_NotGm_ReturnsFalseAndKeepsNpc()
    {
        using var context = NewSeededContext();
        context.NonPlayerCharacters.Add(new NonPlayerCharacter { Id = 1, ChapterId = ChapterId, Name = "Safe", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new NpcService(context, this.loggerMock.Object);

        var ok = await service.DeleteNpcAsync(1, OtherUserId);

        Assert.False(ok);
        var npc = await context.NonPlayerCharacters.FindAsync(1);
        Assert.True(npc!.IsActive);
    }

    [Fact]
    public async Task DeleteNpcAsync_NonExistent_ReturnsFalse()
    {
        using var context = NewSeededContext();
        var service = new NpcService(context, this.loggerMock.Object);

        var ok = await service.DeleteNpcAsync(999, GmId);

        Assert.False(ok);
    }
}
