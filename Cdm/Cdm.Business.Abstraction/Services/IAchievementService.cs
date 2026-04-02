// -----------------------------------------------------------------------
// <copyright file="IAchievementService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing achievements and their attribution to users.
/// </summary>
public interface IAchievementService
{
    /// <summary>
    /// Creates a new achievement.
    /// </summary>
    /// <param name="request">The achievement creation request.</param>
    /// <param name="userId">The user identifier of the achievement creator (must be GM).</param>
    /// <returns>The created achievement.</returns>
    Task<AchievementDto?> CreateAchievementAsync(CreateAchievementDto request, int userId);

    /// <summary>
    /// Gets all achievements for a world.
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="userId">The user identifier requesting the achievements.</param>
    /// <returns>A list of world-level achievements.</returns>
    Task<IEnumerable<AchievementDto>> GetAchievementsByWorldAsync(int worldId, int userId);

    /// <summary>
    /// Gets all achievements for a campaign.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <param name="userId">The user identifier requesting the achievements.</param>
    /// <returns>A list of campaign-level achievements.</returns>
    Task<IEnumerable<AchievementDto>> GetAchievementsByCampaignAsync(int campaignId, int userId);

    /// <summary>
    /// Gets all achievements for a chapter.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="userId">The user identifier requesting the achievements.</param>
    /// <returns>A list of chapter-level achievements.</returns>
    Task<IEnumerable<AchievementDto>> GetAchievementsByChapterAsync(int chapterId, int userId);

    /// <summary>
    /// Gets an achievement by its identifier.
    /// </summary>
    /// <param name="achievementId">The achievement identifier.</param>
    /// <param name="userId">The user identifier requesting the achievement.</param>
    /// <returns>The achievement if found and authorized, null otherwise.</returns>
    Task<AchievementDto?> GetAchievementByIdAsync(int achievementId, int userId);

    /// <summary>
    /// Updates an achievement.
    /// </summary>
    /// <param name="achievementId">The achievement identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="userId">The user identifier requesting the update (must be GM).</param>
    /// <returns>The updated achievement, or null if not found/unauthorized.</returns>
    Task<AchievementDto?> UpdateAchievementAsync(int achievementId, CreateAchievementDto request, int userId);

    /// <summary>
    /// Deletes an achievement.
    /// </summary>
    /// <param name="achievementId">The achievement identifier.</param>
    /// <param name="userId">The user identifier requesting the deletion (must be GM).</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteAchievementAsync(int achievementId, int userId);

    /// <summary>
    /// Manually awards an achievement to a user.
    /// </summary>
    /// <param name="achievementId">The achievement identifier.</param>
    /// <param name="targetUserId">The user to award the achievement to.</param>
    /// <param name="gmUserId">The Game Master awarding the achievement.</param>
    /// <param name="message">Optional message from the GM.</param>
    /// <returns>The user achievement record, or null if failed.</returns>
    Task<UserAchievementDto?> AwardAchievementAsync(int achievementId, int targetUserId, int gmUserId, string? message = null);

    /// <summary>
    /// Gets all achievements unlocked by a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of user achievements.</returns>
    Task<IEnumerable<UserAchievementDto>> GetUserAchievementsAsync(int userId);

    /// <summary>
    /// Gets all achievements unlocked by a user in a specific world.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="worldId">The world identifier.</param>
    /// <returns>A list of user achievements in the world.</returns>
    Task<IEnumerable<UserAchievementDto>> GetUserAchievementsInWorldAsync(int userId, int worldId);

    /// <summary>
    /// Gets all users who unlocked a specific achievement.
    /// </summary>
    /// <param name="achievementId">The achievement identifier.</param>
    /// <param name="userId">The user identifier requesting the data.</param>
    /// <returns>A list of user achievements.</returns>
    Task<IEnumerable<UserAchievementDto>> GetAchievementUnlocksAsync(int achievementId, int userId);

    /// <summary>
    /// Revokes an achievement from a user.
    /// </summary>
    /// <param name="userAchievementId">The user achievement identifier.</param>
    /// <param name="gmUserId">The Game Master revoking the achievement.</param>
    /// <returns>True if revoked, false otherwise.</returns>
    Task<bool> RevokeAchievementAsync(int userAchievementId, int gmUserId);
}
