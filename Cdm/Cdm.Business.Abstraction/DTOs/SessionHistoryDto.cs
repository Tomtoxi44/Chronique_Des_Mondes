// -----------------------------------------------------------------------
// <copyright file="SessionHistoryDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System;
using System.Collections.Generic;

/// <summary>
/// DTO carrying the persisted chat and dice history of a session,
/// so a client reopening a session can rebuild its timeline.
/// </summary>
public class SessionHistoryDto
{
    /// <summary>Gets or sets the chat messages, ordered chronologically.</summary>
    public List<SessionMessageDto> Messages { get; set; } = [];

    /// <summary>Gets or sets the dice rolls, ordered chronologically.</summary>
    public List<SessionDiceRollDto> DiceRolls { get; set; } = [];
}

/// <summary>
/// DTO representing a persisted chat message.
/// </summary>
public class SessionMessageDto
{
    /// <summary>Gets or sets the sender user ID.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the sender display name at the time of sending.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the message content.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets when the message was sent (UTC).</summary>
    public DateTime SentAt { get; set; }
}

/// <summary>
/// DTO representing a persisted dice roll.
/// </summary>
public class SessionDiceRollDto
{
    /// <summary>Gets or sets the roller user ID.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the roller display name at the time of rolling.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the dice type (e.g. "D20").</summary>
    public string DiceType { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of dice rolled.</summary>
    public int Count { get; set; }

    /// <summary>Gets or sets the individual die results.</summary>
    public int[] Results { get; set; } = [];

    /// <summary>Gets or sets the modifier applied to the roll.</summary>
    public int Modifier { get; set; }

    /// <summary>Gets or sets the total (sum of results + modifier).</summary>
    public int Total { get; set; }

    /// <summary>Gets or sets the optional reason for the roll.</summary>
    public string? Reason { get; set; }

    /// <summary>Gets or sets when the roll occurred (UTC).</summary>
    public DateTime RolledAt { get; set; }
}
