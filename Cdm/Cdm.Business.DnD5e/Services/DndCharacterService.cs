// -----------------------------------------------------------------------
// <copyright file="DndCharacterService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.DnD5e.Services;

using System.Text.Json;
using Cdm.Business.Abstraction.DTOs.DnD5e;
using Cdm.Business.Abstraction.Services.DnD5e;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages D&amp;D 5e stats, inventory and spells for world characters.
/// Stats are stored as JSON in <c>WorldCharacter.GameSpecificData</c>.
/// Inventory and spells use their own tables.
/// </summary>
public class DndCharacterService(AppDbContext dbContext, ILogger<DndCharacterService> logger) : IDndCharacterService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    // ── Stats ────────────────────────────────────────────────────────────

    public async Task<bool> ApplyDndStatsAsync(int worldCharacterId, DndCharacterStatsDto stats, int requestingUserId)
    {
        var wc = await dbContext.WorldCharacters
            .Include(w => w.Character)
            .FirstOrDefaultAsync(w => w.Id == worldCharacterId && w.IsActive);

        if (wc is null) return false;
        if (wc.Character.UserId != requestingUserId)
        {
            logger.LogWarning("User {UserId} not authorized to update world character {WcId}", requestingUserId, worldCharacterId);
            return false;
        }

        // Apply race bonuses if race and class are provided
        var adjustedStats = await ApplyRaceBonusesAsync(stats);

        // Compute proficiency bonus
        if (adjustedStats.Level.HasValue)
            adjustedStats.ProficiencyBonus = CalculateProficiencyBonus(adjustedStats.Level.Value);

        wc.GameSpecificData = JsonSerializer.Serialize(adjustedStats, JsonOpts);
        if (adjustedStats.Level.HasValue) wc.Level = adjustedStats.Level;
        if (adjustedStats.MaxHitPoints.HasValue) wc.MaxHealth = adjustedStats.MaxHitPoints;
        if (adjustedStats.CurrentHitPoints.HasValue) wc.CurrentHealth = adjustedStats.CurrentHitPoints;
        wc.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<DndCharacterStatsDto?> GetDndStatsAsync(int worldCharacterId, int requestingUserId)
    {
        var wc = await dbContext.WorldCharacters
            .Include(w => w.Character)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == worldCharacterId && w.IsActive);

        if (wc is null) return null;
        if (wc.Character.UserId != requestingUserId && !await IsGmOfWorldAsync(wc.WorldId, requestingUserId))
            return null;

        if (string.IsNullOrWhiteSpace(wc.GameSpecificData)) return new DndCharacterStatsDto { WorldCharacterId = worldCharacterId };

        try
        {
            var stats = JsonSerializer.Deserialize<DndCharacterStatsDto>(wc.GameSpecificData, JsonOpts) ?? new();
            stats.WorldCharacterId = worldCharacterId;
            return stats;
        }
        catch
        {
            return new DndCharacterStatsDto { WorldCharacterId = worldCharacterId };
        }
    }

    // ── Inventory ────────────────────────────────────────────────────────

    public async Task<IEnumerable<DndInventoryItemDto>> GetInventoryAsync(int worldCharacterId, int requestingUserId)
    {
        if (!await CanAccessWorldCharacterAsync(worldCharacterId, requestingUserId))
            return Enumerable.Empty<DndInventoryItemDto>();

        var items = await dbContext.DndInventoryItems
            .AsNoTracking()
            .Where(i => i.WorldCharacterId == worldCharacterId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();

        return items.Select(MapInventoryItem);
    }

    public async Task<DndInventoryItemDto> AddInventoryItemAsync(int worldCharacterId, DndInventoryItemDto dto, int requestingUserId)
    {
        if (!await IsCharacterOwnerAsync(worldCharacterId, requestingUserId))
            throw new UnauthorizedAccessException("Only the character owner can manage their inventory.");

        var item = new DndInventoryItem
        {
            WorldCharacterId = worldCharacterId,
            Name = dto.Name,
            Category = dto.Category,
            Quantity = dto.Quantity,
            AttackBonus = dto.AttackBonus,
            DamageDice = dto.DamageDice,
            DamageType = dto.DamageType,
            Notes = dto.Notes,
            DndItemId = dto.DndItemId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.DndInventoryItems.Add(item);
        await dbContext.SaveChangesAsync();
        return MapInventoryItem(item);
    }

    public async Task<bool> RemoveInventoryItemAsync(int worldCharacterId, int itemId, int requestingUserId)
    {
        if (!await IsCharacterOwnerAsync(worldCharacterId, requestingUserId)) return false;

        var item = await dbContext.DndInventoryItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.WorldCharacterId == worldCharacterId);

        if (item is null) return false;
        dbContext.DndInventoryItems.Remove(item);
        await dbContext.SaveChangesAsync();
        return true;
    }

    // ── Spells ───────────────────────────────────────────────────────────

    public async Task<IEnumerable<DndCharacterSpellDto>> GetSpellsAsync(int worldCharacterId, int requestingUserId)
    {
        if (!await CanAccessWorldCharacterAsync(worldCharacterId, requestingUserId))
            return Enumerable.Empty<DndCharacterSpellDto>();

        var spells = await dbContext.DndCharacterSpells
            .AsNoTracking()
            .Where(s => s.WorldCharacterId == worldCharacterId)
            .OrderBy(s => s.Level).ThenBy(s => s.Name)
            .ToListAsync();

        return spells.Select(MapSpell);
    }

    public async Task<DndCharacterSpellDto> AddSpellAsync(int worldCharacterId, DndCharacterSpellDto dto, int requestingUserId)
    {
        if (!await IsCharacterOwnerAsync(worldCharacterId, requestingUserId))
            throw new UnauthorizedAccessException("Only the character owner can manage their spells.");

        var spell = new DndCharacterSpell
        {
            WorldCharacterId = worldCharacterId,
            Name = dto.Name,
            Level = dto.Level,
            School = dto.School,
            Description = dto.Description,
            DamageDice = dto.DamageDice,
            DamageType = dto.DamageType,
            IsPrepared = dto.IsPrepared,
            DndSpellId = dto.DndSpellId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.DndCharacterSpells.Add(spell);
        await dbContext.SaveChangesAsync();
        return MapSpell(spell);
    }

    public async Task<bool> RemoveSpellAsync(int worldCharacterId, int spellId, int requestingUserId)
    {
        if (!await IsCharacterOwnerAsync(worldCharacterId, requestingUserId)) return false;

        var spell = await dbContext.DndCharacterSpells
            .FirstOrDefaultAsync(s => s.Id == spellId && s.WorldCharacterId == worldCharacterId);

        if (spell is null) return false;
        dbContext.DndCharacterSpells.Remove(spell);
        await dbContext.SaveChangesAsync();
        return true;
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private async Task<bool> IsCharacterOwnerAsync(int worldCharacterId, int userId)
    {
        return await dbContext.WorldCharacters
            .Include(wc => wc.Character)
            .AnyAsync(wc => wc.Id == worldCharacterId && wc.Character.UserId == userId && wc.IsActive);
    }

    private async Task<bool> CanAccessWorldCharacterAsync(int worldCharacterId, int userId)
    {
        var wc = await dbContext.WorldCharacters
            .Include(wc => wc.Character)
            .Include(wc => wc.World)
            .AsNoTracking()
            .FirstOrDefaultAsync(wc => wc.Id == worldCharacterId && wc.IsActive);

        if (wc is null) return false;
        return wc.Character.UserId == userId || wc.World.UserId == userId;
    }

    private async Task<bool> IsGmOfWorldAsync(int worldId, int userId)
    {
        return await dbContext.Worlds.AnyAsync(w => w.Id == worldId && w.UserId == userId);
    }

    private async Task<DndCharacterStatsDto> ApplyRaceBonusesAsync(DndCharacterStatsDto stats)
    {
        if (string.IsNullOrWhiteSpace(stats.Race)) return stats;

        var race = await dbContext.DndRaces
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == stats.Race);

        if (race is null) return stats;

        var bonuses = new Dictionary<string, int>();

        // Base race bonuses
        if (!string.IsNullOrWhiteSpace(race.StatBonuses))
        {
            try
            {
                var baseBonuses = JsonSerializer.Deserialize<Dictionary<string, int>>(race.StatBonuses) ?? new();
                foreach (var kv in baseBonuses) bonuses[kv.Key] = kv.Value;
            }
            catch { /* ignore */ }
        }

        // Subrace bonuses (override/merge)
        if (!string.IsNullOrWhiteSpace(stats.Subrace) && !string.IsNullOrWhiteSpace(race.SubraceStatBonuses))
        {
            try
            {
                var subraceBonuses = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(race.SubraceStatBonuses) ?? new();
                if (subraceBonuses.TryGetValue(stats.Subrace, out var sub))
                {
                    foreach (var kv in sub)
                        bonuses[kv.Key] = bonuses.TryGetValue(kv.Key, out var existing) ? existing + kv.Value : kv.Value;
                }
            }
            catch { /* ignore */ }
        }

        // Apply bonuses to scores
        if (bonuses.TryGetValue("Strength", out var str)) stats.Strength = (stats.Strength ?? 10) + str;
        if (bonuses.TryGetValue("Dexterity", out var dex)) stats.Dexterity = (stats.Dexterity ?? 10) + dex;
        if (bonuses.TryGetValue("Constitution", out var con)) stats.Constitution = (stats.Constitution ?? 10) + con;
        if (bonuses.TryGetValue("Intelligence", out var intel)) stats.Intelligence = (stats.Intelligence ?? 10) + intel;
        if (bonuses.TryGetValue("Wisdom", out var wis)) stats.Wisdom = (stats.Wisdom ?? 10) + wis;
        if (bonuses.TryGetValue("Charisma", out var cha)) stats.Charisma = (stats.Charisma ?? 10) + cha;

        return stats;
    }

    private static int CalculateProficiencyBonus(int level) => level switch
    {
        >= 1 and <= 4 => 2,
        >= 5 and <= 8 => 3,
        >= 9 and <= 12 => 4,
        >= 13 and <= 16 => 5,
        >= 17 and <= 20 => 6,
        _ => 2
    };

    private static DndInventoryItemDto MapInventoryItem(DndInventoryItem i) => new()
    {
        Id = i.Id,
        WorldCharacterId = i.WorldCharacterId,
        Name = i.Name,
        Category = i.Category,
        Quantity = i.Quantity,
        AttackBonus = i.AttackBonus,
        DamageDice = i.DamageDice,
        DamageType = i.DamageType,
        Notes = i.Notes,
        DndItemId = i.DndItemId
    };

    private static DndCharacterSpellDto MapSpell(DndCharacterSpell s) => new()
    {
        Id = s.Id,
        WorldCharacterId = s.WorldCharacterId,
        Name = s.Name,
        Level = s.Level,
        School = s.School,
        Description = s.Description,
        DamageDice = s.DamageDice,
        DamageType = s.DamageType,
        IsPrepared = s.IsPrepared,
        DndSpellId = s.DndSpellId
    };
}
