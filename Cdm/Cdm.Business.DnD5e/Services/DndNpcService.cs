// -----------------------------------------------------------------------
// <copyright file="DndNpcService.cs" company="ANGIBAUD Tommy">
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

/// <summary>D&amp;D 5e NPC service — stores game stats as JSON in <c>GameSpecificData</c>.</summary>
public class DndNpcService(AppDbContext dbContext, ILogger<DndNpcService> logger) : IDndNpcService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    // ── Queries ──────────────────────────────────────────────────────────

    public async Task<IEnumerable<DndNpcDto>> GetDndNpcsAsync(int chapterId)
    {
        var npcs = await dbContext.NonPlayerCharacters
            .AsNoTracking()
            .Where(n => n.ChapterId == chapterId && n.IsActive)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();

        return npcs.Select(MapToDto);
    }

    public async Task<DndNpcDto?> GetDndNpcByIdAsync(int npcId)
    {
        var npc = await dbContext.NonPlayerCharacters
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == npcId && n.IsActive);

        return npc is null ? null : MapToDto(npc);
    }

    // ── Commands ─────────────────────────────────────────────────────────

    public async Task<DndNpcDto> CreateDndNpcAsync(int chapterId, CreateDndNpcDto dto, int requestingUserId)
    {
        if (!await IsGmOfChapterAsync(chapterId, requestingUserId))
        {
            logger.LogWarning("User {UserId} not authorized to create NPCs in chapter {ChapterId}", requestingUserId, chapterId);
            throw new UnauthorizedAccessException("Only the GM can create NPCs.");
        }

        // If a monster template is requested, use it to pre-fill missing stats
        if (dto.MonsterTemplateId.HasValue)
        {
            var template = await dbContext.DndMonsterTemplates.FindAsync(dto.MonsterTemplateId.Value);
            if (template != null)
            {
                dto.Strength ??= template.Strength;
                dto.Dexterity ??= template.Dexterity;
                dto.Constitution ??= template.Constitution;
                dto.Intelligence ??= template.Intelligence;
                dto.Wisdom ??= template.Wisdom;
                dto.Charisma ??= template.Charisma;
                dto.HitPoints ??= template.HitPoints;
                dto.ArmorClass ??= template.ArmorClass;
                dto.Race ??= template.MonsterType;
            }
        }

        var gameData = BuildGameSpecificData(dto);

        var npc = new NonPlayerCharacter
        {
            ChapterId = chapterId,
            Name = dto.Name,
            FirstName = dto.FirstName,
            Description = dto.Description,
            PhysicalDescription = dto.PhysicalDescription,
            Age = dto.Age,
            GameSpecificData = gameData,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.NonPlayerCharacters.Add(npc);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created D&D NPC {NpcId} in chapter {ChapterId}", npc.Id, chapterId);
        return MapToDto(npc);
    }

    public async Task<DndNpcDto?> UpdateDndNpcAsync(int npcId, CreateDndNpcDto dto, int requestingUserId)
    {
        var npc = await dbContext.NonPlayerCharacters.FirstOrDefaultAsync(n => n.Id == npcId && n.IsActive);
        if (npc is null) return null;

        if (!await IsGmOfChapterAsync(npc.ChapterId, requestingUserId))
        {
            logger.LogWarning("User {UserId} not authorized to update NPC {NpcId}", requestingUserId, npcId);
            return null;
        }

        npc.Name = dto.Name;
        npc.FirstName = dto.FirstName;
        npc.Description = dto.Description;
        npc.PhysicalDescription = dto.PhysicalDescription;
        npc.Age = dto.Age;
        npc.GameSpecificData = BuildGameSpecificData(dto);
        npc.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return MapToDto(npc);
    }

    public async Task<bool> DeleteDndNpcAsync(int npcId, int requestingUserId)
    {
        var npc = await dbContext.NonPlayerCharacters.FirstOrDefaultAsync(n => n.Id == npcId && n.IsActive);
        if (npc is null) return false;

        if (!await IsGmOfChapterAsync(npc.ChapterId, requestingUserId))
            return false;

        npc.IsActive = false;
        npc.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return true;
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private async Task<bool> IsGmOfChapterAsync(int chapterId, int userId)
    {
        var chapter = await dbContext.Chapters
            .Include(ch => ch.Campaign).ThenInclude(c => c.World)
            .FirstOrDefaultAsync(ch => ch.Id == chapterId && ch.IsActive);

        if (chapter is null) return false;
        return chapter.Campaign.World?.UserId == userId || chapter.Campaign.CreatedBy == userId;
    }

    private static string BuildGameSpecificData(CreateDndNpcDto dto)
    {
        var stats = new
        {
            level = dto.Level,
            hitPoints = dto.HitPoints,
            armorClass = dto.ArmorClass,
            strength = dto.Strength,
            dexterity = dto.Dexterity,
            constitution = dto.Constitution,
            intelligence = dto.Intelligence,
            wisdom = dto.Wisdom,
            charisma = dto.Charisma,
            race = dto.Race,
            @class = dto.CharacterClass,
            proficiencyBonus = dto.Level.HasValue
                ? CalculateProficiencyBonus(dto.Level.Value)
                : (int?)null
        };
        return JsonSerializer.Serialize(stats, JsonOpts);
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

    private static DndNpcDto MapToDto(NonPlayerCharacter npc)
    {
        var dto = new DndNpcDto
        {
            Id = npc.Id,
            ChapterId = npc.ChapterId,
            Name = npc.Name,
            FirstName = npc.FirstName,
            Description = npc.Description,
            PhysicalDescription = npc.PhysicalDescription,
            Age = npc.Age,
            CreatedAt = npc.CreatedAt,
            UpdatedAt = npc.UpdatedAt
        };

        if (!string.IsNullOrWhiteSpace(npc.GameSpecificData))
        {
            try
            {
                using var doc = JsonDocument.Parse(npc.GameSpecificData);
                var root = doc.RootElement;
                dto.Level = TryGetInt(root, "level");
                dto.HitPoints = TryGetInt(root, "hitPoints");
                dto.ArmorClass = TryGetInt(root, "armorClass");
                dto.Strength = TryGetInt(root, "strength");
                dto.Dexterity = TryGetInt(root, "dexterity");
                dto.Constitution = TryGetInt(root, "constitution");
                dto.Intelligence = TryGetInt(root, "intelligence");
                dto.Wisdom = TryGetInt(root, "wisdom");
                dto.Charisma = TryGetInt(root, "charisma");
                dto.Race = TryGetString(root, "race");
                dto.CharacterClass = TryGetString(root, "class");
                dto.ProficiencyBonus = TryGetInt(root, "proficiencyBonus");
            }
            catch { /* ignore malformed JSON */ }
        }

        return dto;
    }

    private static int? TryGetInt(JsonElement root, string prop) =>
        root.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.Number
            ? el.GetInt32()
            : null;

    private static string? TryGetString(JsonElement root, string prop) =>
        root.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;
}
