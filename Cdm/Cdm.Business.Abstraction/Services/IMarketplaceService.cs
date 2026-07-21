// -----------------------------------------------------------------------
// <copyright file="IMarketplaceService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;

/// <summary>
/// Marketplace: sharing, browsing and importing worlds, campaigns and base characters.
/// (Codex items are handled by <see cref="ICodexService"/>.)
/// </summary>
public interface IMarketplaceService
{
    Task<bool> SetWorldSharedAsync(int worldId, int userId, bool isShared);

    Task<bool> SetCampaignSharedAsync(int campaignId, int userId, bool isShared);

    Task<bool> SetCharacterSharedAsync(int characterId, int userId, bool isShared);

    Task<IEnumerable<MarketplaceEntryDto>> GetSharedWorldsAsync(GameType? gameType = null, string? search = null);

    Task<IEnumerable<MarketplaceEntryDto>> GetSharedCampaignsAsync(GameType? gameType = null, string? search = null);

    Task<IEnumerable<MarketplaceEntryDto>> GetSharedCharactersAsync(string? search = null);

    /// <summary>Imports a shared world as a new world owned by the user.</summary>
    Task<(bool Success, string? Error)> ImportWorldAsync(int worldId, int userId);

    /// <summary>Imports a shared campaign (and its chapters) into one of the user's worlds of the same game type.</summary>
    Task<(bool Success, string? Error)> ImportCampaignAsync(int campaignId, int targetWorldId, int userId);

    /// <summary>Imports a shared base character as a new base character owned by the user.</summary>
    Task<(bool Success, string? Error)> ImportCharacterAsync(int characterId, int userId);
}
