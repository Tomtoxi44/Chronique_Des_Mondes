// -----------------------------------------------------------------------
// <copyright file="DndRules.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common;

/// <summary>
/// Shared, pure D&amp;D 5e rule calculations, so the same formulas are used across the
/// character wizard, the character sheet and the API services (no duplication/drift).
/// </summary>
public static class DndRules
{
    /// <summary>
    /// Proficiency bonus for a given character level (D&amp;D 5e: +2 at levels 1-4, up to +6 at 17-20).
    /// </summary>
    /// <param name="level">The character level.</param>
    /// <returns>The proficiency bonus.</returns>
    public static int ProficiencyBonus(int level) => level switch
    {
        <= 4 => 2,
        <= 8 => 3,
        <= 12 => 4,
        <= 16 => 5,
        _ => 6
    };

    /// <summary>
    /// Unarmored armor class: 10 + Dexterity modifier.
    /// </summary>
    /// <param name="dexterityModifier">The Dexterity modifier.</param>
    /// <returns>The base armor class without armor.</returns>
    public static int UnarmoredArmorClass(int dexterityModifier) => 10 + dexterityModifier;

    /// <summary>
    /// Average maximum hit points: the full hit die at level 1, then the average roll
    /// (<c>hitDie / 2 + 1</c>) for each subsequent level, plus the Constitution modifier per level.
    /// </summary>
    /// <param name="hitDie">The class hit die (e.g. 8 for a d8).</param>
    /// <param name="constitutionModifier">The Constitution modifier.</param>
    /// <param name="level">The character level.</param>
    /// <returns>The suggested maximum hit points (at least 1).</returns>
    public static int AverageHitPoints(int hitDie, int constitutionModifier, int level)
    {
        if (hitDie <= 0 || level <= 0)
        {
            return 0;
        }

        var perLevelAverage = (hitDie / 2) + 1;
        var hitPoints = hitDie + constitutionModifier; // level 1 gets the max hit die
        if (level > 1)
        {
            hitPoints += (level - 1) * (perLevelAverage + constitutionModifier);
        }

        return Math.Max(1, hitPoints);
    }
}
