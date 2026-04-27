// -----------------------------------------------------------------------
// <copyright file="CreateDndNpcDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

using Cdm.Business.Abstraction.DTOs;

/// <summary>DTO for creating or updating a D&amp;D 5e NPC with game stats.</summary>
public class CreateDndNpcDto : CreateNpcDto
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
    /// <summary>Optional: template monster ID to pre-fill stats from.</summary>
    public int? MonsterTemplateId { get; set; }
}
