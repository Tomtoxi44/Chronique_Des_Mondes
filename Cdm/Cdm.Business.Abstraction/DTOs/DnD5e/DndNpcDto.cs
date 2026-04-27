// -----------------------------------------------------------------------
// <copyright file="DndNpcDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

using Cdm.Business.Abstraction.DTOs;

/// <summary>NPC DTO extended with D&amp;D 5e stats.</summary>
public class DndNpcDto : NpcDto
{
    public int? Level { get; set; }
    public int? HitPoints { get; set; }
    public int? ArmorClass { get; set; }
    public int? Speed { get; set; }
    public int? Strength { get; set; }
    public int? Dexterity { get; set; }
    public int? Constitution { get; set; }
    public int? Intelligence { get; set; }
    public int? Wisdom { get; set; }
    public int? Charisma { get; set; }
    public string? Race { get; set; }
    public string? CharacterClass { get; set; }
    public int? ProficiencyBonus { get; set; }

    // Computed modifiers
    public int? StrengthModifier => Strength.HasValue ? (Strength.Value - 10) / 2 : null;
    public int? DexterityModifier => Dexterity.HasValue ? (Dexterity.Value - 10) / 2 : null;
    public int? ConstitutionModifier => Constitution.HasValue ? (Constitution.Value - 10) / 2 : null;
    public int? IntelligenceModifier => Intelligence.HasValue ? (Intelligence.Value - 10) / 2 : null;
    public int? WisdomModifier => Wisdom.HasValue ? (Wisdom.Value - 10) / 2 : null;
    public int? CharismaModifier => Charisma.HasValue ? (Charisma.Value - 10) / 2 : null;
}
