namespace Cdm.Common.Enums;

/// <summary>
/// Notification type enumeration
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Campaign invitation notification
    /// </summary>
    CampaignInvite = 0,

    /// <summary>
    /// Chapter session is starting soon
    /// </summary>
    SessionStarting = 1,

    /// <summary>
    /// Achievement unlocked notification
    /// </summary>
    AchievementUnlocked = 2,

    /// <summary>
    /// Trade proposal received
    /// </summary>
    TradeProposed = 3,

    /// <summary>
    /// Mentioned in a message/chat
    /// </summary>
    MessageMention = 4,

    /// <summary>
    /// Combat turn notification
    /// </summary>
    CombatTurn = 5,

    /// <summary>
    /// System announcement
    /// </summary>
    SystemAnnouncement = 6,

    /// <summary>
    /// Character status change
    /// </summary>
    CharacterUpdate = 7
}
