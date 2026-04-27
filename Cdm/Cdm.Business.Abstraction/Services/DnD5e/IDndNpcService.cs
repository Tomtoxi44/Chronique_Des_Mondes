// -----------------------------------------------------------------------
// <copyright file="IDndNpcService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services.DnD5e;

using Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>NPC service extended for D&amp;D 5e stats.</summary>
public interface IDndNpcService
{
    Task<IEnumerable<DndNpcDto>> GetDndNpcsAsync(int chapterId);
    Task<DndNpcDto?> GetDndNpcByIdAsync(int npcId);
    Task<DndNpcDto> CreateDndNpcAsync(int chapterId, CreateDndNpcDto dto, int requestingUserId);
    Task<DndNpcDto?> UpdateDndNpcAsync(int npcId, CreateDndNpcDto dto, int requestingUserId);
    Task<bool> DeleteDndNpcAsync(int npcId, int requestingUserId);
}
