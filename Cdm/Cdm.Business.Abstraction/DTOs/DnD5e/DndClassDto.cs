// -----------------------------------------------------------------------
// <copyright file="DndClassDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>D&amp;D 5e class reference data.</summary>
public class DndClassDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int HitDie { get; set; }
    public bool IsSpellcaster { get; set; }
    public List<string> PrimaryAbilities { get; set; } = new();
    public List<string> SavingThrows { get; set; } = new();
    public List<string> Subclasses { get; set; } = new();
    public List<string> AvailableSkills { get; set; } = new();
}
