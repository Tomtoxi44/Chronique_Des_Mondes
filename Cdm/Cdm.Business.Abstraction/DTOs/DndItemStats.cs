// -----------------------------------------------------------------------
// <copyright file="DndItemStats.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// D&amp;D 5e combat stats for an item (weapon), stored as JSON in a codex item's
/// <c>GameSpecificData</c> and copied into the dedicated inventory columns when added to a character.
/// Serialize/deserialize with camelCase property naming.
/// </summary>
public class DndItemStats
{
    /// <summary>Gets or sets the damage dice (e.g. "1d8").</summary>
    public string? DamageDice { get; set; }

    /// <summary>Gets or sets the damage type (e.g. "tranchant").</summary>
    public string? DamageType { get; set; }

    /// <summary>Gets or sets the attack bonus.</summary>
    public int? AttackBonus { get; set; }
}
