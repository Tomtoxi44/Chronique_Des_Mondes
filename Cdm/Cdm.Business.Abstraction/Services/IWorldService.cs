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
}
