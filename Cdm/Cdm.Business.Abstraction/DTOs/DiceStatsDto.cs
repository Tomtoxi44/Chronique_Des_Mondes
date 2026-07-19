// -----------------------------------------------------------------------
// <copyright file="DiceStatsDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System.Collections.Generic;

/// <summary>
/// Aggregated dice-roll statistics for a user, built from persisted session rolls.
/// </summary>
public class DiceStatsDto
{
    /// <summary>Gets or sets the number of roll actions (rows), regardless of dice count.</summary>
    public int TotalRolls { get; set; }

    /// <summary>Gets or sets the total number of individual dice thrown.</summary>
    public int TotalDiceThrown { get; set; }

    /// <summary>Gets or sets the average value of an individual die face across all rolls (0 if none).</summary>
    public double OverallAverage { get; set; }

    /// <summary>Gets or sets the number of d20 dice thrown.</summary>
    public int D20Count { get; set; }

    /// <summary>Gets or sets the number of natural 20s rolled on d20.</summary>
    public int Natural20Count { get; set; }

    /// <summary>Gets or sets the number of natural 1s rolled on d20.</summary>
    public int Natural1Count { get; set; }

    /// <summary>Gets the critical-hit rate on d20 (natural 20s / d20 thrown), as a percentage.</summary>
    public double CritRate => D20Count > 0 ? (double)Natural20Count / D20Count * 100 : 0;

    /// <summary>Gets the fumble rate on d20 (natural 1s / d20 thrown), as a percentage.</summary>
    public double FumbleRate => D20Count > 0 ? (double)Natural1Count / D20Count * 100 : 0;

    /// <summary>Gets or sets the number of distinct sessions the user rolled in (participation).</summary>
    public int SessionsWithRolls { get; set; }

    /// <summary>Gets or sets per-dice-type breakdown.</summary>
    public List<DiceTypeStatDto> PerDiceType { get; set; } = new();
}

/// <summary>
/// Per-dice-type roll statistics.
/// </summary>
public class DiceTypeStatDto
{
    /// <summary>Gets or sets the dice type (e.g. "D20").</summary>
    public string DiceType { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of dice of this type thrown.</summary>
    public int DiceThrown { get; set; }

    /// <summary>Gets or sets the average individual result for this dice type.</summary>
    public double Average { get; set; }
}
