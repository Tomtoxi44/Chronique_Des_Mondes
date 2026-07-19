// -----------------------------------------------------------------------
// <copyright file="DiceNotation.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common;

using System.Globalization;
using System.Text.RegularExpressions;

/// <summary>
/// Parses standard dice-damage notation such as <c>"1d8"</c>, <c>"2d6+3"</c> or <c>"1d10-1"</c>.
/// </summary>
public static partial class DiceNotation
{
    /// <summary>
    /// A parsed dice expression.
    /// </summary>
    /// <param name="Count">Number of dice.</param>
    /// <param name="Faces">Number of faces per die.</param>
    /// <param name="FlatBonus">Flat modifier baked into the notation (may be negative or zero).</param>
    public readonly record struct DiceExpression(int Count, int Faces, int FlatBonus);

    [GeneratedRegex(@"^\s*(\d+)\s*[dD]\s*(\d+)\s*([+-]\s*\d+)?\s*$")]
    private static partial Regex DicePattern();

    /// <summary>
    /// Attempts to parse a dice-notation string (e.g. <c>"2d6+3"</c>).
    /// </summary>
    /// <param name="notation">The notation to parse.</param>
    /// <param name="expression">The parsed expression when successful.</param>
    /// <returns><c>true</c> if the notation was valid.</returns>
    public static bool TryParse(string? notation, out DiceExpression expression)
    {
        expression = default;
        if (string.IsNullOrWhiteSpace(notation))
        {
            return false;
        }

        var match = DicePattern().Match(notation);
        if (!match.Success)
        {
            return false;
        }

        var count = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var faces = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        if (count <= 0 || faces <= 0)
        {
            return false;
        }

        var flatBonus = 0;
        if (match.Groups[3].Success)
        {
            var raw = match.Groups[3].Value.Replace(" ", string.Empty);
            flatBonus = int.Parse(raw, CultureInfo.InvariantCulture);
        }

        expression = new DiceExpression(count, faces, flatBonus);
        return true;
    }
}
