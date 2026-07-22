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

    /// <summary>
    /// The game types offered by the world creation/settings picker, with their full
    /// labels and icons (single source for the "game-type-grid" cards).
    /// </summary>
    public static readonly IReadOnlyList<(GameType Value, string Label, string Icon)> Picker = new[]
    {
        (GameType.Generic,       "Générique",          "bi-globe2"),
        (GameType.DnD5e,         "D&D 5e",             "bi-shield-fill"),
        (GameType.Pathfinder,    "Pathfinder",         "bi-shield-fill-check"),
        (GameType.CallOfCthulhu, "L'Appel de Cthulhu", "bi-eye-fill"),
        (GameType.Warhammer,     "Warhammer",          "bi-hammer"),
        (GameType.Cyberpunk,     "Cyberpunk",          "bi-cpu-fill"),
        (GameType.Skyrim,        "Skyrim",             "bi-snow2"),
        (GameType.Custom,        "Personnalisé",       "bi-stars"),
    };

    /// <summary>Full picker label for a game type (e.g. "L'Appel de Cthulhu").</summary>
    public static string ToPickerLabel(this GameType type) =>
        Picker.FirstOrDefault(x => x.Value == type).Label ?? type.ToString();
}
