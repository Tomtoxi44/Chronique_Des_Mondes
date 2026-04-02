// -----------------------------------------------------------------------
// <copyright file="AchievementLevel.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Enums;

/// <summary>
/// Represents the level at which an achievement is defined.
/// </summary>
public enum AchievementLevel
{
    /// <summary>
    /// Achievement available for all players in the world.
    /// </summary>
    World = 0,

    /// <summary>
    /// Achievement specific to a campaign.
    /// </summary>
    Campaign = 1,

    /// <summary>
    /// Achievement specific to a chapter.
    /// </summary>
    Chapter = 2
}
