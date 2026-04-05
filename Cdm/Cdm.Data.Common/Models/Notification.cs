// -----------------------------------------------------------------------
// <copyright file="Notification.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cdm.Common.Enums;

/// <summary>
/// Represents a user notification
/// </summary>
[Table("Notifications")]
public class Notification
{
    /// <summary>
    /// Gets or sets the notification unique identifier
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the recipient user ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the notification type
    /// </summary>
    [Required]
    public NotificationType Type { get; set; }

    /// <summary>
    /// Gets or sets the notification title
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification message/content
    /// </summary>
    [Required]
    [MaxLength(1000)]
    [Column(TypeName = "nvarchar(max)")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the related entity ID (e.g., campaign ID, achievement ID)
    /// </summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>
    /// Gets or sets the related entity type (e.g., "Campaign", "Achievement", "Chapter")
    /// </summary>
    [MaxLength(50)]
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Gets or sets the action URL (where to navigate when clicking)
    /// </summary>
    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets whether the notification has been read
    /// </summary>
    [Required]
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Gets or sets when the notification was read
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Gets or sets the notification creation timestamp (UTC)
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the sender user ID (optional, for user-to-user notifications)
    /// </summary>
    public int? SentBy { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the recipient user
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation property to the sender (if applicable)
    /// </summary>
    [ForeignKey(nameof(SentBy))]
    public virtual User? Sender { get; set; }
}
