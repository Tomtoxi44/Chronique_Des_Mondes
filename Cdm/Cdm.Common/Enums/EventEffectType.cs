// -----------------------------------------------------------------------
// <copyright file="EventEffectType.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Enums;

/// <summary>
/// Represents the type of effect an event has.
/// </summary>
public enum EventEffectType
{
    /// <summary>
    /// Modifies a character statistic (e.g., Strength, Dexterity).
    /// </summary>
    StatModifier = 0,

    /// <summary>
    /// Modifies health points (current or maximum).
    /// </summary>
    HealthModifier = 1,

    /// <summary>
    /// Modifies dice rolls (attack, save, initiative).
    /// </summary>
    DiceModifier = 2,

    /// <summary>
    /// Narrative effect only (no mechanical impact).
    /// </summary>
    Narrative = 3
}
