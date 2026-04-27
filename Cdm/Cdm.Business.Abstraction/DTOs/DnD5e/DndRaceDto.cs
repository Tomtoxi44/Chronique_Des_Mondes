// -----------------------------------------------------------------------
// <copyright file="DndRaceDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>D&amp;D 5e race reference data.</summary>
public class DndRaceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Speed { get; set; }
    /// <summary>Stat bonuses as dictionary: {"Strength":2,"Dexterity":1}</summary>
    public Dictionary<string, int> StatBonuses { get; set; } = new();
    public List<string> Traits { get; set; } = new();
    public List<string> Subraces { get; set; } = new();
    /// <summary>Subrace-specific bonus overrides: {"High Elf": {"Intelligence":1}}</summary>
    public Dictionary<string, Dictionary<string, int>> SubraceStatBonuses { get; set; } = new();
}
