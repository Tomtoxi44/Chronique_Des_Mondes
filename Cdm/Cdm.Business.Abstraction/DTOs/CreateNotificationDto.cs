// -----------------------------------------------------------------------
// <copyright file="CreateNotificationDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for creating a notification (internal use by services).
/// </summary>
public class CreateNotificationDto
{
    /// <summary>
    /// Gets or sets the recipient user ID.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the notification type.
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Gets or sets the notification title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the related entity ID.
    /// </summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>
    /// Gets or sets the related entity type (e.g., "Campaign", "Achievement").
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Gets or sets the action URL (where to navigate when clicking).
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets the sender user ID (optional, for user-to-user notifications).
    /// </summary>
    public int? SentBy { get; set; }
}
