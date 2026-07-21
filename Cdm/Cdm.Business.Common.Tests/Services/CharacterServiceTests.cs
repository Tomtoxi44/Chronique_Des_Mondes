// -----------------------------------------------------------------------
// <copyright file="CharacterServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Common.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="CharacterService"/> — base-character CRUD and ownership.
/// </summary>
public class CharacterServiceTests
{
    private const int OwnerId = 1;
    private const int OtherUserId = 2;

    private readonly Mock<ILogger<CharacterService>> loggerMock = new();

    private static AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"CharTests_{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options);
    }

    private static Character SeedCharacter(AppDbContext ctx, int id, int userId, string name = "Kaelan", bool active = true)
    {
        var c = new Character { Id = id, UserId = userId, Name = name, FirstName = name, IsActive = active, CreatedAt = DateTime.UtcNow };
        ctx.Characters.Add(c);
        ctx.SaveChanges();
        return c;
    }

    [Fact]
    public async Task CreateCharacterAsync_Success_PersistsAndReturnsDto()
    {
        using var ctx = NewContext();
        var service = new CharacterService(ctx, this.loggerMock.Object);

        var dto = new CreateCharacterDto { Name = "Kaelan", FirstName = "Kae", Description = "A ranger.", Age = 30 };
        var result = await service.CreateCharacterAsync(dto, OwnerId);

        Assert.NotNull(result);
        Assert.Equal("Kaelan", result!.Name);
        var stored = Assert.Single(ctx.Characters);
        Assert.Equal(OwnerId, stored.UserId);
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task CreateCharacterAsync_EmptyName_FallsBackToFirstName()
    {
        using var ctx = NewContext();
        var service = new CharacterService(ctx, this.loggerMock.Object);

        var dto = new CreateCharacterDto { Name = string.Empty, FirstName = "Solo" };
        var result = await service.CreateCharacterAsync(dto, OwnerId);

        Assert.NotNull(result);
        Assert.Equal("Solo", result!.Name);
    }

    [Fact]
    public async Task GetMyCharactersAsync_ReturnsOnlyOwnActiveCharacters()
    {
        using var ctx = NewContext();
        SeedCharacter(ctx, 1, OwnerId, "Mine-Active");
        SeedCharacter(ctx, 2, OwnerId, "Mine-Deleted", active: false);
        SeedCharacter(ctx, 3, OtherUserId, "Theirs");

        var service = new CharacterService(ctx, this.loggerMock.Object);
        var result = (await service.GetMyCharactersAsync(OwnerId)).ToList();

        Assert.Single(result);
        Assert.Equal("Mine-Active", result[0].Name);
    }

    [Fact]
    public async Task GetCharacterByIdAsync_NotOwner_ReturnsNull()
    {
        using var ctx = NewContext();
        SeedCharacter(ctx, 1, OwnerId);

        var service = new CharacterService(ctx, this.loggerMock.Object);
        var result = await service.GetCharacterByIdAsync(1, OtherUserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateCharacterAsync_Owner_UpdatesFields()
    {
        using var ctx = NewContext();
        SeedCharacter(ctx, 1, OwnerId, "Old");

        var service = new CharacterService(ctx, this.loggerMock.Object);
        var dto = new UpdateCharacterDto { Name = "New", FirstName = "N", Age = 42 };
        var result = await service.UpdateCharacterAsync(1, dto, OwnerId);

        Assert.NotNull(result);
        Assert.Equal("New", result!.Name);
        Assert.Equal(42, result.Age);
    }

    [Fact]
    public async Task UpdateCharacterAsync_NotOwner_ReturnsNullAndKeepsData()
    {
        using var ctx = NewContext();
        SeedCharacter(ctx, 1, OwnerId, "Untouched");

        var service = new CharacterService(ctx, this.loggerMock.Object);
        var result = await service.UpdateCharacterAsync(1, new UpdateCharacterDto { Name = "Hacked" }, OtherUserId);

        Assert.Null(result);
        Assert.Equal("Untouched", (await ctx.Characters.FindAsync(1))!.Name);
    }

    [Fact]
    public async Task DeleteCharacterAsync_Owner_SoftDeletes()
    {
        using var ctx = NewContext();
        SeedCharacter(ctx, 1, OwnerId);

        var service = new CharacterService(ctx, this.loggerMock.Object);
        var ok = await service.DeleteCharacterAsync(1, OwnerId);

        Assert.True(ok);
        Assert.False((await ctx.Characters.FindAsync(1))!.IsActive);
        Assert.Null(await service.GetCharacterByIdAsync(1, OwnerId));
    }

    [Fact]
    public async Task DeleteCharacterAsync_NotOwner_ReturnsFalseAndKeepsCharacter()
    {
        using var ctx = NewContext();
        SeedCharacter(ctx, 1, OwnerId);

        var service = new CharacterService(ctx, this.loggerMock.Object);
        var ok = await service.DeleteCharacterAsync(1, OtherUserId);

        Assert.False(ok);
        Assert.True((await ctx.Characters.FindAsync(1))!.IsActive);
    }
}
