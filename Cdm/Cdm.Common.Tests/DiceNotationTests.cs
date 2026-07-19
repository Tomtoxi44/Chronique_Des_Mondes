// -----------------------------------------------------------------------
// <copyright file="DiceNotationTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Tests;

using Cdm.Common;
using Xunit;

/// <summary>
/// Unit tests for <see cref="DiceNotation"/>.
/// </summary>
public class DiceNotationTests
{
    [Theory]
    [InlineData("1d8", 1, 8, 0)]
    [InlineData("2d6", 2, 6, 0)]
    [InlineData("2d6+3", 2, 6, 3)]
    [InlineData("1d10-1", 1, 10, -1)]
    [InlineData(" 3 D 4 + 2 ", 3, 4, 2)]
    public void TryParse_ValidNotation_ReturnsExpression(string notation, int count, int faces, int bonus)
    {
        var ok = DiceNotation.TryParse(notation, out var expr);

        Assert.True(ok);
        Assert.Equal(count, expr.Count);
        Assert.Equal(faces, expr.Faces);
        Assert.Equal(bonus, expr.FlatBonus);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("d20")]
    [InlineData("0d6")]
    [InlineData("1d0")]
    [InlineData("1d6+")]
    public void TryParse_InvalidNotation_ReturnsFalse(string? notation)
    {
        var ok = DiceNotation.TryParse(notation, out _);
        Assert.False(ok);
    }
}
