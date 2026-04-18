using Cdm.Common.Enums;

namespace Cdm.Web.Models;

/// <summary>
/// Static helper providing display metadata for <see cref="GameType"/> enum values.
/// </summary>
public static class GameTypeInfo
{
    /// <summary>
    /// Returns a human-readable display name for the given game type.
    /// </summary>
    public static string GetDisplayName(GameType gameType) => gameType switch
    {
        GameType.Empty => "Non défini",
        GameType.Generic => "Générique",
        GameType.DnD5e => "Donjons & Dragons 5e",
        GameType.Pathfinder => "Pathfinder",
        GameType.CallOfCthulhu => "L'Appel de Cthulhu",
        GameType.Warhammer => "Warhammer",
        GameType.Cyberpunk => "Cyberpunk",
        GameType.Skyrim => "Skyrim",
        GameType.Custom => "Personnalisé",
        _ => "Inconnu"
    };

    /// <summary>
    /// Returns a CSS icon class (Bootstrap Icons) for the given game type.
    /// </summary>
    public static string GetIcon(GameType gameType) => gameType switch
    {
        GameType.Empty => "bi-question-circle",
        GameType.Generic => "bi-dice-5",
        GameType.DnD5e => "bi-shield-shaded",
        GameType.Pathfinder => "bi-compass",
        GameType.CallOfCthulhu => "bi-eye",
        GameType.Warhammer => "bi-hammer",
        GameType.Cyberpunk => "bi-cpu",
        GameType.Skyrim => "bi-snow2",
        GameType.Custom => "bi-gear",
        _ => "bi-controller"
    };

    /// <summary>
    /// Returns a CSS class for styling the game type badge.
    /// </summary>
    public static string GetCssClass(GameType gameType) => gameType switch
    {
        GameType.Empty => "game-type-empty",
        GameType.Generic => "game-type-generic",
        GameType.DnD5e => "game-type-dnd",
        GameType.Pathfinder => "game-type-pathfinder",
        GameType.CallOfCthulhu => "game-type-cthulhu",
        GameType.Warhammer => "game-type-warhammer",
        GameType.Cyberpunk => "game-type-cyberpunk",
        GameType.Skyrim => "game-type-skyrim",
        GameType.Custom => "game-type-custom",
        _ => "game-type-unknown"
    };

    /// <summary>
    /// Returns a short description of the game type.
    /// </summary>
    public static string GetDescription(GameType gameType) => gameType switch
    {
        GameType.Empty => "Aucun type de jeu sélectionné.",
        GameType.Generic => "Un système générique adaptable à tout univers.",
        GameType.DnD5e => "Le jeu de rôle médiéval-fantastique le plus populaire au monde.",
        GameType.Pathfinder => "Un système de règles détaillé dans un univers heroic fantasy.",
        GameType.CallOfCthulhu => "Horreur cosmique et enquêtes dans l'univers de Lovecraft.",
        GameType.Warhammer => "Batailles épiques dans un monde sombre et brutal.",
        GameType.Cyberpunk => "Aventures dystopiques dans un futur high-tech et low-life.",
        GameType.Skyrim => "Explorez les terres gelées de Bordeciel.",
        GameType.Custom => "Un système de jeu personnalisé par le maître de jeu.",
        _ => "Type de jeu inconnu."
    };
}
