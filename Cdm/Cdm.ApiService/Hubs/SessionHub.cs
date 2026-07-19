// -----------------------------------------------------------------------
// <copyright file="SessionHub.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Hubs;

using Cdm.Business.Abstraction.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

/// <summary>
/// SignalR hub for real-time session communication.
/// Handles chat, dice rolls, character status updates, and trade proposals.
/// </summary>
[Authorize]
public class SessionHub : Hub
{
    private readonly ILogger<SessionHub> logger;
    private readonly AppDbContext db;
    private readonly ITradeService tradeService;
    private readonly IAchievementEvaluationService achievementEvaluation;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionHub"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="db">Database context.</param>
    /// <param name="tradeService">Trade service for object-exchange proposals.</param>
    /// <param name="achievementEvaluation">Evaluator that awards automatic achievements.</param>
    public SessionHub(ILogger<SessionHub> logger, AppDbContext db, ITradeService tradeService, IAchievementEvaluationService achievementEvaluation)
    {
        this.logger = logger;
        this.db = db;
        this.tradeService = tradeService;
        this.achievementEvaluation = achievementEvaluation;
    }

    /// <summary>
    /// Joins a session group.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task JoinSession(int sessionId)
    {
        var userId = this.GetUserId();
        var userName = this.GetUserName();
        var groupName = $"session_{sessionId}";

        // Authorization: only members of the related campaign may join the session group (audit fix #6).
        
        if (!await this.IsAuthorizedForSessionAsync(sessionId, userId))
        {
            this.logger.LogWarning(
                "User {UserId} denied access to session {SessionId}",
                userId,
                sessionId);
            throw new HubException("You are not authorized to join this session.");
        }

        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupName);

        this.logger.LogInformation(
            "User {UserId} ({UserName}) joined session {SessionId}",
            userId,
            userName,
            sessionId);

        // Notify others in the group
        await this.Clients.OthersInGroup(groupName).SendAsync(
            "UserJoined",
            new { UserId = userId, UserName = userName, Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Leaves a session group.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task LeaveSession(int sessionId)
    {
        var userId = this.GetUserId();
        var userName = this.GetUserName();
        var groupName = $"session_{sessionId}";

        await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupName);

        this.logger.LogInformation(
            "User {UserId} ({UserName}) left session {SessionId}",
            userId,
            userName,
            sessionId);

        // Notify others in the group
        await this.Clients.Group(groupName).SendAsync(
            "UserLeft",
            new { UserId = userId, UserName = userName, Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Sends a chat message to all users in a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="message">The message content.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task SendMessage(int sessionId, string message)
    {
        var userId = this.GetUserId();
        var userName = this.GetUserName();
        var groupName = $"session_{sessionId}";

        this.logger.LogInformation(
            "User {UserId} sent message in session {SessionId}",
            userId,
            sessionId);

        // Persist message to database
        var sessionMessage = new SessionMessage
        {
            SessionId = sessionId,
            UserId = userId,
            UserName = userName,
            Message = message,
            SentAt = DateTime.UtcNow
        };
        this.db.SessionMessages.Add(sessionMessage);
        await this.db.SaveChangesAsync();

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
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="diceType">The type of dice (e.g., "d20", "d6").</param>
    /// <param name="count">Number of dice rolled.</param>
    /// <param name="results">Array of individual die results.</param>
    /// <param name="modifier">Modifier applied to the roll.</param>
    /// <param name="reason">Reason for the roll (e.g., "Attack roll", "Perception check").</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task RollDice(int sessionId, string diceType, int count, int[] results, int modifier, string? reason)
    {
        var userId = this.GetUserId();
        var userName = this.GetUserName();
        var groupName = $"session_{sessionId}";
        var total = results.Sum() + modifier;

        this.logger.LogInformation(
            "User {UserId} rolled {Count}{DiceType} in session {SessionId}: {Total}",
            userId,
            count,
            diceType,
            sessionId,
            total);

        // Persist dice roll to database
        var diceRoll = new SessionDiceRoll
        {
            SessionId = sessionId,
            UserId = userId,
            UserName = userName,
            DiceType = diceType,
            Count = count,
            Results = string.Join(",", results),
            Modifier = modifier,
            Total = total,
            Reason = reason,
            RolledAt = DateTime.UtcNow
        };
        this.db.SessionDiceRolls.Add(diceRoll);
        await this.db.SaveChangesAsync();

        // Award any automatic achievement whose condition this roll satisfies (crit, fumble, dice count).
        await this.achievementEvaluation.OnDiceRolledAsync(userId, sessionId, diceType, results);

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
    /// Proposes a theory-based object trade to another session member (GM→player or player→player).
    /// The proposal is persisted and the recipient is notified; the pending list survives reconnection.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="targetUserId">The user ID being offered the trade.</param>
    /// <param name="offerDescription">Description of what's being offered.</param>
    /// <param name="requestDescription">Description of what's being requested.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task ProposeTrade(int sessionId, int targetUserId, string offerDescription, string requestDescription)
    {
        var userId = this.GetUserId();
        var groupName = $"session_{sessionId}";

        var trade = await this.tradeService.ProposeTradeAsync(sessionId, userId, targetUserId, offerDescription, requestDescription);
        if (trade == null)
        {
            throw new HubException("Impossible de proposer cet échange.");
        }

        this.logger.LogInformation(
            "User {UserId} proposed trade {TradeId} to {TargetUserId} in session {SessionId}",
            userId, trade.Id, targetUserId, sessionId);

        await this.Clients.Group(groupName).SendAsync("TradeProposed", trade);
    }

    /// <summary>
    /// Responds to a pending trade (accept or decline). Only the recipient may respond.
    /// </summary>
    /// <param name="sessionId">The session identifier (for group broadcast).</param>
    /// <param name="tradeId">The trade identifier.</param>
    /// <param name="accept"><c>true</c> to accept, <c>false</c> to decline.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task RespondToTrade(int sessionId, int tradeId, bool accept)
    {
        var userId = this.GetUserId();
        var groupName = $"session_{sessionId}";

        var trade = await this.tradeService.RespondToTradeAsync(tradeId, userId, accept);
        if (trade == null)
        {
            throw new HubException("Impossible de répondre à cet échange.");
        }

        await this.Clients.Group(groupName).SendAsync("TradeResolved", trade);
    }

    /// <summary>
    /// Cancels a pending trade. Only the proposer may cancel it.
    /// </summary>
    /// <param name="sessionId">The session identifier (for group broadcast).</param>
    /// <param name="tradeId">The trade identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task CancelTrade(int sessionId, int tradeId)
    {
        var userId = this.GetUserId();
        var groupName = $"session_{sessionId}";

        var trade = await this.tradeService.CancelTradeAsync(tradeId, userId);
        if (trade == null)
        {
            throw new HubException("Impossible d'annuler cet échange.");
        }

        await this.Clients.Group(groupName).SendAsync("TradeResolved", trade);
    }

    /// <summary>
    /// Updates a character's status (e.g., health, condition).
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="statusType">Type of status update (e.g., "health", "condition").</param>
    /// <param name="value">The new value or status description.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task UpdateCharacterStatus(int sessionId, int characterId, string statusType, string value)
    {
        var userId = this.GetUserId();
        var groupName = $"session_{sessionId}";

        this.logger.LogInformation(
            "User {UserId} updated character {CharacterId} status ({StatusType}) in session {SessionId}",
            userId,
            characterId,
            statusType,
            sessionId);

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
    /// Checks whether a user belongs to the campaign owning the given session,
    /// either as the Game Master or as a player participant.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns><c>true</c> if the user is authorized.</returns>
    private async Task<bool> IsAuthorizedForSessionAsync(int sessionId, int userId)
    {
        if (userId <= 0)
        {
            return false;
        }

        var session = await this.db.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session is null)
        {
            return false;
        }

        // Game Master of the campaign
        var isGameMaster = await this.db.Campaigns
            .AsNoTracking()
            .AnyAsync(c => c.Id == session.CampaignId && c.CreatedBy == userId);

        if (isGameMaster)
        {
            return true;
        }

        // Player participating in this session through a character they own
        return await this.db.SessionParticipants
            .AsNoTracking()
            .AnyAsync(sp => sp.SessionId == sessionId
                && sp.WorldCharacter.Character.UserId == userId);
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

    /// <summary>
    /// Gets the current user's display name from claims.
    /// Falls back through multiple claim types to handle different JWT mapping configurations.
    /// </summary>
    /// <returns>The user name, or "Unknown" if not found.</returns>
    private string GetUserName()
    {
        return this.Context.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? this.Context.User?.FindFirst("name")?.Value
            ?? this.Context.User?.FindFirst("unique_name")?.Value
            ?? "Unknown";
    }
}
