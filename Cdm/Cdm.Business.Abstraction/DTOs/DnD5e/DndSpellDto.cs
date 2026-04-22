// -----------------------------------------------------------------------
// <copyright file="DndSpellDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>D&amp;D 5e spell reference data.</summary>
public class DndSpellDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? School { get; set; }
    public string? Description { get; set; }
    public string? CastingTime { get; set; }
    public string? Range { get; set; }
    public string? Duration { get; set; }
    public string? DamageDice { get; set; }
    public string? DamageType { get; set; }
    public List<string> Classes { get; set; } = new();
    public string? Components { get; set; }
    public bool RequiresConcentration { get; set; }
    public bool IsRitual { get; set; }
}
