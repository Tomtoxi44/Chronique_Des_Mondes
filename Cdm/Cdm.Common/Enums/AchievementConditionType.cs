namespace Cdm.Common.Enums;

/// <summary>
/// The set of conditions that can automatically unlock an achievement.
/// Stored on <c>Achievement.AutomaticCondition</c> as a code, optionally suffixed
/// with a threshold (e.g. <c>"SessionsAttended:5"</c>). Conditions without a natural
/// threshold (a critical hit) ignore the suffix.
/// </summary>
public enum AchievementConditionType
{
    /// <summary>Rolled a natural 20 on a d20 during a session.</summary>
    DiceCritical = 0,

    /// <summary>Rolled a natural 1 on a d20 during a session.</summary>
    DiceFumble = 1,

    /// <summary>Rolled at least N dice in total across all sessions (threshold).</summary>
    DiceRolls = 2,

    /// <summary>Participated in at least N sessions (threshold; N = 1 means first session).</summary>
    SessionsAttended = 3,

    /// <summary>Won at least N combats (threshold).</summary>
    CombatWon = 4,

    /// <summary>Survived at least N combats (threshold).</summary>
    CombatSurvived = 5,

    /// <summary>Completed at least N accepted object trades (threshold).</summary>
    TradesCompleted = 6
}
