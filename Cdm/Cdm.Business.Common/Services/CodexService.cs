// -----------------------------------------------------------------------
// <copyright file="CodexService.cs" company="ANGIBAUD Tommy">
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
/// Manages a user's personal codex of item templates (CRUD, ownership-scoped).
/// </summary>
public class CodexService(AppDbContext dbContext, ILogger<CodexService> logger) : ICodexService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<CodexService> logger = logger;

    /// <inheritdoc/>
    public async Task<CodexItemDto?> CreateAsync(CreateCodexItemDto dto, int userId)
    {
        try
        {
            var item = new CodexItem
            {
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                GameType = dto.GameType,
                ItemType = dto.ItemType,
                GameSpecificData = dto.GameSpecificData,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            this.dbContext.CodexItems.Add(item);
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Created codex item {ItemId} for user {UserId}", item.Id, userId);
            return MapToDto(item);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating codex item for user {UserId}", userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CodexItemDto>> GetMyItemsAsync(int userId, GameType? gameType = null)
    {
        try
        {
            var query = this.dbContext.CodexItems
                .Where(c => c.UserId == userId && c.IsActive);

            if (gameType.HasValue)
            {
                query = query.Where(c => c.GameType == gameType.Value);
            }

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return items.Select(MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error listing codex items for user {UserId}", userId);
            return Enumerable.Empty<CodexItemDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<CodexItemDto?> GetByIdAsync(int id, int userId)
    {
        try
        {
            var item = await this.dbContext.CodexItems
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && c.IsActive);
            return item == null ? null : MapToDto(item);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching codex item {ItemId} for user {UserId}", id, userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CodexItemDto?> UpdateAsync(int id, CreateCodexItemDto dto, int userId)
    {
        try
        {
            var item = await this.dbContext.CodexItems
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && c.IsActive);
            if (item == null)
            {
                return null;
            }

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.ImageUrl = dto.ImageUrl;
            item.GameType = dto.GameType;
            item.ItemType = dto.ItemType;
            item.GameSpecificData = dto.GameSpecificData;
            item.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Updated codex item {ItemId}", id);
            return MapToDto(item);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating codex item {ItemId} for user {UserId}", id, userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, int userId)
    {
        try
        {
            var item = await this.dbContext.CodexItems
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && c.IsActive);
            if (item == null)
            {
                return false;
            }

            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Soft-deleted codex item {ItemId}", id);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting codex item {ItemId} for user {UserId}", id, userId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error)> AddToCharacterInventoryAsync(int codexItemId, int worldCharacterId, int userId)
    {
        try
        {
            var item = await this.dbContext.CodexItems
                .FirstOrDefaultAsync(c => c.Id == codexItemId && c.UserId == userId && c.IsActive);
            if (item == null)
            {
                return (false, "Item du codex introuvable.");
            }

            var character = await this.dbContext.WorldCharacters
                .Include(w => w.Character)
                .Include(w => w.World)
                .FirstOrDefaultAsync(w => w.Id == worldCharacterId && w.IsActive);
            if (character == null)
            {
                return (false, "Personnage introuvable.");
            }

            if (character.Character.UserId != userId)
            {
                return (false, "Ce personnage ne vous appartient pas.");
            }

            if (character.World.GameType != item.GameType)
            {
                return (false, "Le type de jeu de l'item ne correspond pas à celui du personnage.");
            }

            // Copie indépendante dans l'inventaire (la modification ultérieure du codex n'affecte pas la copie).
            var inventoryItem = new DndInventoryItem
            {
                WorldCharacterId = worldCharacterId,
                Name = item.Name,
                Category = string.IsNullOrWhiteSpace(item.ItemType) ? "Objet" : item.ItemType!,
                Quantity = 1,
                Notes = item.Description,
                GameType = item.GameType,
                GameSpecificData = item.GameSpecificData,
                ImageUrl = item.ImageUrl,
                CreatedAt = DateTime.UtcNow,
            };

            this.dbContext.DndInventoryItems.Add(inventoryItem);
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Copied codex item {ItemId} into inventory of world character {WorldCharacterId}",
                codexItemId,
                worldCharacterId);
            return (true, null);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error adding codex item {ItemId} to character {WorldCharacterId}", codexItemId, worldCharacterId);
            return (false, "Erreur lors de l'ajout à l'inventaire.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SetSharedAsync(int id, int userId, bool isShared)
    {
        try
        {
            var item = await this.dbContext.CodexItems
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && c.IsActive);
            if (item == null)
            {
                return false;
            }

            item.IsShared = isShared;
            item.UpdatedAt = DateTime.UtcNow;
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Codex item {ItemId} share set to {IsShared}", id, isShared);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error setting share on codex item {ItemId}", id);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CodexItemDto>> GetMarketplaceItemsAsync(GameType? gameType = null, string? search = null)
    {
        try
        {
            var query = this.dbContext.CodexItems
                .Include(c => c.User)
                .Where(c => c.IsActive && c.IsShared);

            if (gameType.HasValue)
            {
                query = query.Where(c => c.GameType == gameType.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(c => c.Name.Contains(term));
            }

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return items.Select(c =>
            {
                var dto = MapToDto(c);
                dto.SharedByName = c.User != null ? c.User.Nickname : null;
                return dto;
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error listing marketplace items");
            return Enumerable.Empty<CodexItemDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<CodexItemDto?> ImportToMyCodexAsync(int sourceItemId, int userId)
    {
        try
        {
            var source = await this.dbContext.CodexItems
                .FirstOrDefaultAsync(c => c.Id == sourceItemId && c.IsActive && c.IsShared);
            if (source == null)
            {
                return null;
            }

            // Copie indépendante dans le codex de l'utilisateur (non partagée par défaut).
            var copy = new CodexItem
            {
                UserId = userId,
                Name = source.Name,
                Description = source.Description,
                ImageUrl = source.ImageUrl,
                GameType = source.GameType,
                ItemType = source.ItemType,
                GameSpecificData = source.GameSpecificData,
                IsShared = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            this.dbContext.CodexItems.Add(copy);
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("User {UserId} imported marketplace item {SourceId} as {NewId}", userId, sourceItemId, copy.Id);
            return MapToDto(copy);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error importing marketplace item {SourceId} for user {UserId}", sourceItemId, userId);
            return null;
        }
    }

    private static CodexItemDto MapToDto(CodexItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        ImageUrl = item.ImageUrl,
        GameType = item.GameType,
        ItemType = item.ItemType,
        GameSpecificData = item.GameSpecificData,
        IsShared = item.IsShared,
        CreatedAt = item.CreatedAt,
    };
}
