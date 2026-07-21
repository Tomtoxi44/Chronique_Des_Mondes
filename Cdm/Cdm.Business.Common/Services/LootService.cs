// -----------------------------------------------------------------------
// <copyright file="LootService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Campaign loot management and in-session distribution. The loot's game type always
/// follows the campaign's world, so distribution to any character of that world is compatible.
/// </summary>
public class LootService(AppDbContext dbContext, ILogger<LootService> logger) : ILootService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<LootService> logger = logger;

    /// <inheritdoc/>
    public async Task<CampaignLootDto?> CreateAsync(int campaignId, CreateLootDto dto, int userId)
    {
        try
        {
            var campaign = await this.dbContext.Campaigns
                .Include(c => c.World)
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);
            if (campaign == null || campaign.CreatedBy != userId)
            {
                return null;
            }

            if (dto.ChapterId.HasValue)
            {
                var chapterOk = await this.dbContext.Chapters
                    .AnyAsync(ch => ch.Id == dto.ChapterId.Value && ch.CampaignId == campaignId);
                if (!chapterOk)
                {
                    return null;
                }
            }

            var loot = new CampaignLoot
            {
                CampaignId = campaignId,
                ChapterId = dto.ChapterId,
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                ItemType = dto.ItemType,
                GameSpecificData = dto.GameSpecificData,
                Quantity = dto.Quantity < 1 ? 1 : dto.Quantity,
                GameType = campaign.World.GameType,
                CreatedBy = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            // Seed from a codex item owned by the GM (explicit dto fields still win when provided).
            if (dto.SourceCodexItemId.HasValue)
            {
                var source = await this.dbContext.CodexItems
                    .FirstOrDefaultAsync(c => c.Id == dto.SourceCodexItemId.Value && c.UserId == userId && c.IsActive);
                if (source != null)
                {
                    if (string.IsNullOrWhiteSpace(loot.Name))
                    {
                        loot.Name = source.Name;
                    }

                    loot.Description ??= source.Description;
                    loot.ImageUrl ??= source.ImageUrl;
                    loot.ItemType ??= source.ItemType;
                    loot.GameSpecificData ??= source.GameSpecificData;
                }
            }

            if (string.IsNullOrWhiteSpace(loot.Name))
            {
                return null;
            }

            this.dbContext.CampaignLoots.Add(loot);
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Created loot {LootId} for campaign {CampaignId}", loot.Id, campaignId);
            return MapToDto(loot);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating loot for campaign {CampaignId}", campaignId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CampaignLootDto>> GetCampaignLootAsync(int campaignId, int userId)
    {
        try
        {
            var isGm = await this.dbContext.Campaigns
                .AnyAsync(c => c.Id == campaignId && !c.IsDeleted && c.CreatedBy == userId);
            if (!isGm)
            {
                return Enumerable.Empty<CampaignLootDto>();
            }

            var loot = await this.dbContext.CampaignLoots
                .Where(l => l.CampaignId == campaignId && l.IsActive)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return loot.Select(MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error listing loot for campaign {CampaignId}", campaignId);
            return Enumerable.Empty<CampaignLootDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<CampaignLootDto?> UpdateAsync(int lootId, CreateLootDto dto, int userId)
    {
        try
        {
            var loot = await this.dbContext.CampaignLoots
                .Include(l => l.Campaign)
                .FirstOrDefaultAsync(l => l.Id == lootId && l.IsActive);
            if (loot == null || loot.Campaign.CreatedBy != userId)
            {
                return null;
            }

            if (dto.ChapterId.HasValue)
            {
                var chapterOk = await this.dbContext.Chapters
                    .AnyAsync(ch => ch.Id == dto.ChapterId.Value && ch.CampaignId == loot.CampaignId);
                if (!chapterOk)
                {
                    return null;
                }
            }

            loot.ChapterId = dto.ChapterId;
            loot.Name = dto.Name;
            loot.Description = dto.Description;
            loot.ImageUrl = dto.ImageUrl;
            loot.ItemType = dto.ItemType;
            loot.GameSpecificData = dto.GameSpecificData;
            loot.Quantity = dto.Quantity < 1 ? 1 : dto.Quantity;
            loot.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Updated loot {LootId}", lootId);
            return MapToDto(loot);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating loot {LootId}", lootId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int lootId, int userId)
    {
        try
        {
            var loot = await this.dbContext.CampaignLoots
                .Include(l => l.Campaign)
                .FirstOrDefaultAsync(l => l.Id == lootId && l.IsActive);
            if (loot == null || loot.Campaign.CreatedBy != userId)
            {
                return false;
            }

            loot.IsActive = false;
            loot.UpdatedAt = DateTime.UtcNow;
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Soft-deleted loot {LootId}", lootId);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting loot {LootId}", lootId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error, LootDistributionResultDto? Result)> DistributeAsync(int lootId, int worldCharacterId, int? sessionId, int userId)
    {
        try
        {
            var loot = await this.dbContext.CampaignLoots
                .Include(l => l.Campaign)
                .FirstOrDefaultAsync(l => l.Id == lootId && l.IsActive);
            if (loot == null)
            {
                return (false, "Loot introuvable.", null);
            }

            if (loot.Campaign.CreatedBy != userId)
            {
                return (false, "Seul le MJ de la campagne peut distribuer ce loot.", null);
            }

            var recipient = await this.dbContext.WorldCharacters
                .Include(w => w.Character)
                .FirstOrDefaultAsync(w => w.Id == worldCharacterId && w.IsActive);
            if (recipient == null)
            {
                return (false, "Personnage destinataire introuvable.", null);
            }

            // The recipient must belong to the campaign's world (guarantees a compatible game type).
            if (recipient.WorldId != loot.Campaign.WorldId)
            {
                return (false, "Ce personnage n'appartient pas au monde de la campagne.", null);
            }

            // Independent copy into the character's inventory.
            var inventoryItem = new DndInventoryItem
            {
                WorldCharacterId = worldCharacterId,
                Name = loot.Name,
                Category = string.IsNullOrWhiteSpace(loot.ItemType) ? "Objet" : loot.ItemType!,
                Quantity = loot.Quantity,
                Notes = loot.Description,
                GameType = loot.GameType,
                GameSpecificData = loot.GameSpecificData,
                ImageUrl = loot.ImageUrl,
                CreatedAt = DateTime.UtcNow,
            };
            this.dbContext.DndInventoryItems.Add(inventoryItem);

            this.dbContext.LootDistributions.Add(new LootDistribution
            {
                LootId = lootId,
                WorldCharacterId = worldCharacterId,
                SessionId = sessionId,
                DistributedByUserId = userId,
                DistributedAt = DateTime.UtcNow,
            });

            await this.dbContext.SaveChangesAsync();

            var recipientName = !string.IsNullOrWhiteSpace(recipient.Character.FirstName)
                ? recipient.Character.FirstName!
                : recipient.Character.Name;

            this.logger.LogInformation(
                "GM {UserId} distributed loot {LootId} to world character {WorldCharacterId}",
                userId,
                lootId,
                worldCharacterId);

            return (true, null, new LootDistributionResultDto
            {
                LootId = loot.Id,
                LootName = loot.Name,
                ImageUrl = loot.ImageUrl,
                ItemType = loot.ItemType,
                Quantity = loot.Quantity,
                RecipientWorldCharacterId = worldCharacterId,
                RecipientName = recipientName,
                RecipientUserId = recipient.Character.UserId,
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error distributing loot {LootId} to character {WorldCharacterId}", lootId, worldCharacterId);
            return (false, "Erreur lors de la distribution du loot.", null);
        }
    }

    private static CampaignLootDto MapToDto(CampaignLoot loot) => new()
    {
        Id = loot.Id,
        CampaignId = loot.CampaignId,
        ChapterId = loot.ChapterId,
        Name = loot.Name,
        Description = loot.Description,
        ImageUrl = loot.ImageUrl,
        GameType = loot.GameType,
        ItemType = loot.ItemType,
        GameSpecificData = loot.GameSpecificData,
        Quantity = loot.Quantity,
        CreatedAt = loot.CreatedAt,
    };
}
