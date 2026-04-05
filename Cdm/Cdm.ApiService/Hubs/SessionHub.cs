// -----------------------------------------------------------------------
// <copyright file="SessionHub.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

/// <summary>
/// SignalR hub for real-time chapter session communication.
/// Handles chat, dice rolls, character status updates, and trade proposals.
/// </summary>
[Authorize]
public class SessionHub : Hub
{
    private readonly ILogger<SessionHub> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionHub"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public SessionHub(ILogger<SessionHub> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Joins a chapter session group.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task JoinSession(int chapterId)
    {
        var userId = this.GetUserId();
        var userName = this.Context.User?.Identity?.Name ?? "Unknown";
        var groupName = $"chapter_{chapterId}";

        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupName);

        this.logger.LogInformation(
            "User {UserId} ({UserName}) joined session {ChapterId}",
            userId,
            userName,
            chapterId);

        // Notify others in the group
        await this.Clients.OthersInGroup(groupName).SendAsync(
            "UserJoined",
            new { UserId = userId, UserName = userName, Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Leaves a chapter session group.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task LeaveSession(int chapterId)
    {
        var userId = this.GetUserId();
        var userName = this.Context.User?.Identity?.Name ?? "Unknown";
        var groupName = $"chapter_{chapterId}";

        await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupName);

        this.logger.LogInformation(
            "User {UserId} ({UserName}) left session {ChapterId}",
            userId,
            userName,
            chapterId);

        // Notify others in the group
        await this.Clients.Group(groupName).SendAsync(
            "UserLeft",
            new { UserId = userId, UserName = userName, Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Sends a chat message to all users in a session.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="message">The message content.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task SendMessage(int chapterId, string message)
    {
        var userId = this.GetUserId();
        var userName = this.Context.User?.Identity?.Name ?? "Unknown";
        var groupName = $"chapter_{chapterId}";

        this.logger.LogInformation(
            "User {UserId} sent message in session {ChapterId}",
            userId,
            chapterId);

        await this.Clients.Group(groupName).SendAsync(
            "ReceiveMessage",
            new
            {
                UserId = userId,
                UserName = userName,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Broadcasts a dice roll result to the session.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="diceType">The type of dice (e.g., "d20", "d6").</param>
    /// <param name="count">Number of dice rolled.</param>
    /// <param name="results">Array of individual die results.</param>
    /// <param name="modifier">Modifier applied to the roll.</param>
    /// <param name="reason">Reason for the roll (e.g., "Attack roll", "Perception check").</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task RollDice(int chapterId, string diceType, int count, int[] results, int modifier, string? reason)
    {
        var userId = this.GetUserId();
        var userName = this.Context.User?.Identity?.Name ?? "Unknown";
        var groupName = $"chapter_{chapterId}";
        var total = results.Sum() + modifier;

        this.logger.LogInformation(
            "User {UserId} rolled {Count}{DiceType} in session {ChapterId}: {Total}",
            userId,
            count,
            diceType,
            chapterId,
            total);

        await this.Clients.Group(groupName).SendAsync(
            "DiceRolled",
            new
            {
                UserId = userId,
                UserName = userName,
                DiceType = diceType,
                Count = count,
                Results = results,
                Modifier = modifier,
                Total = total,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Proposes a theoretical trade between characters (theory-based RPG mechanic).
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="targetUserId">The user ID being offered the trade.</param>
    /// <param name="offerDescription">Description of what's being offered.</param>
    /// <param name="requestDescription">Description of what's being requested.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task ProposeTradeTheory(int chapterId, int targetUserId, string offerDescription, string requestDescription)
    {
        var userId = this.GetUserId();
        var userName = this.Context.User?.Identity?.Name ?? "Unknown";
        var groupName = $"chapter_{chapterId}";

        this.logger.LogInformation(
            "User {UserId} proposed trade to {TargetUserId} in session {ChapterId}",
            userId,
            targetUserId,
            chapterId);

        await this.Clients.Group(groupName).SendAsync(
            "TradeProposed",
            new
            {
                FromUserId = userId,
                FromUserName = userName,
                ToUserId = targetUserId,
                Offer = offerDescription,
                Request = requestDescription,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Updates a character's status (e.g., health, condition).
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="statusType">Type of status update (e.g., "health", "condition").</param>
    /// <param name="value">The new value or status description.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task UpdateCharacterStatus(int chapterId, int characterId, string statusType, string value)
    {
        var userId = this.GetUserId();
        var groupName = $"chapter_{chapterId}";

        this.logger.LogInformation(
            "User {UserId} updated character {CharacterId} status ({StatusType}) in session {ChapterId}",
            userId,
            characterId,
            statusType,
            chapterId);

        await this.Clients.Group(groupName).SendAsync(
            "CharacterStatusUpdated",
            new
            {
                CharacterId = characterId,
                StatusType = statusType,
                Value = value,
                UpdatedBy = userId,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Called when a client disconnects.
    /// </summary>
    /// <param name="exception">Exception if disconnection was caused by error.</param>
    /// <returns>A task representing the async operation.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = this.GetUserId();
        
        if (exception != null)
        {
            this.logger.LogWarning(
                exception,
                "User {UserId} disconnected with error",
                userId);
        }
        else
        {
            this.logger.LogInformation("User {UserId} disconnected", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Gets the current user ID from claims.
    /// </summary>
    /// <returns>The user ID.</returns>
    private int GetUserId()
    {
        var userIdClaim = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}
