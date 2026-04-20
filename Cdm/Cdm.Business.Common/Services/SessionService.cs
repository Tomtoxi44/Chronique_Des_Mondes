// -----------------------------------------------------------------------
// <copyright file="SessionService.cs" company="ANGIBAUD Tommy">
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
/// Service for managing game sessions.
/// </summary>
public class SessionService(AppDbContext dbContext, ILogger<SessionService> logger, INotificationService notificationService) : ISessionService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<SessionService> logger = logger;
    private readonly INotificationService notificationService = notificationService;

    /// <inheritdoc/>
    public async Task<SessionDto?> StartSessionAsync(StartSessionDto dto, int userId)
    {
        try
        {
            var campaign = await this.dbContext.Campaigns
                .Include(c => c.World)
                .FirstOrDefaultAsync(c => c.Id == dto.CampaignId && c.IsActive && !c.IsDeleted);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign {CampaignId} not found", dto.CampaignId);
                return null;
            }

            if (campaign.CreatedBy != userId)
            {
                this.logger.LogWarning("User {UserId} is not the GM of campaign {CampaignId}", userId, dto.CampaignId);
                return null;
            }

            // Check for existing active session
            var existingActive = await this.dbContext.Sessions
                .AnyAsync(s => s.CampaignId == dto.CampaignId && s.Status == SessionStatus.Active);
            if (existingActive)
            {
                this.logger.LogWarning("Campaign {CampaignId} already has an active session", dto.CampaignId);
                return null;
            }

            var session = new Session
            {
                CampaignId = dto.CampaignId,
                StartedById = userId,
                StartedAt = DateTime.UtcNow,
                Status = SessionStatus.Active,
                WelcomeMessage = dto.WelcomeMessage
            };

            this.dbContext.Sessions.Add(session);
            await this.dbContext.SaveChangesAsync();

            // Add invited participants
            foreach (var worldCharId in dto.WorldCharacterIds)
            {
                var worldChar = await this.dbContext.WorldCharacters
                    .Include(wc => wc.Character)
                    .FirstOrDefaultAsync(wc => wc.Id == worldCharId && wc.WorldId == campaign.WorldId && wc.IsActive);
                if (worldChar != null)
                {
                    this.dbContext.SessionParticipants.Add(new SessionParticipant
                    {
                        SessionId = session.Id,
                        WorldCharacterId = worldCharId,
                        JoinedAt = DateTime.UtcNow,
                        Status = SessionParticipantStatus.Invited
                    });

                    // Notify the player
                    await this.notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = worldChar.Character.UserId,
                        Type = NotificationType.SessionStarting,
                        Title = "Session de jeu lancée",
                        Message = $"Le MJ vient de lancer une session pour la campagne « {campaign.Name} ».",
                        RelatedEntityId = session.Id,
                        RelatedEntityType = "Session",
                        ActionUrl = $"/sessions/{session.Id}/player",
                        SentBy = userId
                    });
                }
            }

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Session {SessionId} started for campaign {CampaignId}", session.Id, dto.CampaignId);

            return await this.BuildSessionDtoAsync(session.Id);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error starting session for campaign {CampaignId}", dto.CampaignId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SessionDto?> GetSessionAsync(int sessionId, int userId)
    {
        try
        {
            var session = await this.dbContext.Sessions
                .Include(s => s.Campaign)
                .Include(s => s.Participants)
                    .ThenInclude(p => p.WorldCharacter)
                        .ThenInclude(wc => wc.Character)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                return null;
            }

            // Authorize: GM or participant
            var isGm = session.StartedById == userId;
            var isParticipant = session.Participants
                .Any(p => p.WorldCharacter.Character.UserId == userId);

            if (!isGm && !isParticipant)
            {
                return null;
            }

            return await this.BuildSessionDtoAsync(sessionId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SessionDto?> GetActiveSessionByCampaignAsync(int campaignId, int userId)
    {
        try
        {
            var session = await this.dbContext.Sessions
                .Include(s => s.Campaign)
                .FirstOrDefaultAsync(s => s.CampaignId == campaignId && s.Status == SessionStatus.Active);

            if (session == null)
            {
                return null;
            }

            // Authorize: GM or participant
            var isGm = session.StartedById == userId;
            var isParticipant = await this.dbContext.SessionParticipants
                .AnyAsync(p => p.SessionId == session.Id && p.WorldCharacter.Character.UserId == userId);

            if (!isGm && !isParticipant)
            {
                return null;
            }

            return await this.BuildSessionDtoAsync(session.Id);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving active session for campaign {CampaignId}", campaignId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SessionDto>> GetMySessionsAsync(int userId)
    {
        try
        {
            // Sessions where user is GM
            var gmSessionIds = await this.dbContext.Sessions
                .Where(s => s.StartedById == userId)
                .Select(s => s.Id)
                .ToListAsync();

            // Sessions where user is participant
            var playerSessionIds = await this.dbContext.SessionParticipants
                .Where(p => p.WorldCharacter.Character.UserId == userId)
                .Select(p => p.SessionId)
                .Distinct()
                .ToListAsync();

            var allIds = gmSessionIds.Union(playerSessionIds).ToList();
            var result = new List<SessionDto>();

            foreach (var id in allIds)
            {
                var dto = await this.BuildSessionDtoAsync(id);
                if (dto != null) result.Add(dto);
            }

            return result.OrderByDescending(s => s.StartedAt);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving sessions for user {UserId}", userId);
            return Enumerable.Empty<SessionDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> EndSessionAsync(int sessionId, int userId)
    {
        try
        {
            var session = await this.dbContext.Sessions
                .Include(s => s.Campaign)
                .Include(s => s.Participants)
                    .ThenInclude(p => p.WorldCharacter)
                        .ThenInclude(wc => wc.Character)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                return false;
            }

            if (session.StartedById != userId)
            {
                this.logger.LogWarning("User {UserId} is not the GM of session {SessionId}", userId, sessionId);
                return false;
            }

            session.Status = SessionStatus.Ended;
            session.EndedAt = DateTime.UtcNow;
            await this.dbContext.SaveChangesAsync();

            // Notify participants that the session has ended
            foreach (var participant in session.Participants)
            {
                if (participant.WorldCharacter?.Character == null) continue;
                await this.notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = participant.WorldCharacter.Character.UserId,
                    Type = NotificationType.SessionEnded,
                    Title = "Session terminée",
                    Message = $"La session de la campagne « {session.Campaign?.Name ?? "Campagne"} » est terminée.",
                    RelatedEntityId = session.Id,
                    RelatedEntityType = "Session",
                    ActionUrl = $"/sessions",
                    SentBy = userId
                });
            }

            this.logger.LogInformation("Session {SessionId} ended by user {UserId}", sessionId, userId);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error ending session {SessionId}", sessionId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<SessionDto?> JoinSessionAsync(int sessionId, int worldCharacterId, int userId)
    {
        try
        {
            var session = await this.dbContext.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.Status == SessionStatus.Active);
            if (session == null)
            {
                return null;
            }

            var participant = await this.dbContext.SessionParticipants
                .Include(p => p.WorldCharacter).ThenInclude(wc => wc.Character)
                .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.WorldCharacterId == worldCharacterId);

            if (participant == null || participant.WorldCharacter.Character.UserId != userId)
            {
                return null;
            }

            participant.Status = SessionParticipantStatus.Joined;
            await this.dbContext.SaveChangesAsync();

            return await this.BuildSessionDtoAsync(sessionId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error joining session {SessionId}", sessionId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SessionDto?> UpdateCurrentChapterAsync(int sessionId, int? chapterId, int userId)
    {
        try
        {
            var session = await this.dbContext.Sessions.FindAsync(sessionId);
            if (session == null || session.StartedById != userId)
            {
                return null;
            }

            session.CurrentChapterId = chapterId;
            await this.dbContext.SaveChangesAsync();

            return await this.BuildSessionDtoAsync(sessionId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating current chapter for session {SessionId}", sessionId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> LeaveSessionAsync(int sessionId, int userId)
    {
        try
        {
            var participant = await this.dbContext.SessionParticipants
                .Include(p => p.WorldCharacter).ThenInclude(wc => wc.Character)
                .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.WorldCharacter.Character.UserId == userId);

            if (participant == null)
            {
                this.logger.LogWarning("Participant for user {UserId} not found in session {SessionId}", userId, sessionId);
                return false;
            }

            participant.Status = SessionParticipantStatus.Left;
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("User {UserId} left session {SessionId}", userId, sessionId);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error leaving session {SessionId} for user {UserId}", sessionId, userId);
            return false;
        }
    }

    private async Task<SessionDto?> BuildSessionDtoAsync(int sessionId)
    {
        var session = await this.dbContext.Sessions
            .Include(s => s.Campaign).ThenInclude(c => c.World)
            .Include(s => s.StartedBy)
            .Include(s => s.CurrentChapter)
            .Include(s => s.Participants)
                .ThenInclude(p => p.WorldCharacter).ThenInclude(wc => wc.Character)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return null;

        return new SessionDto
        {
            Id = session.Id,
            CampaignId = session.CampaignId,
            CampaignName = session.Campaign?.Name ?? string.Empty,
            WorldId = session.Campaign?.WorldId ?? 0,
            StartedById = session.StartedById,
            StartedByName = session.StartedBy?.Nickname ?? session.StartedBy?.Email ?? string.Empty,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            Status = session.Status,
            CurrentChapterId = session.CurrentChapterId,
            CurrentChapterTitle = session.CurrentChapter?.Title,
            WelcomeMessage = session.WelcomeMessage,
            Participants = session.Participants.Select(p => new SessionParticipantDto
            {
                Id = p.Id,
                SessionId = p.SessionId,
                WorldCharacterId = p.WorldCharacterId,
                CharacterName = p.WorldCharacter?.Character?.Name ?? string.Empty,
                UserId = p.WorldCharacter?.Character?.UserId ?? 0,
                UserName = p.WorldCharacter?.Character?.Owner?.Nickname ?? string.Empty,
                JoinedAt = p.JoinedAt,
                Status = p.Status
            })
        };
    }
}
