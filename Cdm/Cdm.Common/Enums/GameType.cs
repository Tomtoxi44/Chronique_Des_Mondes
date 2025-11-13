namespace Cdm.Common.Enums;

/// <summary>
/// Represents the different types of games supported by the platform
/// </summary>
public enum GameType
{
    /// <summary>
    /// Generic game type with flexible attributes
    /// </summary>
    Generic = 0,

    /// <summary>
    /// Dungeons & Dragons 5th edition
    /// </summary>
    DnD5e = 1,

    /// <summary>
    /// Pathfinder role-playing game
    /// </summary>
    Pathfinder = 2,

    /// <summary>
    /// Call of Cthulhu horror role-playing game
    /// </summary>
    CallOfCthulhu = 3,

    /// <summary>
    /// Warhammer Fantasy Roleplay
    /// </summary>
    Warhammer = 4,

    /// <summary>
    /// The Elder Scrolls V: Skyrim
    /// </summary>
    Skyrim = 10,

    /// <summary>
    /// Custom game type defined by the game master
    /// </summary>
    Custom = 99
}
