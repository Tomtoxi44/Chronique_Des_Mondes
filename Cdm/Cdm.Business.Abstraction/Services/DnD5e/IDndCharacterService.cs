// -----------------------------------------------------------------------
// <copyright file="IDndCharacterService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services.DnD5e;

using Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>D&amp;D 5e character service: stats, inventory, spells for world characters.</summary>
public interface IDndCharacterService
{
    /// <summary>Apply D&amp;D stats (level, class, race, abilities) to a world character.</summary>
    Task<bool> ApplyDndStatsAsync(int worldCharacterId, DndCharacterStatsDto stats, int requestingUserId);

    /// <summary>Get the D&amp;D stats for a world character.</summary>
    Task<DndCharacterStatsDto?> GetDndStatsAsync(int worldCharacterId, int requestingUserId);

    Task<IEnumerable<DndInventoryItemDto>> GetInventoryAsync(int worldCharacterId, int requestingUserId);
    Task<DndInventoryItemDto> AddInventoryItemAsync(int worldCharacterId, DndInventoryItemDto item, int requestingUserId);
    Task<bool> RemoveInventoryItemAsync(int worldCharacterId, int itemId, int requestingUserId);

    Task<IEnumerable<DndCharacterSpellDto>> GetSpellsAsync(int worldCharacterId, int requestingUserId);
    Task<DndCharacterSpellDto> AddSpellAsync(int worldCharacterId, DndCharacterSpellDto spell, int requestingUserId);
    Task<bool> RemoveSpellAsync(int worldCharacterId, int spellId, int requestingUserId);
}
