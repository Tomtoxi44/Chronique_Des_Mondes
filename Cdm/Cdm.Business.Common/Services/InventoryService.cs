// -----------------------------------------------------------------------
// <copyright file="InventoryService.cs" company="ANGIBAUD Tommy">
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
/// Universal inventory over the unified <c>DndInventoryItems</c> store. Ownership is the
/// character's owner; the D&amp;D combat columns are populated only for D&amp;D items.
/// </summary>
public class InventoryService(AppDbContext dbContext, ILogger<InventoryService> logger) : IInventoryService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<InventoryService> logger = logger;

    /// <inheritdoc/>
    public async Task<IEnumerable<InventoryItemDto>> GetForCharacterAsync(int worldCharacterId, int userId)
    {
        try
        {
            if (!await this.OwnsCharacterAsync(worldCharacterId, userId))
            {
                return Enumerable.Empty<InventoryItemDto>();
            }

            var items = await this.dbContext.DndInventoryItems
                .Where(i => i.WorldCharacterId == worldCharacterId)
                .OrderBy(i => i.Id)
                .ToListAsync();

            return items.Select(MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error listing inventory for world character {WorldCharacterId}", worldCharacterId);
            return Enumerable.Empty<InventoryItemDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InventoryItemDto>> GetForCharacterAsGmAsync(int worldCharacterId, int gmUserId)
    {
        try
        {
            // The GM is the owner of the world the character belongs to.
            var isGm = await this.dbContext.WorldCharacters
                .Include(w => w.World)
                .AnyAsync(w => w.Id == worldCharacterId && w.World.UserId == gmUserId);
            if (!isGm)
            {
                return Enumerable.Empty<InventoryItemDto>();
            }

            var items = await this.dbContext.DndInventoryItems
                .Where(i => i.WorldCharacterId == worldCharacterId)
                .OrderBy(i => i.Id)
                .ToListAsync();

            return items.Select(MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error listing inventory (GM view) for world character {WorldCharacterId}", worldCharacterId);
            return Enumerable.Empty<InventoryItemDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<InventoryItemDto?> AddAsync(int worldCharacterId, CreateInventoryItemDto dto, int userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || !await this.OwnsCharacterAsync(worldCharacterId, userId))
            {
                return null;
            }

            var item = new DndInventoryItem
            {
                WorldCharacterId = worldCharacterId,
                CreatedAt = DateTime.UtcNow,
            };
            Apply(item, dto);

            this.dbContext.DndInventoryItems.Add(item);
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Added inventory item {ItemId} to world character {WorldCharacterId}", item.Id, worldCharacterId);
            return MapToDto(item);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error adding inventory item to world character {WorldCharacterId}", worldCharacterId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<InventoryItemDto?> UpdateAsync(int itemId, CreateInventoryItemDto dto, int userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return null;
            }

            var item = await this.dbContext.DndInventoryItems
                .FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null || !await this.OwnsCharacterAsync(item.WorldCharacterId, userId))
            {
                return null;
            }

            Apply(item, dto);
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Updated inventory item {ItemId}", itemId);
            return MapToDto(item);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating inventory item {ItemId}", itemId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int itemId, int userId)
    {
        try
        {
            var item = await this.dbContext.DndInventoryItems
                .FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null || !await this.OwnsCharacterAsync(item.WorldCharacterId, userId))
            {
                return false;
            }

            this.dbContext.DndInventoryItems.Remove(item);
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Deleted inventory item {ItemId}", itemId);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting inventory item {ItemId}", itemId);
            return false;
        }
    }

    private async Task<bool> OwnsCharacterAsync(int worldCharacterId, int userId)
    {
        return await this.dbContext.WorldCharacters
            .Include(w => w.Character)
            .AnyAsync(w => w.Id == worldCharacterId && w.IsActive && w.Character.UserId == userId);
    }

    private static void Apply(DndInventoryItem item, CreateInventoryItemDto dto)
    {
        item.Name = dto.Name.Trim();
        item.Category = string.IsNullOrWhiteSpace(dto.Category) ? "Objet" : dto.Category.Trim();
        item.Quantity = dto.Quantity < 1 ? 1 : dto.Quantity;
        item.Notes = dto.Description;
        item.ImageUrl = dto.ImageUrl;
        item.GameType = dto.GameType;
        item.GameSpecificData = dto.GameSpecificData;

        // D&D combat fields only make sense for D&D items; clear them otherwise.
        if (dto.GameType == Cdm.Common.Enums.GameType.DnD5e)
        {
            item.AttackBonus = dto.AttackBonus;
            item.DamageDice = dto.DamageDice;
            item.DamageType = dto.DamageType;
        }
        else
        {
            item.AttackBonus = null;
            item.DamageDice = null;
            item.DamageType = null;
        }
    }

    private static InventoryItemDto MapToDto(DndInventoryItem item) => new()
    {
        Id = item.Id,
        WorldCharacterId = item.WorldCharacterId,
        Name = item.Name,
        Category = item.Category,
        Quantity = item.Quantity,
        Description = item.Notes,
        ImageUrl = item.ImageUrl,
        GameType = item.GameType,
        GameSpecificData = item.GameSpecificData,
        AttackBonus = item.AttackBonus,
        DamageDice = item.DamageDice,
        DamageType = item.DamageType,
    };
}
