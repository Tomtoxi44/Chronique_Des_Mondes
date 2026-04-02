// -----------------------------------------------------------------------
// <copyright file="AchievementRarity.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Enums;

/// <summary>
/// Represents the rarity of an achievement.
/// </summary>
public enum AchievementRarity
{
    /// <summary>
    /// Common achievement (easy to obtain).
    /// </summary>
    Common = 0,

    /// <summary>
    /// Rare achievement (requires effort).
    /// </summary>
    Rare = 1,

    /// <summary>
    /// Epic achievement (notable accomplishment).
    /// </summary>
    Epic = 2,

    /// <summary>
    /// Legendary achievement (exceptional feat).
    /// </summary>
    Legendary = 3
}
