// -----------------------------------------------------------------------
// <copyright file="AchievementService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing achievements and their attribution to users.
/// </summary>
/// <param name="dbContext">Database context for achievement data access.</param>
/// <param name="logger">Logger instance for structured logging.</param>
public class AchievementService(
    AppDbContext dbContext,
    ILogger<AchievementService> logger) : IAchievementService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<AchievementService> logger = logger;

    /// <inheritdoc/>
    public async Task<AchievementDto?> CreateAchievementAsync(CreateAchievementDto dto, int userId)
    {
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

        try
        {
            this.logger.LogInformation(
                "Creating achievement '{Name}' at level {Level} by user {UserId}",
                dto.Name,
                dto.Level,
                userId);

            // Validate level and corresponding ID
            var validationResult = await this.ValidateAchievementLevelAsync(dto, userId);
            if (!validationResult.IsValid)
            {
                this.logger.LogWarning(validationResult.ErrorMessage);
                return null;
            }

            var achievement = new Achievement
            {
                Name = dto.Name,
                Description = dto.Description,
                Level = dto.Level,
                WorldId = dto.Level == AchievementLevel.World ? dto.WorldId : null,
                CampaignId = dto.Level == AchievementLevel.Campaign ? dto.CampaignId : null,
                ChapterId = dto.Level == AchievementLevel.Chapter ? dto.ChapterId : null,
                Rarity = dto.Rarity,
                IconUrl = dto.IconUrl,
                RewardDescription = dto.RewardDescription,
                IsAutomatic = dto.IsAutomatic,
                AutomaticCondition = dto.AutomaticCondition,
                IsSecret = dto.IsSecret,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            this.dbContext.Achievements.Add(achievement);
            await this.dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            this.logger.LogInformation(
                "Successfully created achievement {AchievementId} '{Name}'",
                achievement.Id,
                achievement.Name);

            return this.MapToDto(achievement);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            this.logger.LogError(
                ex,
                "Error creating achievement '{Name}'",
                dto.Name);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AchievementDto>> GetAchievementsByWorldAsync(int worldId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving achievements for world {WorldId} by user {UserId}",
                worldId,
                userId);

            var isAuthorized = await this.IsUserAuthorizedForWorldAsync(worldId, userId);
            if (!isAuthorized)
            {
                return Enumerable.Empty<AchievementDto>();
            }

            var achievements = await this.dbContext.Achievements
                .Include(a => a.UserAchievements)
                .Where(a => a.WorldId == worldId && a.Level == AchievementLevel.World && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return achievements.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving achievements for world {WorldId}", worldId);
            return Enumerable.Empty<AchievementDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AchievementDto>> GetAchievementsByCampaignAsync(int campaignId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving achievements for campaign {CampaignId} by user {UserId}",
                campaignId,
                userId);

            var campaign = await this.dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

            if (campaign == null)
            {
                return Enumerable.Empty<AchievementDto>();
            }

            var isAuthorized = await this.IsUserAuthorizedForWorldAsync(campaign.WorldId, userId);
            if (!isAuthorized)
            {
                return Enumerable.Empty<AchievementDto>();
            }

            var achievements = await this.dbContext.Achievements
                .Include(a => a.UserAchievements)
                .Where(a => a.CampaignId == campaignId && a.Level == AchievementLevel.Campaign && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return achievements.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving achievements for campaign {CampaignId}", campaignId);
            return Enumerable.Empty<AchievementDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AchievementDto>> GetAchievementsByChapterAsync(int chapterId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving achievements for chapter {ChapterId} by user {UserId}",
                chapterId,
                userId);

            var chapter = await this.dbContext.Chapters
                .Include(ch => ch.Campaign)
                .FirstOrDefaultAsync(ch => ch.Id == chapterId && ch.IsActive);

            if (chapter == null)
            {
                return Enumerable.Empty<AchievementDto>();
            }

            var isAuthorized = await this.IsUserAuthorizedForWorldAsync(chapter.Campaign.WorldId, userId);
            if (!isAuthorized)
            {
                return Enumerable.Empty<AchievementDto>();
            }

            var achievements = await this.dbContext.Achievements
                .Include(a => a.UserAchievements)
                .Where(a => a.ChapterId == chapterId && a.Level == AchievementLevel.Chapter && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return achievements.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving achievements for chapter {ChapterId}", chapterId);
            return Enumerable.Empty<AchievementDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<AchievementDto?> GetAchievementByIdAsync(int achievementId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving achievement {AchievementId} for user {UserId}",
                achievementId,
                userId);

            var achievement = await this.dbContext.Achievements
                .Include(a => a.World)
                .Include(a => a.Campaign)
                    .ThenInclude(c => c!.World)
                .Include(a => a.Chapter)
                    .ThenInclude(ch => ch!.Campaign)
                        .ThenInclude(c => c.World)
                .Include(a => a.UserAchievements)
                .FirstOrDefaultAsync(a => a.Id == achievementId && a.IsActive);

            if (achievement == null)
            {
                return null;
            }

            // Get the world ID for authorization
            int? worldId = achievement.Level switch
            {
                AchievementLevel.World => achievement.WorldId,
                AchievementLevel.Campaign => achievement.Campaign?.WorldId,
                AchievementLevel.Chapter => achievement.Chapter?.Campaign?.WorldId,
                _ => null
            };

            if (worldId == null || !await this.IsUserAuthorizedForWorldAsync(worldId.Value, userId))
            {
                return null;
            }

            return this.MapToDto(achievement);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving achievement {AchievementId}", achievementId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<AchievementDto?> UpdateAchievementAsync(int achievementId, CreateAchievementDto request, int userId)
    {
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

        try
        {
            this.logger.LogInformation(
                "Updating achievement {AchievementId} by user {UserId}",
                achievementId,
                userId);

            var achievement = await this.dbContext.Achievements
                .Include(a => a.World)
                .Include(a => a.Campaign)
                    .ThenInclude(c => c!.World)
                .Include(a => a.Chapter)
                    .ThenInclude(ch => ch!.Campaign)
                        .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(a => a.Id == achievementId && a.IsActive);

            if (achievement == null)
            {
                return null;
            }

            // Only GM can update
            if (!await this.IsUserGmForAchievementAsync(achievement, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to update achievement {AchievementId}",
                    userId,
                    achievementId);
                return null;
            }

            achievement.Name = request.Name;
            achievement.Description = request.Description;
            achievement.Rarity = request.Rarity;
            achievement.IconUrl = request.IconUrl;
            achievement.RewardDescription = request.RewardDescription;
            achievement.IsAutomatic = request.IsAutomatic;
            achievement.AutomaticCondition = request.AutomaticCondition;
            achievement.IsSecret = request.IsSecret;
            achievement.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return this.MapToDto(achievement);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            this.logger.LogError(ex, "Error updating achievement {AchievementId}", achievementId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAchievementAsync(int achievementId, int userId)
    {
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

        try
        {
            this.logger.LogInformation(
                "Deleting achievement {AchievementId} by user {UserId}",
                achievementId,
                userId);

            var achievement = await this.dbContext.Achievements
                .Include(a => a.World)
                .Include(a => a.Campaign)
                    .ThenInclude(c => c!.World)
                .Include(a => a.Chapter)
                    .ThenInclude(ch => ch!.Campaign)
                        .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(a => a.Id == achievementId);

            if (achievement == null)
            {
                return false;
            }

            if (!await this.IsUserGmForAchievementAsync(achievement, userId))
            {
                return false;
            }

            // Soft delete
            achievement.IsActive = false;
            achievement.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            this.logger.LogError(ex, "Error deleting achievement {AchievementId}", achievementId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<UserAchievementDto?> AwardAchievementAsync(int achievementId, int targetUserId, int gmUserId, string? message = null)
    {
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

        try
        {
            this.logger.LogInformation(
                "Awarding achievement {AchievementId} to user {TargetUserId} by GM {GmUserId}",
                achievementId,
                targetUserId,
                gmUserId);

            var achievement = await this.dbContext.Achievements
                .Include(a => a.World)
                .Include(a => a.Campaign)
                    .ThenInclude(c => c!.World)
                .Include(a => a.Chapter)
                    .ThenInclude(ch => ch!.Campaign)
                        .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(a => a.Id == achievementId && a.IsActive);

            if (achievement == null)
            {
                this.logger.LogWarning("Achievement {AchievementId} not found", achievementId);
                return null;
            }

            // Only GM can award achievements
            if (!await this.IsUserGmForAchievementAsync(achievement, gmUserId))
            {
                this.logger.LogWarning(
                    "User {GmUserId} not authorized to award achievement {AchievementId}",
                    gmUserId,
                    achievementId);
                return null;
            }

            // Check if already awarded
            var existingAward = await this.dbContext.UserAchievements
                .FirstOrDefaultAsync(ua => ua.UserId == targetUserId && ua.AchievementId == achievementId);

            if (existingAward != null)
            {
                this.logger.LogWarning(
                    "User {TargetUserId} already has achievement {AchievementId}",
                    targetUserId,
                    achievementId);
                return null;
            }

            // Verify target user exists
            var targetUser = await this.dbContext.Users.FindAsync(targetUserId);
            if (targetUser == null)
            {
                this.logger.LogWarning("Target user {TargetUserId} not found", targetUserId);
                return null;
            }

            var userAchievement = new UserAchievement
            {
                UserId = targetUserId,
                AchievementId = achievementId,
                UnlockedAt = DateTime.UtcNow,
                IsManuallyAwarded = true,
                AwardedBy = gmUserId,
                AwardMessage = message
            };

            this.dbContext.UserAchievements.Add(userAchievement);
            await this.dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            this.logger.LogInformation(
                "Successfully awarded achievement {AchievementId} to user {TargetUserId}",
                achievementId,
                targetUserId);

            // Reload with includes
            userAchievement = await this.dbContext.UserAchievements
                .Include(ua => ua.User)
                .Include(ua => ua.Achievement)
                .Include(ua => ua.AwardedByUser)
                .FirstOrDefaultAsync(ua => ua.Id == userAchievement.Id);

            return userAchievement != null ? this.MapToUserAchievementDto(userAchievement) : null;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            this.logger.LogError(
                ex,
                "Error awarding achievement {AchievementId} to user {TargetUserId}",
                achievementId,
                targetUserId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UserAchievementDto>> GetUserAchievementsAsync(int userId)
    {
        try
        {
            this.logger.LogInformation("Retrieving achievements for user {UserId}", userId);

            var userAchievements = await this.dbContext.UserAchievements
                .Include(ua => ua.User)
                .Include(ua => ua.Achievement)
                .Include(ua => ua.AwardedByUser)
                .Where(ua => ua.UserId == userId && ua.Achievement.IsActive)
                .OrderByDescending(ua => ua.UnlockedAt)
                .ToListAsync();

            return userAchievements.Select(this.MapToUserAchievementDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving achievements for user {UserId}", userId);
            return Enumerable.Empty<UserAchievementDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UserAchievementDto>> GetUserAchievementsInWorldAsync(int userId, int worldId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving achievements for user {UserId} in world {WorldId}",
                userId,
                worldId);

            // Get world-level achievements
            var worldAchievementIds = await this.dbContext.Achievements
                .Where(a => a.WorldId == worldId && a.Level == AchievementLevel.World && a.IsActive)
                .Select(a => a.Id)
                .ToListAsync();

            // Get campaign-level achievements for campaigns in this world
            var campaignIds = await this.dbContext.Campaigns
                .Where(c => c.WorldId == worldId && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();

            var campaignAchievementIds = await this.dbContext.Achievements
                .Where(a => a.CampaignId.HasValue && campaignIds.Contains(a.CampaignId.Value) && a.Level == AchievementLevel.Campaign && a.IsActive)
                .Select(a => a.Id)
                .ToListAsync();

            // Get chapter-level achievements for chapters in this world's campaigns
            var chapterIds = await this.dbContext.Chapters
                .Where(ch => campaignIds.Contains(ch.CampaignId) && ch.IsActive)
                .Select(ch => ch.Id)
                .ToListAsync();

            var chapterAchievementIds = await this.dbContext.Achievements
                .Where(a => a.ChapterId.HasValue && chapterIds.Contains(a.ChapterId.Value) && a.Level == AchievementLevel.Chapter && a.IsActive)
                .Select(a => a.Id)
                .ToListAsync();

            var allAchievementIds = worldAchievementIds
                .Concat(campaignAchievementIds)
                .Concat(chapterAchievementIds)
                .Distinct()
                .ToList();

            var userAchievements = await this.dbContext.UserAchievements
                .Include(ua => ua.User)
                .Include(ua => ua.Achievement)
                .Include(ua => ua.AwardedByUser)
                .Where(ua => ua.UserId == userId && allAchievementIds.Contains(ua.AchievementId))
                .OrderByDescending(ua => ua.UnlockedAt)
                .ToListAsync();

            return userAchievements.Select(this.MapToUserAchievementDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error retrieving achievements for user {UserId} in world {WorldId}",
                userId,
                worldId);
            return Enumerable.Empty<UserAchievementDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UserAchievementDto>> GetAchievementUnlocksAsync(int achievementId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving unlocks for achievement {AchievementId}",
                achievementId);

            var achievement = await this.dbContext.Achievements
                .Include(a => a.World)
                .Include(a => a.Campaign)
                .FirstOrDefaultAsync(a => a.Id == achievementId && a.IsActive);

            if (achievement == null)
            {
                return Enumerable.Empty<UserAchievementDto>();
            }

            // Get the world ID for authorization
            int? worldId = achievement.Level switch
            {
                AchievementLevel.World => achievement.WorldId,
                AchievementLevel.Campaign => achievement.Campaign?.WorldId,
                AchievementLevel.Chapter => achievement.Chapter?.Campaign?.WorldId,
                _ => null
            };

            if (worldId == null || !await this.IsUserAuthorizedForWorldAsync(worldId.Value, userId))
            {
                return Enumerable.Empty<UserAchievementDto>();
            }

            var userAchievements = await this.dbContext.UserAchievements
                .Include(ua => ua.User)
                .Include(ua => ua.Achievement)
                .Include(ua => ua.AwardedByUser)
                .Where(ua => ua.AchievementId == achievementId)
                .OrderByDescending(ua => ua.UnlockedAt)
                .ToListAsync();

            return userAchievements.Select(this.MapToUserAchievementDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving unlocks for achievement {AchievementId}", achievementId);
            return Enumerable.Empty<UserAchievementDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RevokeAchievementAsync(int userAchievementId, int gmUserId)
    {
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

        try
        {
            this.logger.LogInformation(
                "Revoking user achievement {UserAchievementId} by GM {GmUserId}",
                userAchievementId,
                gmUserId);

            var userAchievement = await this.dbContext.UserAchievements
                .Include(ua => ua.Achievement)
                    .ThenInclude(a => a.World)
                .Include(ua => ua.Achievement)
                    .ThenInclude(a => a.Campaign)
                        .ThenInclude(c => c!.World)
                .Include(ua => ua.Achievement)
                    .ThenInclude(a => a.Chapter)
                        .ThenInclude(ch => ch!.Campaign)
                            .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(ua => ua.Id == userAchievementId);

            if (userAchievement == null)
            {
                this.logger.LogWarning("User achievement {UserAchievementId} not found", userAchievementId);
                return false;
            }

            // Only GM can revoke
            if (!await this.IsUserGmForAchievementAsync(userAchievement.Achievement, gmUserId))
            {
                this.logger.LogWarning(
                    "User {GmUserId} not authorized to revoke user achievement {UserAchievementId}",
                    gmUserId,
                    userAchievementId);
                return false;
            }

            this.dbContext.UserAchievements.Remove(userAchievement);
            await this.dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            this.logger.LogInformation(
                "Successfully revoked user achievement {UserAchievementId}",
                userAchievementId);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            this.logger.LogError(
                ex,
                "Error revoking user achievement {UserAchievementId}",
                userAchievementId);
            return false;
        }
    }

    /// <summary>
    /// Validates the achievement level and corresponding entity ID.
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage)> ValidateAchievementLevelAsync(CreateAchievementDto dto, int userId)
    {
        switch (dto.Level)
        {
            case AchievementLevel.World:
                if (!dto.WorldId.HasValue)
                {
                    return (false, "WorldId is required for world-level achievements");
                }
                var world = await this.dbContext.Worlds.FindAsync(dto.WorldId.Value);
                if (world == null || !world.IsActive)
                {
                    return (false, $"World {dto.WorldId} not found");
                }
                if (world.UserId != userId)
                {
                    return (false, $"User {userId} is not GM of world {dto.WorldId}");
                }
                break;

            case AchievementLevel.Campaign:
                if (!dto.CampaignId.HasValue)
                {
                    return (false, "CampaignId is required for campaign-level achievements");
                }
                var campaign = await this.dbContext.Campaigns
                    .Include(c => c.World)
                    .FirstOrDefaultAsync(c => c.Id == dto.CampaignId.Value && !c.IsDeleted);
                if (campaign == null)
                {
                    return (false, $"Campaign {dto.CampaignId} not found");
                }
                if (campaign.World?.UserId != userId && campaign.CreatedBy != userId)
                {
                    return (false, $"User {userId} is not GM of campaign {dto.CampaignId}");
                }
                break;

            case AchievementLevel.Chapter:
                if (!dto.ChapterId.HasValue)
                {
                    return (false, "ChapterId is required for chapter-level achievements");
                }
                var chapter = await this.dbContext.Chapters
                    .Include(ch => ch.Campaign)
                        .ThenInclude(c => c.World)
                    .FirstOrDefaultAsync(ch => ch.Id == dto.ChapterId.Value && ch.IsActive);
                if (chapter == null)
                {
                    return (false, $"Chapter {dto.ChapterId} not found");
                }
                var chCampaign = chapter.Campaign;
                if (chCampaign.World?.UserId != userId && chCampaign.CreatedBy != userId)
                {
                    return (false, $"User {userId} is not GM of chapter {dto.ChapterId}");
                }
                break;
        }

        return (true, null);
    }

    /// <summary>
    /// Checks if the user is authorized to access a world.
    /// </summary>
    private async Task<bool> IsUserAuthorizedForWorldAsync(int worldId, int userId)
    {
        var world = await this.dbContext.Worlds.FindAsync(worldId);
        if (world == null || !world.IsActive)
        {
            return false;
        }

        // GM has access
        if (world.UserId == userId)
        {
            return true;
        }

        // Player with character in world has access
        return await this.dbContext.WorldCharacters
            .AnyAsync(wc => wc.WorldId == worldId && wc.Character.UserId == userId && wc.IsActive);
    }

    /// <summary>
    /// Checks if the user is the GM for the achievement's context.
    /// </summary>
    private async Task<bool> IsUserGmForAchievementAsync(Achievement achievement, int userId)
    {
        return achievement.Level switch
        {
            AchievementLevel.World => achievement.World?.UserId == userId,
            AchievementLevel.Campaign => achievement.Campaign?.World?.UserId == userId || achievement.Campaign?.CreatedBy == userId,
            AchievementLevel.Chapter => achievement.Chapter?.Campaign?.World?.UserId == userId || achievement.Chapter?.Campaign?.CreatedBy == userId,
            _ => false
        };
    }

    /// <summary>
    /// Maps an Achievement entity to an AchievementDto.
    /// </summary>
    private AchievementDto MapToDto(Achievement achievement)
    {
        return new AchievementDto
        {
            Id = achievement.Id,
            Name = achievement.Name,
            Description = achievement.Description,
            Level = achievement.Level,
            WorldId = achievement.WorldId,
            CampaignId = achievement.CampaignId,
            ChapterId = achievement.ChapterId,
            Rarity = achievement.Rarity,
            IconUrl = achievement.IconUrl,
            RewardDescription = achievement.RewardDescription,
            IsAutomatic = achievement.IsAutomatic,
            IsSecret = achievement.IsSecret,
            IsActive = achievement.IsActive,
            CreatedAt = achievement.CreatedAt,
            UpdatedAt = achievement.UpdatedAt,
            CreatedBy = achievement.CreatedBy,
            UnlockCount = achievement.UserAchievements?.Count ?? 0
        };
    }

    /// <summary>
    /// Maps a UserAchievement entity to a UserAchievementDto.
    /// </summary>
    private UserAchievementDto MapToUserAchievementDto(UserAchievement ua)
    {
        return new UserAchievementDto
        {
            Id = ua.Id,
            UserId = ua.UserId,
            UserName = ua.User?.Nickname ?? string.Empty,
            AchievementId = ua.AchievementId,
            AchievementName = ua.Achievement?.Name ?? string.Empty,
            AchievementDescription = ua.Achievement?.Description ?? string.Empty,
            Rarity = ua.Achievement?.Rarity ?? AchievementRarity.Common,
            IconUrl = ua.Achievement?.IconUrl,
            UnlockedAt = ua.UnlockedAt,
            IsManuallyAwarded = ua.IsManuallyAwarded,
            AwardedBy = ua.AwardedBy,
            AwardedByName = ua.AwardedByUser?.Nickname,
            AwardMessage = ua.AwardMessage
        };
    }
}
