// -----------------------------------------------------------------------
// <copyright file="ICodexService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;

/// <summary>
/// Manages a user's personal codex of item templates.
/// </summary>
public interface ICodexService
{
    /// <summary>Creates a codex item owned by the user.</summary>
    Task<CodexItemDto?> CreateAsync(CreateCodexItemDto dto, int userId);

    /// <summary>Lists the user's active codex items, optionally filtered by game type.</summary>
    Task<IEnumerable<CodexItemDto>> GetMyItemsAsync(int userId, GameType? gameType = null);

    /// <summary>Gets one of the user's codex items by id.</summary>
    Task<CodexItemDto?> GetByIdAsync(int id, int userId);

    /// <summary>Updates one of the user's codex items.</summary>
    Task<CodexItemDto?> UpdateAsync(int id, CreateCodexItemDto dto, int userId);

    /// <summary>Soft-deletes one of the user's codex items.</summary>
    Task<bool> DeleteAsync(int id, int userId);

    /// <summary>
    /// Copies a codex item into a world character's inventory (independent copy).
    /// The character must belong to the user and its world's game type must match the item's.
    /// </summary>
    /// <returns>Success flag and, on failure, a user-facing error message.</returns>
    Task<(bool Success, string? Error)> AddToCharacterInventoryAsync(int codexItemId, int worldCharacterId, int userId);

    /// <summary>Publishes or unpublishes one of the user's codex items to the marketplace.</summary>
    Task<bool> SetSharedAsync(int id, int userId, bool isShared);

    /// <summary>Lists items shared on the marketplace by any user, optionally filtered by type and search term.</summary>
    Task<IEnumerable<CodexItemDto>> GetMarketplaceItemsAsync(GameType? gameType = null, string? search = null);

    /// <summary>Imports a shared marketplace item as an independent copy into the user's codex.</summary>
    Task<CodexItemDto?> ImportToMyCodexAsync(int sourceItemId, int userId);
}
