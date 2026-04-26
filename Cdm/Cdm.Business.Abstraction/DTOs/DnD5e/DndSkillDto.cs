// -----------------------------------------------------------------------
// <copyright file="DndSkillDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>D&amp;D 5e skill reference data.</summary>
public class DndSkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LinkedAbility { get; set; } = string.Empty;
    public string? Description { get; set; }
}
