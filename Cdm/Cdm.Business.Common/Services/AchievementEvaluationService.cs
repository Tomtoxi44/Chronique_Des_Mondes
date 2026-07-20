// -----------------------------------------------------------------------
// <copyright file="AchievementEvaluationService.cs" company="ANGIBAUD Tommy">
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
/// Evaluates and awards automatic achievements. Every entry point is best-effort:
/// exceptions are logged and swallowed so gameplay actions are never broken by
/// achievement evaluation.
/// </summary>
public class AchievementEvaluationService(
    AppDbContext dbContext,
    INotificationService notificationService,
    ILogger<AchievementEvaluationService> logger) : IAchievementEvaluationService
{
    private const int CombatEndedStatus = 3;

    private readonly AppDbContext dbContext = dbContext;
    private readonly INotificationService notificationService = notificationService;
    private readonly ILogger<AchievementEvaluationService> logger = logger;

    /// <inheritdoc/>
    public async Task OnDiceRolledAsync(int userId, int sessionId, string diceType, int[] results)
    {
        try
        {
            var scope = await this.ResolveSessionScopeAsync(sessionId);
            if (scope is null)
            {
                return;
            }

            var isD20 = string.Equals(diceType?.TrimStart('d', 'D'), "20", StringComparison.OrdinalIgnoreCase);
            if (isD20 && results.Contains(20))
            {
                await this.AwardMatchingAsync(userId, scope.Value, AchievementConditionType.DiceCritical, currentStat: 1);
            }

            if (isD20 && results.Contains(1))
            {
                await this.AwardMatchingAsync(userId, scope.Value, AchievementConditionType.DiceFumble, currentStat: 1);
            }

            var totalDice = await this.dbContext.SessionDiceRolls
                .Where(d => d.UserId == userId)
                .SumAsync(d => d.Count);

            await this.AwardMatchingAsync(userId, scope.Value, AchievementConditionType.DiceRolls, totalDice);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Achievement evaluation failed after a dice roll (user {UserId}, session {SessionId})", userId, sessionId);
        }
    }

    /// <inheritdoc/>
    public async Task OnSessionAttendedAsync(int userId, int sessionId)
    {
        try
        {
            var scope = await this.ResolveSessionScopeAsync(sessionId);
            if (scope is null)
            {
                return;
            }

            var sessionsAttended = await this.dbContext.SessionParticipants
                .Where(p => p.WorldCharacter.Character.UserId == userId)
                .Select(p => p.SessionId)
                .Distinct()
                .CountAsync();

            await this.AwardMatchingAsync(userId, scope.Value, AchievementConditionType.SessionsAttended, sessionsAttended);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Achievement evaluation failed after session attendance (user {UserId}, session {SessionId})", userId, sessionId);
        }
    }

    /// <inheritdoc/>
    public async Task OnTradeAcceptedAsync(int userId, int sessionId)
    {
        try
        {
            var scope = await this.ResolveSessionScopeAsync(sessionId);
            if (scope is null)
            {
                return;
            }

            var tradesCompleted = await this.dbContext.SessionTrades
                .Where(t => t.Status == TradeStatus.Accepted && (t.FromUserId == userId || t.ToUserId == userId))
                .CountAsync();

            await this.AwardMatchingAsync(userId, scope.Value, AchievementConditionType.TradesCompleted, tradesCompleted);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Achievement evaluation failed after a trade (user {UserId}, session {SessionId})", userId, sessionId);
        }
    }

    /// <inheritdoc/>
    public async Task OnCombatEndedAsync(int combatId)
    {
        try
        {
            var combat = await this.dbContext.Combats
                .Include(c => c.Groups)
                    .ThenInclude(g => g.Participants)
                .FirstOrDefaultAsync(c => c.Id == combatId);

            if (combat is null)
            {
                return;
            }

            var scope = await this.ResolveSessionScopeAsync(combat.SessionId, combat.ChapterId);
            if (scope is null)
            {
                return;
            }

            var participants = combat.Groups.SelectMany(g => g.Participants).ToList();
            var playerUserIds = participants
                .Where(p => p.IsPlayerCharacter && p.UserId.HasValue)
                .Select(p => p.UserId!.Value)
                .Distinct()
                .ToList();

            foreach (var userId in playerUserIds)
            {
                var survivedThisCombat = participants.Any(p => p.UserId == userId && p.CurrentHp > 0);
                if (!survivedThisCombat)
                {
                    continue;
                }

                var survivedCount = await this.CountSurvivedCombatsAsync(userId);
                await this.AwardMatchingAsync(userId, scope.Value, AchievementConditionType.CombatSurvived, survivedCount);

                var wonThisCombat = !string.IsNullOrWhiteSpace(combat.VictorySide)
                    && combat.Groups.Any(g => g.Name == combat.VictorySide && g.Participants.Any(p => p.UserId == userId && p.CurrentHp > 0));

                if (wonThisCombat)
                {
                    var wonCount = await this.CountWonCombatsAsync(userId);
                    await this.AwardMatchingAsync(userId, scope.Value, AchievementConditionType.CombatWon, wonCount);
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Achievement evaluation failed after combat {CombatId} ended", combatId);
        }
    }

    /// <summary>
    /// Resolves the (world, campaign, chapter) scope a session belongs to.
    /// </summary>
    private async Task<Scope?> ResolveSessionScopeAsync(int sessionId, int? explicitChapterId = null)
    {
        var session = await this.dbContext.Sessions
            .AsNoTracking()
            .Include(s => s.Campaign)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session?.Campaign is null)
        {
            return null;
        }

        return new Scope(session.Campaign.WorldId, session.CampaignId, explicitChapterId ?? session.CurrentChapterId);
    }

    private async Task<int> CountSurvivedCombatsAsync(int userId) =>
        await this.dbContext.CombatParticipants
            .Where(p => p.UserId == userId && p.CurrentHp > 0 && p.Combat.Status == CombatEndedStatus)
            .Select(p => p.CombatId)
            .Distinct()
            .CountAsync();

    private async Task<int> CountWonCombatsAsync(int userId) =>
        await this.dbContext.CombatParticipants
            .Where(p => p.UserId == userId
                && p.CurrentHp > 0
                && p.Combat.Status == CombatEndedStatus
                && p.Combat.VictorySide != null
                && p.Group.Name == p.Combat.VictorySide)
            .Select(p => p.CombatId)
            .Distinct()
            .CountAsync();

    /// <summary>
    /// Awards every active automatic achievement in the given scope that matches the
    /// condition type, whose threshold is met, and that the user has not yet unlocked.
    /// </summary>
    private async Task AwardMatchingAsync(int userId, Scope scope, AchievementConditionType type, int currentStat)
    {
        var candidates = await this.dbContext.Achievements
            .Where(a => a.IsActive && a.IsAutomatic && a.AutomaticCondition != null
                && ((a.Level == AchievementLevel.World && a.WorldId == scope.WorldId)
                    || (a.Level == AchievementLevel.Campaign && a.CampaignId == scope.CampaignId)
                    || (a.Level == AchievementLevel.Chapter && scope.ChapterId != null && a.ChapterId == scope.ChapterId)))
            .ToListAsync();

        foreach (var achievement in candidates)
        {
            if (!AchievementCondition.TryParse(achievement.AutomaticCondition, out var condition) || condition.Type != type)
            {
                continue;
            }

            if (currentStat < condition.Threshold)
            {
                continue;
            }

            var alreadyUnlocked = await this.dbContext.UserAchievements
                .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id);
            if (alreadyUnlocked)
            {
                continue;
            }

            this.dbContext.UserAchievements.Add(new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.Id,
                UnlockedAt = DateTime.UtcNow,
                IsManuallyAwarded = false
            });
            await this.dbContext.SaveChangesAsync();

            await this.notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.AchievementUnlocked,
                Title = "Succès débloqué !",
                Message = $"Vous avez débloqué le succès « {achievement.Name} ».",
                RelatedEntityId = achievement.Id,
                RelatedEntityType = "Achievement"
            });

            this.logger.LogInformation(
                "Automatic achievement {AchievementId} unlocked for user {UserId} (condition {Condition})",
                achievement.Id, userId, achievement.AutomaticCondition);
        }
    }

    private readonly record struct Scope(int WorldId, int CampaignId, int? ChapterId);
}
