using Cdm.Common.Enums;
using System;

namespace Cdm.Web.Services
{
    public class ThemeStateService
    {
        public event Action? OnThemeChanged;

        private GameType _currentGameType = GameType.Generic;

        public GameType CurrentGameType
        {
            get => _currentGameType;
            set
            {
                if (_currentGameType != value)
                {
                    _currentGameType = value;
                    OnThemeChanged?.Invoke();
                }
            }
        }

        // Associe chaque type de jeu de rôle à une couleur hexadécimale dynamique
        public string GetThemeColor()
        {
            return _currentGameType switch
            {
                GameType.Cyberpunk => "#fcee0a",     // Neon Yellow
                GameType.DnD5e => "#8b0000",         // Crimson Red / Dark Red
                GameType.Pathfinder => "#004080",    // Ocean Blue
                GameType.CallOfCthulhu => "#003B28", // Eldritch Dark Green
                GameType.Warhammer => "#4b0082",     // Deep Indigo
                GameType.Skyrim => "#a0a0a0",        // Steel Silver
                _ => "#4f46e5",                      // Default Indigo (Generic)
            };
        }
    }
}
