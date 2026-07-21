// -----------------------------------------------------------------------
// <copyright file="ParticipationStatsDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System.Collections.Generic;

/// <summary>
/// Aggregated participation statistics for a user: how much they play,
/// as GM vs player, and their monthly activity.
/// </summary>
public class ParticipationStatsDto
{
    /// <summary>Gets or sets the total number of distinct sessions the user took part in (as GM or player).</summary>
    public int TotalSessions { get; set; }

    /// <summary>Gets or sets the number of sessions the user ran as game master.</summary>
    public int SessionsAsGm { get; set; }

    /// <summary>Gets or sets the number of sessions the user played in (excluding those they ran).</summary>
    public int SessionsAsPlayer { get; set; }

    /// <summary>Gets or sets the total hours played, summed over ended sessions the user took part in.</summary>
    public double TotalHoursPlayed { get; set; }

    /// <summary>Gets or sets the average group size (participant count) across the user's sessions.</summary>
    public double AverageGroupSize { get; set; }

    /// <summary>Gets or sets the number of distinct campaigns the user took part in.</summary>
    public int CampaignsPlayed { get; set; }

    /// <summary>Gets or sets the monthly activity breakdown (months with activity, chronological).</summary>
    public List<MonthlyActivityDto> ByMonth { get; set; } = new();
}

/// <summary>
/// Activity for a single calendar month.
/// </summary>
public class MonthlyActivityDto
{
    /// <summary>Gets or sets the calendar year.</summary>
    public int Year { get; set; }

    /// <summary>Gets or sets the calendar month (1-12).</summary>
    public int Month { get; set; }

    /// <summary>Gets or sets the number of sessions in this month.</summary>
    public int Sessions { get; set; }

    /// <summary>Gets or sets the hours played in this month (ended sessions).</summary>
    public double Hours { get; set; }
}
