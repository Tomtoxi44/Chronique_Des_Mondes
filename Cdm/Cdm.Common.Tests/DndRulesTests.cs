// -----------------------------------------------------------------------
// <copyright file="DndRulesTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Tests;

using Cdm.Common;
using Xunit;

/// <summary>
/// Unit tests for <see cref="DndRules"/>.
/// </summary>
public class DndRulesTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(4, 2)]
    [InlineData(5, 3)]
    [InlineData(8, 3)]
    [InlineData(9, 4)]
    [InlineData(12, 4)]
    [InlineData(13, 5)]
    [InlineData(16, 5)]
    [InlineData(17, 6)]
    [InlineData(20, 6)]
    public void ProficiencyBonus_MatchesTable(int level, int expected)
    {
        Assert.Equal(expected, DndRules.ProficiencyBonus(level));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(3, 13)]
    [InlineData(-1, 9)]
    public void UnarmoredArmorClass_IsTenPlusDex(int dexMod, int expected)
    {
        Assert.Equal(expected, DndRules.UnarmoredArmorClass(dexMod));
    }

    /// <summary>Level 1 fighter (d10) with +2 CON => 10 + 2 = 12.</summary>
    [Fact]
    public void AverageHitPoints_Level1_UsesFullHitDiePlusCon()
    {
        Assert.Equal(12, DndRules.AverageHitPoints(hitDie: 10, constitutionModifier: 2, level: 1));
    }

    /// <summary>Level 3 fighter (d10) with +2 CON => 12 (L1) + 2 * (6 + 2) = 12 + 16 = 28.</summary>
    [Fact]
    public void AverageHitPoints_HigherLevel_AddsAveragePerLevel()
    {
        Assert.Equal(28, DndRules.AverageHitPoints(hitDie: 10, constitutionModifier: 2, level: 3));
    }

    [Theory]
    [InlineData(0, 2, 1)]
    [InlineData(8, 0, 0)]
    public void AverageHitPoints_InvalidInputs_ReturnZero(int hitDie, int conMod, int level)
    {
        Assert.Equal(0, DndRules.AverageHitPoints(hitDie, conMod, level));
    }
}
