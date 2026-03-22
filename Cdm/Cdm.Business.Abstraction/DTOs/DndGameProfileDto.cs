// -----------------------------------------------------------------------
// <copyright file="DndGameProfileDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// DTO for D&D 5e game profile data (to be stored in GameSpecificData JSON).
/// </summary>
public class DndGameProfileDto
{
    /// <summary>
    /// Gets or sets Strength ability score (1-20).
    /// </summary>
    public int Strength { get; set; }

    /// <summary>
    /// Gets or sets Dexterity ability score (1-20).
    /// </summary>
    public int Dexterity { get; set; }

    /// <summary>
    /// Gets or sets Constitution ability score (1-20).
    /// </summary>
    public int Constitution { get; set; }

    /// <summary>
    /// Gets or sets Intelligence ability score (1-20).
    /// </summary>
    public int Intelligence { get; set; }

    /// <summary>
    /// Gets or sets Wisdom ability score (1-20).
    /// </summary>
    public int Wisdom { get; set; }

    /// <summary>
    /// Gets or sets Charisma ability score (1-20).
    /// </summary>
    public int Charisma { get; set; }

    /// <summary>
    /// Gets or sets the character class (Fighter, Wizard, etc.).
    /// </summary>
    public string? CharacterClass { get; set; }

    /// <summary>
    /// Gets or sets the character race (Human, Elf, Dwarf, etc.).
    /// </summary>
    public string? Race { get; set; }

    /// <summary>
    /// Gets or sets the character background.
    /// </summary>
    public string? Background { get; set; }

    /// <summary>
    /// Gets or sets the armor class.
    /// </summary>
    public int? ArmorClass { get; set; }

    /// <summary>
    /// Gets or sets the initiative bonus.
    /// </summary>
    public int? InitiativeBonus { get; set; }

    /// <summary>
    /// Gets or sets the speed (in feet).
    /// </summary>
    public int? Speed { get; set; }
}
