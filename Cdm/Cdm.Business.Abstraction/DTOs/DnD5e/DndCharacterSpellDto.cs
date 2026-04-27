// -----------------------------------------------------------------------
// <copyright file="DndCharacterSpellDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>A spell known/prepared by a character (world-specific).</summary>
public class DndCharacterSpellDto
{
    public int Id { get; set; }
    public int WorldCharacterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? School { get; set; }
    public string? Description { get; set; }
    public string? DamageDice { get; set; }
    public string? DamageType { get; set; }
    public bool IsPrepared { get; set; }
    /// <summary>Reference to a DndSpell template, if selected from the list.</summary>
    public int? DndSpellId { get; set; }
}
