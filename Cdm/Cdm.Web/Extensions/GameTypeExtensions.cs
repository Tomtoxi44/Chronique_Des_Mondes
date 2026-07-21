// -----------------------------------------------------------------------
// <copyright file="GameTypeExtensions.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Extensions;

using System.Collections.Generic;
using System.Linq;
using Cdm.Common.Enums;

/// <summary>
/// Single source for the compact game-type labels used in filters, badges and pickers
/// (replaces the identical tuple arrays / label switches duplicated across components).
/// For long display names/icons/descriptions, see <see cref="Cdm.Web.Models.GameTypeInfo"/>.
/// </summary>
public static class GameTypeExtensions
{
    /// <summary>The user-selectable game types with their compact labels, in display order.</summary>
    public static readonly IReadOnlyList<(GameType Value, string Label)> Selectable = new[]
    {
        (GameType.Generic, "Générique"),
        (GameType.DnD5e, "D&D 5e"),
        (GameType.Pathfinder, "Pathfinder"),
        (GameType.CallOfCthulhu, "Cthulhu"),
        (GameType.Warhammer, "Warhammer"),
        (GameType.Cyberpunk, "Cyberpunk"),
        (GameType.Skyrim, "Skyrim"),
        (GameType.Custom, "Personnalisé"),
    };

    /// <summary>Compact label for a game type (e.g. "D&amp;D 5e").</summary>
    public static string ToShortName(this GameType type) =>
        Selectable.FirstOrDefault(x => x.Value == type).Label ?? type.ToString();
}
