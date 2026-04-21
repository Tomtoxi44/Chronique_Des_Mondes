// -----------------------------------------------------------------------
// <copyright file="SessionMessage.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Represents a chat message sent during a session.
/// </summary>
public class SessionMessage
{
    /// <summary>Gets or sets the message ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the chapter ID this message belongs to.</summary>
    public int ChapterId { get; set; }

    /// <summary>Gets or sets the user ID of the sender.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the display name of the sender at the time of sending.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the message content.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets when this message was sent.</summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the user navigation property.</summary>
    public virtual User User { get; set; } = null!;

    /// <summary>Gets or sets the chapter navigation property.</summary>
    public virtual Chapter Chapter { get; set; } = null!;
}
