// -----------------------------------------------------------------------
// <copyright file="AchievementCondition.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Common.Enums;

/// <summary>
/// Parsed representation of an achievement's automatic condition, stored on
/// <c>Achievement.AutomaticCondition</c> as <c>"Type"</c> or <c>"Type:Threshold"</c>.
/// </summary>
public readonly record struct AchievementCondition(AchievementConditionType Type, int Threshold)
{
    /// <summary>
    /// Parses a stored condition code (e.g. <c>"SessionsAttended:5"</c> or <c>"DiceCritical"</c>).
    /// A missing or invalid threshold defaults to 1.
    /// </summary>
    /// <param name="raw">The stored condition string.</param>
    /// <param name="condition">The parsed condition when successful.</param>
    /// <returns><c>true</c> if the code was recognised.</returns>
    public static bool TryParse(string? raw, out AchievementCondition condition)
    {
        condition = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parts = raw.Split(':', 2);
        if (!Enum.TryParse<AchievementConditionType>(parts[0].Trim(), ignoreCase: true, out var type))
        {
            return false;
        }

        var threshold = 1;
        if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out var parsed) && parsed > 0)
        {
            threshold = parsed;
        }

        condition = new AchievementCondition(type, threshold);
        return true;
    }
}
