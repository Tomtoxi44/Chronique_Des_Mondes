// -----------------------------------------------------------------------
// <copyright file="IDndReferenceService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services.DnD5e;

using Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>Read-only access to D&amp;D 5e reference data (races, classes, items, spells, monsters).</summary>
public interface IDndReferenceService
{
    Task<IEnumerable<DndRaceDto>> GetRacesAsync();
    Task<DndRaceDto?> GetRaceByIdAsync(int id);

    Task<IEnumerable<DndClassDto>> GetClassesAsync();
    Task<DndClassDto?> GetClassByIdAsync(int id);

    Task<IEnumerable<DndItemDto>> GetItemsAsync(string? category = null);
    Task<DndItemDto?> GetItemByIdAsync(int id);

    Task<IEnumerable<DndSpellDto>> GetSpellsAsync(int? level = null, string? characterClass = null);
    Task<DndSpellDto?> GetSpellByIdAsync(int id);

    Task<IEnumerable<DndMonsterTemplateDto>> GetMonsterTemplatesAsync();
    Task<DndMonsterTemplateDto?> GetMonsterTemplateByIdAsync(int id);
}
