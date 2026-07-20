// -----------------------------------------------------------------------
// <copyright file="DndNpcStats.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Components.Pages.Worlds;

/// <summary>D&amp;D 5e stat block for a Non-Player Character.</summary>
public class DndNpcStats
{
    public string? CreatureType { get; set; }
    public string? Race { get; set; }
    public string? ClassName { get; set; }
    public string? ChallengeRating { get; set; }
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;
    public int MaxHitPoints { get; set; } = 10;
    public int ArmorClass { get; set; } = 10;
    public int Speed { get; set; } = 9;

    public static int Modifier(int score) => (score - 10) / 2;

    public static string ModStr(int score)
    {
        var m = Modifier(score);
        return m >= 0 ? $"+{m}" : $"{m}";
    }
}
