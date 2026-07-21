// -----------------------------------------------------------------------
// <copyright file="ChapterImageService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Chapter image gallery management. The GM is the campaign creator of the chapter's campaign.
/// </summary>
public class ChapterImageService(AppDbContext dbContext, ILogger<ChapterImageService> logger) : IChapterImageService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<ChapterImageService> logger = logger;

    /// <inheritdoc/>
    public async Task<IEnumerable<ChapterImageDto>> GetForChapterAsync(int chapterId, int userId)
    {
        try
        {
            if (!await this.IsGmOfChapterAsync(chapterId, userId))
            {
                return Enumerable.Empty<ChapterImageDto>();
            }

            var images = await this.dbContext.ChapterImages
                .Where(i => i.ChapterId == chapterId)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .ToListAsync();

            return images.Select(MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error listing images for chapter {ChapterId}", chapterId);
            return Enumerable.Empty<ChapterImageDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<ChapterImageDto?> AddAsync(int chapterId, AddChapterImageDto dto, int userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.ImageUrl) || !await this.IsGmOfChapterAsync(chapterId, userId))
            {
                return null;
            }

            var nextOrder = await this.dbContext.ChapterImages
                .Where(i => i.ChapterId == chapterId)
                .Select(i => (int?)i.SortOrder)
                .MaxAsync() ?? -1;

            var image = new ChapterImage
            {
                ChapterId = chapterId,
                ImageUrl = dto.ImageUrl.Trim(),
                Caption = string.IsNullOrWhiteSpace(dto.Caption) ? null : dto.Caption.Trim(),
                SortOrder = nextOrder + 1,
                CreatedAt = DateTime.UtcNow,
            };

            this.dbContext.ChapterImages.Add(image);
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Added image {ImageId} to chapter {ChapterId}", image.Id, chapterId);
            return MapToDto(image);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error adding image to chapter {ChapterId}", chapterId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int imageId, int userId)
    {
        try
        {
            var image = await this.dbContext.ChapterImages.FirstOrDefaultAsync(i => i.Id == imageId);
            if (image == null || !await this.IsGmOfChapterAsync(image.ChapterId, userId))
            {
                return false;
            }

            this.dbContext.ChapterImages.Remove(image);
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Deleted chapter image {ImageId}", imageId);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting chapter image {ImageId}", imageId);
            return false;
        }
    }

    private async Task<bool> IsGmOfChapterAsync(int chapterId, int userId)
    {
        return await this.dbContext.Chapters
            .Include(ch => ch.Campaign)
            .AnyAsync(ch => ch.Id == chapterId && ch.Campaign.CreatedBy == userId);
    }

    private static ChapterImageDto MapToDto(ChapterImage image) => new()
    {
        Id = image.Id,
        ChapterId = image.ChapterId,
        ImageUrl = image.ImageUrl,
        Caption = image.Caption,
        SortOrder = image.SortOrder,
    };
}
