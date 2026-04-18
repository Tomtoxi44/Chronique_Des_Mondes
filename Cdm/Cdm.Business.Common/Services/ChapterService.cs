// -----------------------------------------------------------------------
// <copyright file="ChapterService.cs" company="ANGIBAUD Tommy">
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
/// Service for managing chapter operations within campaigns.
/// </summary>
/// <param name="dbContext">Database context for chapter data access.</param>
/// <param name="logger">Logger instance for structured logging.</param>
public class ChapterService(
    AppDbContext dbContext,
    ILogger<ChapterService> logger) : IChapterService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<ChapterService> logger = logger;

    /// <inheritdoc/>
    public async Task<ChapterDto?> CreateChapterAsync(CreateChapterDto dto, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Creating chapter '{Title}' for campaign {CampaignId} by user {UserId}",
                dto.Title,
                dto.CampaignId,
                userId);

            // Verify campaign exists and user is the GM
            var campaign = await this.dbContext.Campaigns
                .Include(c => c.World)
                .FirstOrDefaultAsync(c => c.Id == dto.CampaignId && !c.IsDeleted);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign {CampaignId} not found", dto.CampaignId);
                return null;
            }

            // Only the world GM can create chapters
            if (campaign.World?.UserId != userId && campaign.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to create chapters in campaign {CampaignId}",
                    userId,
                    dto.CampaignId);
                return null;
            }

            // Auto-assign chapter number if not provided
            var chapterNumber = dto.ChapterNumber;
            if (chapterNumber <= 0)
            {
                var maxNumber = await this.dbContext.Chapters
                    .Where(ch => ch.CampaignId == dto.CampaignId && ch.IsActive)
                    .MaxAsync(ch => (int?)ch.ChapterNumber) ?? 0;
                chapterNumber = maxNumber + 1;
            }

            var chapter = new Chapter
            {
                CampaignId = dto.CampaignId,
                ChapterNumber = chapterNumber,
                Title = dto.Title,
                Content = dto.Content,
                IsCompleted = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            this.dbContext.Chapters.Add(chapter);
            await this.dbContext.SaveChangesAsync();


            this.logger.LogInformation(
                "Successfully created chapter {ChapterId} '{Title}'",
                chapter.Id,
                chapter.Title);

            return this.MapToDto(chapter);
        }
        catch (Exception ex)
        {

            this.logger.LogError(
                ex,
                "Error creating chapter '{Title}' for campaign {CampaignId}",
                dto.Title,
                dto.CampaignId);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ChapterDto>> GetChaptersByCampaignAsync(int campaignId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving chapters for campaign {CampaignId} by user {UserId}",
                campaignId,
                userId);

            // Verify campaign access
            var campaign = await this.dbContext.Campaigns
                .Include(c => c.World)
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return Enumerable.Empty<ChapterDto>();
            }

            // Check authorization (GM or participant)
            var isGm = campaign.World?.UserId == userId || campaign.CreatedBy == userId;
            var isParticipant = await this.dbContext.WorldCharacters
                .AnyAsync(wc => wc.WorldId == campaign.WorldId && wc.Character.UserId == userId && wc.IsActive);

            if (!isGm && !isParticipant)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to view chapters in campaign {CampaignId}",
                    userId,
                    campaignId);
                return Enumerable.Empty<ChapterDto>();
            }

            var chapters = await this.dbContext.Chapters
                .Include(ch => ch.Events)
                .Include(ch => ch.Achievements)
                .Where(ch => ch.CampaignId == campaignId && ch.IsActive)
                .OrderBy(ch => ch.ChapterNumber)
                .ToListAsync();

            this.logger.LogInformation(
                "Retrieved {Count} chapters for campaign {CampaignId}",
                chapters.Count,
                campaignId);

            return chapters.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error retrieving chapters for campaign {CampaignId}",
                campaignId);
            return Enumerable.Empty<ChapterDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<ChapterDto?> GetChapterByIdAsync(int chapterId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving chapter {ChapterId} for user {UserId}",
                chapterId,
                userId);

            var chapter = await this.dbContext.Chapters
                .Include(ch => ch.Campaign)
                    .ThenInclude(c => c.World)
                .Include(ch => ch.Events)
                .Include(ch => ch.Achievements)
                .FirstOrDefaultAsync(ch => ch.Id == chapterId && ch.IsActive);

            if (chapter == null)
            {
                this.logger.LogWarning("Chapter {ChapterId} not found", chapterId);
                return null;
            }

            // Check authorization
            var campaign = chapter.Campaign;
            var isGm = campaign.World?.UserId == userId || campaign.CreatedBy == userId;
            var isParticipant = await this.dbContext.WorldCharacters
                .AnyAsync(wc => wc.WorldId == campaign.WorldId && wc.Character.UserId == userId && wc.IsActive);

            if (!isGm && !isParticipant)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to view chapter {ChapterId}",
                    userId,
                    chapterId);
                return null;
            }

            this.logger.LogInformation("Successfully retrieved chapter {ChapterId}", chapterId);

            return this.MapToDto(chapter);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error retrieving chapter {ChapterId}",
                chapterId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<ChapterDto?> UpdateChapterAsync(int chapterId, CreateChapterDto request, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Updating chapter {ChapterId} by user {UserId}",
                chapterId,
                userId);

            var chapter = await this.dbContext.Chapters
                .Include(ch => ch.Campaign)
                    .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(ch => ch.Id == chapterId && ch.IsActive);

            if (chapter == null)
            {
                this.logger.LogWarning("Chapter {ChapterId} not found", chapterId);
                return null;
            }

            // Only GM can update
            var campaign = chapter.Campaign;
            if (campaign.World?.UserId != userId && campaign.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to update chapter {ChapterId}",
                    userId,
                    chapterId);
                return null;
            }

            chapter.Title = request.Title;
            chapter.Content = request.Content;
            if (request.ChapterNumber > 0)
            {
                chapter.ChapterNumber = request.ChapterNumber;
            }
            chapter.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Successfully updated chapter {ChapterId}", chapterId);

            return this.MapToDto(chapter);
        }
        catch (Exception ex)
        {

            this.logger.LogError(
                ex,
                "Error updating chapter {ChapterId}",
                chapterId);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteChapterAsync(int chapterId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Deleting chapter {ChapterId} by user {UserId}",
                chapterId,
                userId);

            var chapter = await this.dbContext.Chapters
                .Include(ch => ch.Campaign)
                    .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(ch => ch.Id == chapterId);

            if (chapter == null)
            {
                this.logger.LogWarning("Chapter {ChapterId} not found", chapterId);
                return false;
            }

            // Only GM can delete
            var campaign = chapter.Campaign;
            if (campaign.World?.UserId != userId && campaign.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to delete chapter {ChapterId}",
                    userId,
                    chapterId);
                return false;
            }

            // Soft delete
            chapter.IsActive = false;
            chapter.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Successfully deleted chapter {ChapterId}", chapterId);

            return true;
        }
        catch (Exception ex)
        {

            this.logger.LogError(
                ex,
                "Error deleting chapter {ChapterId}",
                chapterId);

            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<ChapterDto?> StartChapterAsync(int chapterId, int userId)
    {
        return await this.SetChapterStatusAsync(chapterId, userId, isCompleted: false, isStarting: true);
    }

    /// <inheritdoc/>
    public async Task<ChapterDto?> CompleteChapterAsync(int chapterId, int userId)
    {
        return await this.SetChapterStatusAsync(chapterId, userId, isCompleted: true, isStarting: false);
    }

    /// <summary>
    /// Sets the completion status of a chapter.
    /// </summary>
    private async Task<ChapterDto?> SetChapterStatusAsync(int chapterId, int userId, bool isCompleted, bool isStarting)
    {
        try
        {
            var action = isCompleted ? "completing" : (isStarting ? "starting" : "updating");
            this.logger.LogInformation(
                "{Action} chapter {ChapterId} by user {UserId}",
                action,
                chapterId,
                userId);

            var chapter = await this.dbContext.Chapters
                .Include(ch => ch.Campaign)
                    .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(ch => ch.Id == chapterId && ch.IsActive);

            if (chapter == null)
            {
                this.logger.LogWarning("Chapter {ChapterId} not found", chapterId);
                return null;
            }

            // Only GM can change status
            var campaign = chapter.Campaign;
            if (campaign.World?.UserId != userId && campaign.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to {Action} chapter {ChapterId}",
                    userId,
                    action,
                    chapterId);
                return null;
            }

            chapter.IsCompleted = isCompleted;
            chapter.UpdatedAt = DateTime.UtcNow;
            if (isCompleted)
            {
                chapter.CompletedAt = DateTime.UtcNow;
            }

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Successfully {Action} chapter {ChapterId}",
                action,
                chapterId);

            return this.MapToDto(chapter);
        }
        catch (Exception ex)
        {

            this.logger.LogError(
                ex,
                "Error changing status of chapter {ChapterId}",
                chapterId);

            return null;
        }
    }

    /// <summary>
    /// Maps a Chapter entity to a ChapterDto.
    /// </summary>
    /// <param name="chapter">Chapter entity to map.</param>
    /// <returns>Mapped ChapterDto.</returns>
    private ChapterDto MapToDto(Chapter chapter)
    {
        return new ChapterDto
        {
            Id = chapter.Id,
            CampaignId = chapter.CampaignId,
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            Content = chapter.Content,
            IsCompleted = chapter.IsCompleted,
            IsActive = chapter.IsActive,
            CreatedAt = chapter.CreatedAt,
            UpdatedAt = chapter.UpdatedAt,
            CompletedAt = chapter.CompletedAt,
            EventCount = chapter.Events?.Count(e => e.IsActive) ?? 0,
            AchievementCount = chapter.Achievements?.Count(a => a.IsActive) ?? 0
        };
    }
}
