// -----------------------------------------------------------------------
// <copyright file="ChapterImageServiceTests.cs" company="ANGIBAUD Tommy">
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

/// <summary>Unit tests for <see cref="ChapterImageService"/> — GM-scoped gallery management.</summary>
public class ChapterImageServiceTests
{
    private const int GmId = 1;
    private const int OtherUserId = 2;

    private readonly Mock<ILogger<ChapterImageService>> loggerMock = new();

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ChapterImgTests_{Guid.NewGuid()}")
            .Options);

    /// <summary>Seeds a world+campaign owned by the GM and a chapter. Returns the chapter id.</summary>
    private static int SeedChapter(AppDbContext ctx, int chapterId = 500, int campaignId = 200)
    {
        ctx.Worlds.Add(new World { Id = 100, UserId = GmId, Name = "W", GameType = GameType.DnD5e, IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.Campaigns.Add(new Campaign { Id = campaignId, Name = "C", WorldId = 100, CreatedBy = GmId, IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.Chapters.Add(new Chapter { Id = chapterId, CampaignId = campaignId, ChapterNumber = 1, Title = "Ch", IsActive = true, CreatedAt = DateTime.UtcNow });
        ctx.SaveChanges();
        return chapterId;
    }

    private ChapterImageService NewService(AppDbContext ctx) => new(ctx, this.loggerMock.Object);

    [Fact]
    public async Task AddAsync_GmAdds_WithIncrementingSortOrder()
    {
        using var ctx = NewContext();
        var chapterId = SeedChapter(ctx);
        var service = this.NewService(ctx);

        var first = await service.AddAsync(chapterId, new AddChapterImageDto { ImageUrl = "/a.png", Caption = "Plan" }, GmId);
        var second = await service.AddAsync(chapterId, new AddChapterImageDto { ImageUrl = "/b.png" }, GmId);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(0, first!.SortOrder);
        Assert.Equal(1, second!.SortOrder);
        Assert.Equal("Plan", first.Caption);
    }

    [Fact]
    public async Task AddAsync_NonGmRejected()
    {
        using var ctx = NewContext();
        var chapterId = SeedChapter(ctx);
        var service = this.NewService(ctx);

        var result = await service.AddAsync(chapterId, new AddChapterImageDto { ImageUrl = "/a.png" }, OtherUserId);

        Assert.Null(result);
        Assert.Empty(ctx.ChapterImages);
    }

    [Fact]
    public async Task GetForChapterAsync_GmSees_OtherDoesNot()
    {
        using var ctx = NewContext();
        var chapterId = SeedChapter(ctx);
        var service = this.NewService(ctx);
        await service.AddAsync(chapterId, new AddChapterImageDto { ImageUrl = "/a.png" }, GmId);

        var forGm = (await service.GetForChapterAsync(chapterId, GmId)).ToList();
        var forOther = (await service.GetForChapterAsync(chapterId, OtherUserId)).ToList();

        Assert.Single(forGm);
        Assert.Empty(forOther);
    }

    [Fact]
    public async Task DeleteAsync_GmDeletes_NonGmCannot()
    {
        using var ctx = NewContext();
        var chapterId = SeedChapter(ctx);
        var service = this.NewService(ctx);
        var img = await service.AddAsync(chapterId, new AddChapterImageDto { ImageUrl = "/a.png" }, GmId);

        var byOther = await service.DeleteAsync(img!.Id, OtherUserId);
        var byGm = await service.DeleteAsync(img.Id, GmId);

        Assert.False(byOther);
        Assert.True(byGm);
        Assert.Empty(ctx.ChapterImages);
    }
}
