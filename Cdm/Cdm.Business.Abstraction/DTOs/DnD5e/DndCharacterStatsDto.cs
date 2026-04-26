// -----------------------------------------------------------------------
// <copyright file="DndCharacterStatsDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>D&amp;D 5e stats applied to a world character (level, class, race, ability scores).</summary>
public class DndCharacterStatsDto
{
    public int WorldCharacterId { get; set; }
    public int? Level { get; set; }
    public string? CharacterClass { get; set; }
    public string? Subclass { get; set; }
    public string? Race { get; set; }
    public string? Subrace { get; set; }
    public string? Background { get; set; }
    public string? Alignment { get; set; }
    public int? ExperiencePoints { get; set; }
    public int? Strength { get; set; }
    public int? Dexterity { get; set; }
    public int? Constitution { get; set; }
    public int? Intelligence { get; set; }
    public int? Wisdom { get; set; }
    public int? Charisma { get; set; }
    public int? MaxHitPoints { get; set; }
    public int? CurrentHitPoints { get; set; }
    public int? ArmorClass { get; set; }
    public int? Speed { get; set; }

    /// <summary>Skills the character is proficient in (list of skill names in French).</summary>
    public List<string> SkillProficiencies { get; set; } = new();

    /// <summary>Saving throws the character is proficient in (list of ability codes, e.g. "FOR", "DEX").</summary>
    public List<string> SavingThrowProficiencies { get; set; } = new();

    // Computed (read-only, set by service)
    public int? ProficiencyBonus { get; set; }
    public int? StrengthModifier => Strength.HasValue ? (Strength.Value - 10) / 2 : null;
    public int? DexterityModifier => Dexterity.HasValue ? (Dexterity.Value - 10) / 2 : null;
    public int? ConstitutionModifier => Constitution.HasValue ? (Constitution.Value - 10) / 2 : null;
    public int? IntelligenceModifier => Intelligence.HasValue ? (Intelligence.Value - 10) / 2 : null;
    public int? WisdomModifier => Wisdom.HasValue ? (Wisdom.Value - 10) / 2 : null;
    public int? CharismaModifier => Charisma.HasValue ? (Charisma.Value - 10) / 2 : null;
}
