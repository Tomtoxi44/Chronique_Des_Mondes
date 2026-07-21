// -----------------------------------------------------------------------
// <copyright file="IInventoryService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Universal (all game systems) inventory management for a world character.
/// The character's owner manages their own inventory; items carry a GameType so a system's
/// rules apply only to items of that system ("specifying" = changing an item's GameType/stats).
/// </summary>
public interface IInventoryService
{
    /// <summary>Lists the inventory of a world character owned by the user.</summary>
    Task<IEnumerable<InventoryItemDto>> GetForCharacterAsync(int worldCharacterId, int userId);

    /// <summary>Adds an item to a world character owned by the user.</summary>
    Task<InventoryItemDto?> AddAsync(int worldCharacterId, CreateInventoryItemDto dto, int userId);

    /// <summary>Updates an inventory item (including "specifying" it to a game system).</summary>
    Task<InventoryItemDto?> UpdateAsync(int itemId, CreateInventoryItemDto dto, int userId);

    /// <summary>Deletes an inventory item owned by the user.</summary>
    Task<bool> DeleteAsync(int itemId, int userId);
}
