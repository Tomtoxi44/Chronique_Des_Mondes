// -----------------------------------------------------------------------
// <copyright file="MarketplaceService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Marketplace sharing/browsing/importing for worlds, campaigns and base characters.
/// Imports always create independent copies owned by the importing user.
/// </summary>
public class MarketplaceService(AppDbContext dbContext, ILogger<MarketplaceService> logger) : IMarketplaceService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<MarketplaceService> logger = logger;

    // ── Sharing toggles ──────────────────────────────────────────────────

    public async Task<bool> SetWorldSharedAsync(int worldId, int userId, bool isShared)
    {
        var world = await this.dbContext.Worlds.FirstOrDefaultAsync(w => w.Id == worldId && w.UserId == userId && w.IsActive);
        if (world == null)
        {
            return false;
        }

        world.IsShared = isShared;
        world.UpdatedAt = DateTime.UtcNow;
        await this.dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetCampaignSharedAsync(int campaignId, int userId, bool isShared)
    {
        var campaign = await this.dbContext.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId && c.CreatedBy == userId && c.IsActive && !c.IsDeleted);
        if (campaign == null)
        {
            return false;
        }

        campaign.IsShared = isShared;
        campaign.UpdatedAt = DateTime.UtcNow;
        await this.dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetCharacterSharedAsync(int characterId, int userId, bool isShared)
    {
        var character = await this.dbContext.Characters.FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId && c.IsActive && c.IsBaseCharacter);
        if (character == null)
        {
            return false;
        }

        character.IsShared = isShared;
        character.UpdatedAt = DateTime.UtcNow;
        await this.dbContext.SaveChangesAsync();
        return true;
    }

    // ── Browse ───────────────────────────────────────────────────────────

    public async Task<IEnumerable<MarketplaceEntryDto>> GetSharedWorldsAsync(GameType? gameType = null, string? search = null)
    {
        try
        {
            var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            var query = from w in this.dbContext.Worlds
                        join u in this.dbContext.Users on w.UserId equals u.Id
                        where w.IsActive && w.IsShared
                            && (gameType == null || w.GameType == gameType)
                            && (term == null || w.Name.Contains(term))
                        orderby w.CreatedAt descending
                        select new MarketplaceEntryDto
                        {
                            Id = w.Id,
                            Name = w.Name,
                            Description = w.Description,
                            GameType = w.GameType,
                            SharedByName = u.Nickname,
                        };
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error listing shared worlds");
            return Enumerable.Empty<MarketplaceEntryDto>();
        }
    }

    public async Task<IEnumerable<MarketplaceEntryDto>> GetSharedCampaignsAsync(GameType? gameType = null, string? search = null)
    {
        try
        {
            var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            var query = from c in this.dbContext.Campaigns
                        join wld in this.dbContext.Worlds on c.WorldId equals wld.Id
                        join u in this.dbContext.Users on c.CreatedBy equals u.Id
                        where c.IsActive && !c.IsDeleted && c.IsShared
                            && (gameType == null || wld.GameType == gameType)
                            && (term == null || c.Name.Contains(term))
                        orderby c.CreatedAt descending
                        select new MarketplaceEntryDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description,
                            ImageUrl = c.CoverImageUrl,
                            GameType = wld.GameType,
                            SharedByName = u.Nickname,
                        };
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error listing shared campaigns");
            return Enumerable.Empty<MarketplaceEntryDto>();
        }
    }

    public async Task<IEnumerable<MarketplaceEntryDto>> GetSharedCharactersAsync(string? search = null)
    {
        try
        {
            var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            var query = from ch in this.dbContext.Characters
                        join u in this.dbContext.Users on ch.UserId equals u.Id
                        where ch.IsActive && ch.IsShared && ch.IsBaseCharacter
                            && (term == null || ch.Name.Contains(term) || (ch.FirstName != null && ch.FirstName.Contains(term)))
                        orderby ch.CreatedAt descending
                        select new MarketplaceEntryDto
                        {
                            Id = ch.Id,
                            Name = ch.FirstName ?? ch.Name,
                            Description = ch.Description,
                            ImageUrl = ch.AvatarUrl,
                            GameType = GameType.Generic,
                            SharedByName = u.Nickname,
                        };
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error listing shared characters");
            return Enumerable.Empty<MarketplaceEntryDto>();
        }
    }

    // ── Import (independent copies) ──────────────────────────────────────

    public async Task<(bool Success, string? Error)> ImportWorldAsync(int worldId, int userId)
    {
        try
        {
            var src = await this.dbContext.Worlds.FirstOrDefaultAsync(w => w.Id == worldId && w.IsActive && w.IsShared);
            if (src == null)
            {
                return (false, "Monde introuvable ou non partagé.");
            }

            var copy = new World
            {
                UserId = userId,
                Name = src.Name,
                Description = src.Description,
                GameType = src.GameType,
                IsActive = true,
                IsShared = false,
                CreatedAt = DateTime.UtcNow,
            };
            this.dbContext.Worlds.Add(copy);
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("User {UserId} imported world {WorldId} as {NewId}", userId, worldId, copy.Id);
            return (true, null);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error importing world {WorldId}", worldId);
            return (false, "Erreur lors de l'import du monde.");
        }
    }

    public async Task<(bool Success, string? Error)> ImportCampaignAsync(int campaignId, int targetWorldId, int userId)
    {
        try
        {
            var src = await this.dbContext.Campaigns
                .Include(c => c.World)
                .FirstOrDefaultAsync(c => c.Id == campaignId && c.IsActive && !c.IsDeleted && c.IsShared);
            if (src == null)
            {
                return (false, "Campagne introuvable ou non partagée.");
            }

            var targetWorld = await this.dbContext.Worlds
                .FirstOrDefaultAsync(w => w.Id == targetWorldId && w.IsActive && w.UserId == userId);
            if (targetWorld == null)
            {
                return (false, "Monde cible introuvable ou ne vous appartient pas.");
            }

            if (targetWorld.GameType != src.World.GameType)
            {
                return (false, "Le type de jeu du monde cible ne correspond pas à celui de la campagne.");
            }

            var copy = new Campaign
            {
                Name = src.Name,
                Description = src.Description,
                WorldId = targetWorldId,
                CreatedBy = userId,
                Visibility = Visibility.Private,
                MaxPlayers = src.MaxPlayers,
                CoverImageUrl = src.CoverImageUrl,
                IsActive = true,
                IsShared = false,
                CreatedAt = DateTime.UtcNow,
            };
            this.dbContext.Campaigns.Add(copy);
            await this.dbContext.SaveChangesAsync();

            // Deep-copy the chapters (content of the campaign).
            var chapters = await this.dbContext.Chapters
                .Where(ch => ch.CampaignId == src.Id && ch.IsActive)
                .OrderBy(ch => ch.ChapterNumber)
                .ToListAsync();

            foreach (var ch in chapters)
            {
                this.dbContext.Chapters.Add(new Chapter
                {
                    CampaignId = copy.Id,
                    ChapterNumber = ch.ChapterNumber,
                    Title = ch.Title,
                    Content = ch.Content,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            if (chapters.Count > 0)
            {
                await this.dbContext.SaveChangesAsync();
            }

            this.logger.LogInformation("User {UserId} imported campaign {CampaignId} into world {WorldId} ({ChapterCount} chapters)", userId, campaignId, targetWorldId, chapters.Count);
            return (true, null);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error importing campaign {CampaignId}", campaignId);
            return (false, "Erreur lors de l'import de la campagne.");
        }
    }

    public async Task<(bool Success, string? Error)> ImportCharacterAsync(int characterId, int userId)
    {
        try
        {
            var src = await this.dbContext.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.IsActive && c.IsShared && c.IsBaseCharacter);
            if (src == null)
            {
                return (false, "Personnage introuvable ou non partagé.");
            }

            var copy = new Character
            {
                UserId = userId,
                Name = src.Name,
                FirstName = src.FirstName,
                Description = src.Description,
                Age = src.Age,
                AvatarUrl = src.AvatarUrl,
                IsActive = true,
                IsBaseCharacter = true,
                IsShared = false,
                CreatedAt = DateTime.UtcNow,
            };
            this.dbContext.Characters.Add(copy);
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("User {UserId} imported character {CharacterId} as {NewId}", userId, characterId, copy.Id);
            return (true, null);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error importing character {CharacterId}", characterId);
            return (false, "Erreur lors de l'import du personnage.");
        }
    }
}
