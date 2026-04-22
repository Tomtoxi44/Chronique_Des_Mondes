// -----------------------------------------------------------------------
// <copyright file="DndReferenceService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.DnD5e.Services;

using Cdm.Business.Abstraction.DTOs.DnD5e;
using Cdm.Business.Abstraction.Services.DnD5e;
using Cdm.Data.Common;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

/// <summary>Read-only access to D&amp;D 5e reference data seeded in the database.</summary>
public class DndReferenceService(AppDbContext dbContext) : IDndReferenceService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // ── Races ────────────────────────────────────────────────────────────

    public async Task<IEnumerable<DndRaceDto>> GetRacesAsync()
    {
        var races = await dbContext.DndRaces.AsNoTracking().ToListAsync();
        return races.Select(MapRace);
    }

    public async Task<DndRaceDto?> GetRaceByIdAsync(int id)
    {
        var race = await dbContext.DndRaces.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return race is null ? null : MapRace(race);
    }

    // ── Classes ──────────────────────────────────────────────────────────

    public async Task<IEnumerable<DndClassDto>> GetClassesAsync()
    {
        var classes = await dbContext.DndClasses.AsNoTracking().ToListAsync();
        return classes.Select(MapClass);
    }

    public async Task<DndClassDto?> GetClassByIdAsync(int id)
    {
        var cls = await dbContext.DndClasses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return cls is null ? null : MapClass(cls);
    }

    // ── Items ────────────────────────────────────────────────────────────

    public async Task<IEnumerable<DndItemDto>> GetItemsAsync(string? category = null)
    {
        var query = dbContext.DndItems.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(i => i.Category == category);
        return (await query.ToListAsync()).Select(MapItem);
    }

    public async Task<DndItemDto?> GetItemByIdAsync(int id)
    {
        var item = await dbContext.DndItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return item is null ? null : MapItem(item);
    }

    // ── Spells ───────────────────────────────────────────────────────────

    public async Task<IEnumerable<DndSpellDto>> GetSpellsAsync(int? level = null, string? characterClass = null)
    {
        var spells = await dbContext.DndSpells.AsNoTracking().ToListAsync();

        if (level.HasValue)
            spells = spells.Where(s => s.Level == level.Value).ToList();

        if (!string.IsNullOrWhiteSpace(characterClass))
        {
            spells = spells.Where(s =>
            {
                if (string.IsNullOrWhiteSpace(s.Classes)) return false;
                var classList = JsonSerializer.Deserialize<List<string>>(s.Classes, JsonOpts) ?? [];
                return classList.Any(c => string.Equals(c, characterClass, StringComparison.OrdinalIgnoreCase));
            }).ToList();
        }

        return spells.Select(MapSpell);
    }

    public async Task<DndSpellDto?> GetSpellByIdAsync(int id)
    {
        var spell = await dbContext.DndSpells.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return spell is null ? null : MapSpell(spell);
    }

    // ── Monsters ─────────────────────────────────────────────────────────

    public async Task<IEnumerable<DndMonsterTemplateDto>> GetMonsterTemplatesAsync()
    {
        var monsters = await dbContext.DndMonsterTemplates.AsNoTracking().ToListAsync();
        return monsters.Select(MapMonster);
    }

    public async Task<DndMonsterTemplateDto?> GetMonsterTemplateByIdAsync(int id)
    {
        var m = await dbContext.DndMonsterTemplates.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return m is null ? null : MapMonster(m);
    }

    // ── Mappers ──────────────────────────────────────────────────────────

    private static DndRaceDto MapRace(Data.Common.Models.DndRace r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        Speed = r.Speed,
        StatBonuses = DeserializeDict(r.StatBonuses),
        Traits = DeserializeList(r.Traits),
        Subraces = DeserializeList(r.Subraces),
        SubraceStatBonuses = string.IsNullOrWhiteSpace(r.SubraceStatBonuses)
            ? new()
            : JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(r.SubraceStatBonuses, JsonOpts) ?? new()
    };

    private static DndClassDto MapClass(Data.Common.Models.DndClass c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        HitDie = c.HitDie,
        IsSpellcaster = c.IsSpellcaster,
        PrimaryAbilities = DeserializeList(c.PrimaryAbilities),
        SavingThrows = DeserializeList(c.SavingThrows),
        Subclasses = DeserializeList(c.Subclasses)
    };

    private static DndItemDto MapItem(Data.Common.Models.DndItem i) => new()
    {
        Id = i.Id,
        Name = i.Name,
        Category = i.Category,
        Description = i.Description,
        DamageDice = i.DamageDice,
        DamageType = i.DamageType,
        WeaponRange = i.WeaponRange,
        ArmorClassBonus = i.ArmorClassBonus,
        Weight = i.Weight,
        CostGp = i.CostGp,
        Properties = DeserializeList(i.Properties),
        HealingDice = i.HealingDice
    };

    private static DndSpellDto MapSpell(Data.Common.Models.DndSpell s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Level = s.Level,
        School = s.School,
        Description = s.Description,
        CastingTime = s.CastingTime,
        Range = s.Range,
        Duration = s.Duration,
        DamageDice = s.DamageDice,
        DamageType = s.DamageType,
        Classes = DeserializeList(s.Classes),
        Components = s.Components,
        RequiresConcentration = s.RequiresConcentration,
        IsRitual = s.IsRitual
    };

    private static DndMonsterTemplateDto MapMonster(Data.Common.Models.DndMonsterTemplate m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        MonsterType = m.MonsterType,
        ChallengeRating = m.ChallengeRating,
        Description = m.Description,
        HitPoints = m.HitPoints,
        HitDice = m.HitDice,
        ArmorClass = m.ArmorClass,
        Speed = m.Speed,
        Strength = m.Strength,
        Dexterity = m.Dexterity,
        Constitution = m.Constitution,
        Intelligence = m.Intelligence,
        Wisdom = m.Wisdom,
        Charisma = m.Charisma,
        Actions = m.Actions,
        SpecialAbilities = DeserializeList(m.SpecialAbilities),
        Alignment = m.Alignment
    };

    private static Dictionary<string, int> DeserializeDict(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        return JsonSerializer.Deserialize<Dictionary<string, int>>(json, JsonOpts) ?? new();
    }

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        return JsonSerializer.Deserialize<List<string>>(json, JsonOpts) ?? new();
    }
}
