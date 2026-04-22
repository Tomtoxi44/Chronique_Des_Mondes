// -----------------------------------------------------------------------
// <copyright file="DndMonsterTemplateDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>D&amp;D 5e monster template for GM NPC creation.</summary>
public class DndMonsterTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MonsterType { get; set; }
    public string? ChallengeRating { get; set; }
    public string? Description { get; set; }
    public int HitPoints { get; set; }
    public string? HitDice { get; set; }
    public int ArmorClass { get; set; }
    public int Speed { get; set; }
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
    public string? Actions { get; set; }
    public List<string> SpecialAbilities { get; set; } = new();
    public string? Alignment { get; set; }
}
