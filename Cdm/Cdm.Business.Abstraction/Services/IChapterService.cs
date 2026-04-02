// -----------------------------------------------------------------------
// <copyright file="IChapterService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing chapters within campaigns.
/// </summary>
public interface IChapterService
{
    /// <summary>
    /// Creates a new chapter in a campaign.
    /// </summary>
    /// <param name="request">The chapter creation request.</param>
    /// <param name="userId">The user identifier of the chapter creator (must be GM).</param>
    /// <returns>The created chapter.</returns>
    Task<ChapterDto?> CreateChapterAsync(CreateChapterDto request, int userId);

    /// <summary>
    /// Gets all chapters in a campaign.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <param name="userId">The user identifier requesting the chapters.</param>
    /// <returns>A list of chapters in the campaign.</returns>
    Task<IEnumerable<ChapterDto>> GetChaptersByCampaignAsync(int campaignId, int userId);

    /// <summary>
    /// Gets a chapter by its identifier.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="userId">The user identifier requesting the chapter.</param>
    /// <returns>The chapter if found and authorized, null otherwise.</returns>
    Task<ChapterDto?> GetChapterByIdAsync(int chapterId, int userId);

    /// <summary>
    /// Updates a chapter.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="userId">The user identifier requesting the update (must be GM).</param>
    /// <returns>The updated chapter, or null if not found/unauthorized.</returns>
    Task<ChapterDto?> UpdateChapterAsync(int chapterId, CreateChapterDto request, int userId);

    /// <summary>
    /// Deletes a chapter.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="userId">The user identifier requesting the deletion (must be GM).</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteChapterAsync(int chapterId, int userId);

    /// <summary>
    /// Starts a chapter session (sets status to InProgress).
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="userId">The user identifier (must be GM).</param>
    /// <returns>The updated chapter, or null if not found/unauthorized.</returns>
    Task<ChapterDto?> StartChapterAsync(int chapterId, int userId);

    /// <summary>
    /// Completes a chapter session (sets status to Completed).
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="userId">The user identifier (must be GM).</param>
    /// <returns>The updated chapter, or null if not found/unauthorized.</returns>
    Task<ChapterDto?> CompleteChapterAsync(int chapterId, int userId);
}
