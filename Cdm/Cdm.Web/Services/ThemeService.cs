using Cdm.Common.Enums;

namespace Cdm.Web.Services;

/// <summary>
/// Configuration for a game-type-specific theme
/// </summary>
public record GameThemeConfig(
    string Name,
    string AccentColor,
    string SecondaryColor,
    string HeadingFont,
    string Description
);

/// <summary>
/// Centralized theme engine that maps GameType to visual theme configuration.
/// Drives FluentDesignTheme CustomColor binding and exposes theme state to the entire app.
/// </summary>
public class ThemeService
{
    private static readonly Dictionary<GameType, GameThemeConfig> Themes = new()
    {
        [GameType.Empty] = new("Générique", "#4f46e5", "#7c3aed", "'Cinzel', serif", "Thème par défaut"),
        [GameType.Generic] = new("Générique", "#4f46e5", "#7c3aed", "'Cinzel', serif", "Thème par défaut"),
        [GameType.DnD5e] = new("Donjons & Dragons", "#b8860b", "#8b0000", "'Cinzel', serif", "Fantasy épique et noble"),
        [GameType.Pathfinder] = new("Pathfinder", "#cd7f32", "#2d5a27", "'Cinzel', serif", "Aventure et exploration"),
        [GameType.CallOfCthulhu] = new("L'Appel de Cthulhu", "#2d5a3d", "#1a1a2e", "'Cinzel', serif", "Horreur lovecraftienne"),
        [GameType.Warhammer] = new("Warhammer", "#8b0000", "#2d2d2d", "'Cinzel', serif", "Grimdark brutal"),
        [GameType.Cyberpunk] = new("Cyberpunk", "#00e5ff", "#e040fb", "'Inter', sans-serif", "Néon et technologie"),
        [GameType.Skyrim] = new("Skyrim", "#5b8fa8", "#8b8680", "'Cinzel', serif", "Ambiance nordique sobre"),
        [GameType.Custom] = new("Personnalisé", "#4f46e5", "#7c3aed", "'Cinzel', serif", "Thème personnalisable"),
    };

    private GameType _currentGameType = GameType.Generic;
    private bool _isDarkMode = true;

    public event Action? OnThemeChanged;

    public GameType CurrentGameType => _currentGameType;
    public bool IsDarkMode => _isDarkMode;
    public GameThemeConfig CurrentTheme => GetTheme(_currentGameType);

    public static GameThemeConfig GetTheme(GameType gameType)
    {
        return Themes.GetValueOrDefault(gameType, Themes[GameType.Generic]);
    }

    public static IReadOnlyDictionary<GameType, GameThemeConfig> AllThemes => Themes;

    public void SetGameType(GameType gameType)
    {
        if (_currentGameType == gameType) return;
        _currentGameType = gameType;
        OnThemeChanged?.Invoke();
    }

    public void ResetToDefault()
    {
        SetGameType(GameType.Generic);
    }

    public void SetDarkMode(bool isDark)
    {
        if (_isDarkMode == isDark) return;
        _isDarkMode = isDark;
        OnThemeChanged?.Invoke();
    }

    public void ToggleDarkMode()
    {
        SetDarkMode(!_isDarkMode);
    }

    public static string GetGameTypeDisplayName(GameType gameType) => gameType switch
    {
        GameType.Empty => "Aucun",
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

    public static string GetGameTypeIcon(GameType gameType) => gameType switch
    {
        GameType.DnD5e => "bi-shield-fill",
        GameType.Pathfinder => "bi-map-fill",
        GameType.CallOfCthulhu => "bi-eye-fill",
        GameType.Warhammer => "bi-hammer",
        GameType.Cyberpunk => "bi-lightning-charge-fill",
        GameType.Skyrim => "bi-snow",
        GameType.Custom => "bi-dice-6-fill",
        _ => "bi-globe2"
    };
}
