// -----------------------------------------------------------------------
// <copyright file="SkyrimGameProfileDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// DTO for Skyrim game profile data (to be stored in GameSpecificData JSON).
/// </summary>
public class SkyrimGameProfileDto
{
    /// <summary>
    /// Gets or sets Health attribute.
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Gets or sets Magicka attribute.
    /// </summary>
    public int Magicka { get; set; }

    /// <summary>
    /// Gets or sets Stamina attribute.
    /// </summary>
    public int Stamina { get; set; }

    /// <summary>
    /// Gets or sets the character race (Nord, Altmer, Khajiit, etc.).
    /// </summary>
    public string? Race { get; set; }

    /// <summary>
    /// Gets or sets the primary skill tree focus.
    /// </summary>
    public string? PrimarySkill { get; set; }
}
