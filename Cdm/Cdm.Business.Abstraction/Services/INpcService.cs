// -----------------------------------------------------------------------
// <copyright file="INpcService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing Non-Player Characters (NPCs) within chapters.
/// </summary>
public interface INpcService
{
    /// <summary>
    /// Creates a new NPC in a chapter. Only the Game Master can create NPCs.
    /// </summary>
    /// <param name="request">The NPC creation request.</param>
    /// <param name="userId">The user identifier of the requestor (must be GM).</param>
    /// <returns>The created NPC, or null if unauthorized or chapter not found.</returns>
    Task<NpcDto?> CreateNpcAsync(CreateNpcDto request, int userId);

    /// <summary>
    /// Gets all active NPCs for a given chapter.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="userId">The user identifier (must be GM).</param>
    /// <returns>A list of NPCs for the chapter.</returns>
    Task<IEnumerable<NpcDto>> GetNpcsByChapterAsync(int chapterId, int userId);

    /// <summary>
    /// Gets a specific NPC by its identifier.
    /// </summary>
    /// <param name="npcId">The NPC identifier.</param>
    /// <param name="userId">The user identifier (must be GM).</param>
    /// <returns>The NPC if found and authorized, null otherwise.</returns>
    Task<NpcDto?> GetNpcByIdAsync(int npcId, int userId);

    /// <summary>
    /// Updates an existing NPC.
    /// </summary>
    /// <param name="npcId">The NPC identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="userId">The user identifier (must be GM).</param>
    /// <returns>The updated NPC, or null if not found or unauthorized.</returns>
    Task<NpcDto?> UpdateNpcAsync(int npcId, CreateNpcDto request, int userId);

    /// <summary>
    /// Soft-deletes an NPC.
    /// </summary>
    /// <param name="npcId">The NPC identifier.</param>
    /// <param name="userId">The user identifier (must be GM).</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteNpcAsync(int npcId, int userId);
}
