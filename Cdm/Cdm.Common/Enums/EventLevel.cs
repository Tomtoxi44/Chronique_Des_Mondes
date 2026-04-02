// -----------------------------------------------------------------------
// <copyright file="EventLevel.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Enums;

/// <summary>
/// Represents the level at which an event is applied.
/// </summary>
public enum EventLevel
{
    /// <summary>
    /// Event affects all characters in the world.
    /// </summary>
    World = 0,

    /// <summary>
    /// Event affects all characters in the campaign.
    /// </summary>
    Campaign = 1,

    /// <summary>
    /// Event affects characters only during this chapter.
    /// </summary>
    Chapter = 2
}
