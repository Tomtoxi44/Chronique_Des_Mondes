// -----------------------------------------------------------------------
// <copyright file="IWorldService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing worlds.
/// </summary>
public interface IWorldService
{
    /// <summary>
    /// Creates a new world.
    /// </summary>
    /// <param name="request">The world creation request.</param>
    /// <param name="userId">The user identifier of the world creator (becomes Game Master).</param>
    /// <returns>The created world.</returns>
    Task<WorldDto?> CreateWorldAsync(CreateWorldDto request, int userId);

    /// <summary>
    /// Gets all worlds created by a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of worlds created by the user.</returns>
    Task<IEnumerable<WorldDto>> GetMyWorldsAsync(int userId);

    /// <summary>
    /// Gets a world by its identifier.
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="userId">The user identifier requesting the world.</param>
    /// <returns>The world if found and authorized, null otherwise.</returns>
    Task<WorldDto?> GetWorldByIdAsync(int worldId, int userId);

    /// <summary>
    /// Gets all worlds a user participates in (as GM or player).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of worlds the user participates in.</returns>
    Task<IEnumerable<WorldDto>> GetWorldsForUserAsync(int userId);

    /// <summary>
    /// Updates a world.
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="userId">The user identifier requesting the update.</param>
    /// <returns>The updated world, or null if not found/unauthorized.</returns>
    Task<WorldDto?> UpdateWorldAsync(int worldId, CreateWorldDto request, int userId);

    /// <summary>
    /// Deletes a world.
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="userId">The user identifier requesting the deletion.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteWorldAsync(int worldId, int userId);

    /// <summary>
    /// Gets all characters in a world.
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="userId">The user identifier requesting the data.</param>
    /// <returns>A list of world characters.</returns>
    Task<IEnumerable<WorldCharacterDto>> GetWorldCharactersAsync(int worldId, int userId);

    /// <summary>
    /// Removes a character from a world (soft delete). Only the world owner (GM) or character owner can do this.
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="userId">The user identifier requesting the removal.</param>
    /// <returns>True if removed successfully, false otherwise.</returns>
    Task<bool> RemoveCharacterFromWorldAsync(int worldId, int characterId, int userId);

    /// <summary>
    /// Generates (or refreshes) an invite token for a world. Only the GM can do this.
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="userId">The GM user identifier.</param>
    /// <returns>The invite token string, or null if unauthorized.</returns>
    Task<string?> GenerateWorldInviteTokenAsync(int worldId, int userId);

    /// <summary>
    /// Gets a world by its invite token (to display info before joining).
    /// </summary>
    /// <param name="inviteToken">The invite token.</param>
    /// <returns>The world DTO, or null if token is invalid/expired.</returns>
    Task<WorldDto?> GetWorldByInviteTokenAsync(string inviteToken);

    /// <summary>
    /// Joins a world with a character using an invite token.
    /// Locks the character and creates a WorldCharacter entry.
    /// </summary>
    /// <param name="inviteToken">The invite token.</param>
    /// <param name="characterId">The character to bring into the world.</param>
    /// <param name="userId">The player user identifier.</param>
    /// <returns>The world character DTO, or null if failed.</returns>
    Task<WorldCharacterDto?> JoinWorldAsync(string inviteToken, int characterId, int userId);

    /// <summary>
    /// Gets the current user's world character for a specific world.
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="userId">The player user identifier.</param>
    /// <returns>The world character DTO, or null if not found.</returns>
    Task<WorldCharacterDto?> GetMyWorldCharacterAsync(int worldId, int userId);

    /// <summary>
    /// Updates the current user's world character profile (game-specific stats).
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="dto">The update data.</param>
    /// <param name="userId">The player user identifier.</param>
    /// <returns>The updated world character DTO, or null if not found/unauthorized.</returns>
    Task<WorldCharacterDto?> UpdateMyWorldCharacterAsync(int worldId, UpdateWorldCharacterProfileDto dto, int userId);

    /// <summary>
    /// Gets campaigns in a world accessible to a member (GM or player).
    /// Players see campaign title + description only (no chapter content).
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="userId">The user identifier (must be GM or world member).</param>
    /// <returns>List of campaigns visible to the member.</returns>
    Task<IEnumerable<CampaignDto>> GetWorldCampaignsForMemberAsync(int worldId, int userId);

    /// <summary>
    /// Gets a world character by its ID (must belong to the requesting user or the user must be the world GM).
    /// </summary>
    /// <param name="wcId">The world character identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The world character DTO, or null if not found or unauthorized.</returns>
    Task<WorldCharacterDto?> GetWorldCharacterByIdAsync(int wcId, int userId);
}
