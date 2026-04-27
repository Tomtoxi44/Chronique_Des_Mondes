// -----------------------------------------------------------------------
// <copyright file="DndBackgroundDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs.DnD5e;

/// <summary>D&amp;D 5e background reference data.</summary>
public class DndBackgroundDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> SkillProficiencies { get; set; } = new();
    public List<string> ToolProficiencies { get; set; } = new();
    public string? Languages { get; set; }
    public string? Feature { get; set; }
    public string? FeatureDescription { get; set; }
}
