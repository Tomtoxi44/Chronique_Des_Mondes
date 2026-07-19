// -----------------------------------------------------------------------
// <copyright file="TradeService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing theory-based object trades within a session.
/// </summary>
public class TradeService(
    AppDbContext dbContext,
    INotificationService notificationService,
    IAchievementEvaluationService achievementEvaluation,
    ILogger<TradeService> logger) : ITradeService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly INotificationService notificationService = notificationService;
    private readonly IAchievementEvaluationService achievementEvaluation = achievementEvaluation;
    private readonly ILogger<TradeService> logger = logger;

    /// <inheritdoc/>
    public async Task<SessionTradeDto?> ProposeTradeAsync(int sessionId, int fromUserId, int toUserId, string offerDescription, string requestDescription)
    {
        try
        {
            if (fromUserId == toUserId)
            {
                return null;
            }

            var session = await this.dbContext.Sessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null)
            {
                return null;
            }

            // Both the proposer and the recipient must belong to the session.
            if (!await this.IsMemberAsync(session, fromUserId) || !await this.IsMemberAsync(session, toUserId))
            {
                this.logger.LogWarning(
                    "Trade proposal rejected: user {FromUserId} or {ToUserId} is not a member of session {SessionId}",
                    fromUserId, toUserId, sessionId);
                return null;
            }

            var fromName = await this.GetUserNameAsync(fromUserId);
            var toName = await this.GetUserNameAsync(toUserId);

            var trade = new SessionTrade
            {
                SessionId = sessionId,
                FromUserId = fromUserId,
                FromUserName = fromName,
                ToUserId = toUserId,
                ToUserName = toName,
                OfferDescription = offerDescription?.Trim() ?? string.Empty,
                RequestDescription = requestDescription?.Trim() ?? string.Empty,
                Status = TradeStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            this.dbContext.SessionTrades.Add(trade);
            await this.dbContext.SaveChangesAsync();

            await this.notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = toUserId,
                Type = NotificationType.TradeProposed,
                Title = "Proposition d'échange",
                Message = $"{fromName} vous propose un échange.",
                RelatedEntityId = trade.Id,
                RelatedEntityType = "SessionTrade",
                ActionUrl = $"/sessions/{sessionId}/player",
                SentBy = fromUserId
            });

            this.logger.LogInformation(
                "Trade {TradeId} proposed by {FromUserId} to {ToUserId} in session {SessionId}",
                trade.Id, fromUserId, toUserId, sessionId);

            return MapToDto(trade);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error proposing trade in session {SessionId}", sessionId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SessionTradeDto?> RespondToTradeAsync(int tradeId, int userId, bool accept)
    {
        try
        {
            var trade = await this.dbContext.SessionTrades.FirstOrDefaultAsync(t => t.Id == tradeId);
            if (trade == null || trade.Status != TradeStatus.Pending)
            {
                return null;
            }

            // Only the recipient may answer.
            if (trade.ToUserId != userId)
            {
                this.logger.LogWarning("User {UserId} is not the recipient of trade {TradeId}", userId, tradeId);
                return null;
            }

            trade.Status = accept ? TradeStatus.Accepted : TradeStatus.Declined;
            trade.RespondedAt = DateTime.UtcNow;
            await this.dbContext.SaveChangesAsync();

            // An accepted trade may unlock trade-based automatic achievements for both parties.
            if (accept)
            {
                await this.achievementEvaluation.OnTradeAcceptedAsync(trade.FromUserId, trade.SessionId);
                await this.achievementEvaluation.OnTradeAcceptedAsync(trade.ToUserId, trade.SessionId);
            }

            await this.notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = trade.FromUserId,
                Type = NotificationType.TradeProposed,
                Title = accept ? "Échange accepté" : "Échange refusé",
                Message = accept
                    ? $"{trade.ToUserName} a accepté votre échange."
                    : $"{trade.ToUserName} a refusé votre échange.",
                RelatedEntityId = trade.Id,
                RelatedEntityType = "SessionTrade",
                ActionUrl = $"/sessions/{trade.SessionId}/player",
                SentBy = userId
            });

            this.logger.LogInformation(
                "Trade {TradeId} {Status} by {UserId}", tradeId, trade.Status, userId);

            return MapToDto(trade);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error responding to trade {TradeId}", tradeId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SessionTradeDto?> CancelTradeAsync(int tradeId, int userId)
    {
        try
        {
            var trade = await this.dbContext.SessionTrades.FirstOrDefaultAsync(t => t.Id == tradeId);
            if (trade == null || trade.Status != TradeStatus.Pending)
            {
                return null;
            }

            // Only the proposer may cancel.
            if (trade.FromUserId != userId)
            {
                this.logger.LogWarning("User {UserId} is not the proposer of trade {TradeId}", userId, tradeId);
                return null;
            }

            trade.Status = TradeStatus.Cancelled;
            trade.RespondedAt = DateTime.UtcNow;
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Trade {TradeId} cancelled by {UserId}", tradeId, userId);

            return MapToDto(trade);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error cancelling trade {TradeId}", tradeId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SessionTradeDto>?> GetPendingTradesAsync(int sessionId, int userId)
    {
        try
        {
            var session = await this.dbContext.Sessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null)
            {
                return null;
            }

            if (!await this.IsMemberAsync(session, userId))
            {
                return null;
            }

            var trades = await this.dbContext.SessionTrades
                .AsNoTracking()
                .Where(t => t.SessionId == sessionId && t.Status == TradeStatus.Pending)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            return trades.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving pending trades for session {SessionId}", sessionId);
            return null;
        }
    }

    /// <summary>
    /// Determines whether a user belongs to a session, either as the GM or as a participant.
    /// </summary>
    private async Task<bool> IsMemberAsync(Session session, int userId)
    {
        if (userId <= 0)
        {
            return false;
        }

        if (session.StartedById == userId)
        {
            return true;
        }

        return await this.dbContext.SessionParticipants
            .AsNoTracking()
            .AnyAsync(p => p.SessionId == session.Id && p.WorldCharacter.Character.UserId == userId);
    }

    /// <summary>
    /// Resolves a user's display name (nickname, falling back to email).
    /// </summary>
    private async Task<string> GetUserNameAsync(int userId)
    {
        var user = await this.dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        return user?.Nickname ?? user?.Email ?? "Inconnu";
    }

    private static SessionTradeDto MapToDto(SessionTrade trade) => new()
    {
        Id = trade.Id,
        SessionId = trade.SessionId,
        FromUserId = trade.FromUserId,
        FromUserName = trade.FromUserName,
        ToUserId = trade.ToUserId,
        ToUserName = trade.ToUserName,
        OfferDescription = trade.OfferDescription,
        RequestDescription = trade.RequestDescription,
        Status = trade.Status,
        CreatedAt = trade.CreatedAt,
        RespondedAt = trade.RespondedAt
    };
}
