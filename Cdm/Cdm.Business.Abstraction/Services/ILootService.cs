// -----------------------------------------------------------------------
// <copyright file="ILootService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Manages campaign loot: the GM prepares items on a campaign/chapter and hands them
/// out to players' characters during a session (copied into their inventory).
/// </summary>
public interface ILootService
{
    /// <summary>Creates a loot entry on a campaign (GM only). Optionally seeded from a codex item.</summary>
    Task<CampaignLootDto?> CreateAsync(int campaignId, CreateLootDto dto, int userId);

    /// <summary>Lists the active loot of a campaign (GM only).</summary>
    Task<IEnumerable<CampaignLootDto>> GetCampaignLootAsync(int campaignId, int userId);

    /// <summary>Updates a loot entry (GM only).</summary>
    Task<CampaignLootDto?> UpdateAsync(int lootId, CreateLootDto dto, int userId);

    /// <summary>Soft-deletes a loot entry (GM only).</summary>
    Task<bool> DeleteAsync(int lootId, int userId);

    /// <summary>
    /// Hands a loot item to a player's character: copies it into their inventory and records
    /// the distribution. Only the campaign's GM may distribute. Returns a payload suitable for
    /// broadcasting to the session.
    /// </summary>
    Task<(bool Success, string? Error, LootDistributionResultDto? Result)> DistributeAsync(int lootId, int worldCharacterId, int? sessionId, int userId);
}
