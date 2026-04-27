// -----------------------------------------------------------------------
// <copyright file="CombatService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing generic combat encounters within game sessions.
/// </summary>
/// <param name="dbContext">Database context.</param>
/// <param name="logger">Logger instance.</param>
public class CombatService(
    AppDbContext dbContext,
    ILogger<CombatService> logger) : ICombatService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<CombatService> logger = logger;

    /// <inheritdoc/>
    public async Task<CombatDto?> CreateCombatAsync(CreateCombatDto request, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Creating combat for session {SessionId} by user {UserId}",
                request.SessionId,
                userId);

            if (!await this.IsGmOfSessionAsync(request.SessionId, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to create a combat for session {SessionId}",
                    userId,
                    request.SessionId);
                return null;
            }

            var combat = new Combat
            {
                SessionId = request.SessionId,
                ChapterId = request.ChapterId,
                Status = 0, // Setup
                StartedById = userId,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CurrentTurnOrder = 0,
            };

            this.dbContext.Combats.Add(combat);
            await this.dbContext.SaveChangesAsync();

            int displayOrder = 0;
            foreach (var groupDto in request.Groups)
            {
                var group = new CombatGroup
                {
                    CombatId = combat.Id,
                    Name = groupDto.Name,
                    Color = groupDto.Color,
                    DisplayOrder = displayOrder++,
                };

                this.dbContext.CombatGroups.Add(group);
                await this.dbContext.SaveChangesAsync();

                foreach (var participantDto in groupDto.Participants)
                {
                    var participant = new CombatParticipant
                    {
                        CombatId = combat.Id,
                        GroupId = group.Id,
                        Name = participantDto.Name,
                        IsPlayerCharacter = participantDto.IsPlayerCharacter,
                        CharacterId = participantDto.CharacterId,
                        NpcId = participantDto.NpcId,
                        UserId = participantDto.UserId,
                        MaxHp = participantDto.MaxHp,
                        CurrentHp = participantDto.MaxHp,
                        IsActive = true,
                        TurnOrder = 0,
                    };

                    this.dbContext.CombatParticipants.Add(participant);
                }
            }

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Created combat {CombatId} for session {SessionId}", combat.Id, request.SessionId);

            return await this.LoadCombatDtoAsync(combat.Id);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating combat for session {SessionId}", request.SessionId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatDto?> GetCombatAsync(int combatId, int userId)
    {
        try
        {
            var combat = await this.dbContext.Combats
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == combatId);

            if (combat == null)
            {
                this.logger.LogWarning("Combat {CombatId} not found", combatId);
                return null;
            }

            // Participants in the session (players) or the GM can view the combat.
            if (!await this.IsGmOfSessionAsync(combat.SessionId, userId) &&
                !await this.IsParticipantOfSessionAsync(combat.SessionId, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to view combat {CombatId}",
                    userId,
                    combatId);
                return null;
            }

            return await this.LoadCombatDtoAsync(combatId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving combat {CombatId}", combatId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatDto?> GetActiveCombatForSessionAsync(int sessionId, int userId)
    {
        try
        {
            var combat = await this.dbContext.Combats
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Status < 3);

            if (combat == null) return null;

            if (!await this.IsGmOfSessionAsync(sessionId, userId) &&
                !await this.IsParticipantOfSessionAsync(sessionId, userId))
            {
                return null;
            }

            return await this.LoadCombatDtoAsync(combat.Id);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving active combat for session {SessionId}", sessionId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatDto?> StartInitiativePhaseAsync(int combatId, StartInitiativeDto request, int userId)
    {
        try
        {
            var combat = await this.dbContext.Combats
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == combatId);

            if (combat == null) return null;

            if (!await this.IsGmOfSessionAsync(combat.SessionId, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to start initiative for combat {CombatId}",
                    userId,
                    combatId);
                return null;
            }

            combat.Status = 1; // Initiative

            // In "roll" mode, auto-roll for NPCs; players set their own via SetInitiative.
            if (string.Equals(request.Mode, "roll", StringComparison.OrdinalIgnoreCase))
            {
                var rng = new Random();
                string expression = request.DiceExpression ?? "1d20";

                foreach (var participant in combat.Participants.Where(p => p.IsActive && !p.IsPlayerCharacter))
                {
                    participant.Initiative = RollDice(rng, expression);
                }
            }

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Initiative phase started for combat {CombatId} (mode: {Mode})",
                combatId,
                request.Mode);

            return await this.LoadCombatDtoAsync(combatId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error starting initiative phase for combat {CombatId}", combatId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatDto?> SetInitiativeAsync(int combatId, int participantId, SetInitiativeDto request, int userId)
    {
        try
        {
            var combat = await this.dbContext.Combats
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == combatId);

            if (combat == null) return null;

            var participant = combat.Participants.FirstOrDefault(p => p.Id == participantId);
            if (participant == null) return null;

            bool isGm = await this.IsGmOfSessionAsync(combat.SessionId, userId);

            // A player may only set initiative for their own participant.
            if (!isGm && participant.UserId != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} attempted to set initiative for participant {ParticipantId} they do not own",
                    userId,
                    participantId);
                return null;
            }

            participant.Initiative = request.Value;
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Initiative set to {Value} for participant {ParticipantId} in combat {CombatId}",
                request.Value,
                participantId,
                combatId);

            return await this.LoadCombatDtoAsync(combatId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error setting initiative for participant {ParticipantId}", participantId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatDto?> StartCombatAsync(int combatId, StartCombatDto? request, int userId)
    {
        try
        {
            var combat = await this.dbContext.Combats
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == combatId);

            if (combat == null) return null;

            if (!await this.IsGmOfSessionAsync(combat.SessionId, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to start combat {CombatId}",
                    userId,
                    combatId);
                return null;
            }

            // Use explicit order if provided (GM reordered in setup), else sort by initiative desc.
            List<CombatParticipant> sorted;
            if (request?.ParticipantIds != null && request.ParticipantIds.Count > 0)
            {
                sorted = request.ParticipantIds
                    .Select(id => combat.Participants.FirstOrDefault(p => p.Id == id && p.IsActive))
                    .Where(p => p != null)
                    .Select(p => p!)
                    .ToList();
                // append any active participants not in the explicit list
                var listed = sorted.Select(p => p.Id).ToHashSet();
                sorted.AddRange(combat.Participants
                    .Where(p => p.IsActive && !listed.Contains(p.Id))
                    .OrderByDescending(p => p.Initiative ?? 0));
            }
            else
            {
                sorted = combat.Participants
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.Initiative ?? 0)
                    .ToList();
            }

            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].TurnOrder = i;
            }

            combat.Status = 2; // Active
            combat.CurrentTurnOrder = 0;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Combat {CombatId} started (Active phase)", combatId);

            return await this.LoadCombatDtoAsync(combatId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error starting combat {CombatId}", combatId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatActionDto?> RecordActionAsync(int combatId, CreateCombatActionDto request, int userId)
    {
        try
        {
            var combatExists = await this.dbContext.Combats
                .AnyAsync(c => c.Id == combatId);

            if (!combatExists) return null;

            var action = new CombatAction
            {
                CombatId = combatId,
                ParticipantName = request.ParticipantName,
                ActionType = request.ActionType,
                Description = request.Description,
                DiceExpression = request.DiceExpression,
                DiceResult = request.DiceResult,
                IsPrivate = request.IsPrivate,
                PerformedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            this.dbContext.CombatActions.Add(action);
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Recorded action '{ActionType}' in combat {CombatId} by user {UserId}",
                request.ActionType,
                combatId,
                userId);

            return MapActionToDto(action);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error recording action in combat {CombatId}", combatId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatParticipantDto?> UpdateHpAsync(int combatId, int participantId, UpdateHpDto request, int userId)
    {
        try
        {
            var participant = await this.dbContext.CombatParticipants
                .Include(p => p.Group)
                .FirstOrDefaultAsync(p => p.Id == participantId && p.CombatId == combatId);

            if (participant == null) return null;

            bool isGm = await this.IsGmOfSessionAsync(combatId, userId, isCombatId: true);

            // Players can only update HP for their own participant.
            if (!isGm && participant.UserId != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to update HP for participant {ParticipantId}",
                    userId,
                    participantId);
                return null;
            }

            participant.CurrentHp = Math.Max(0, Math.Min(request.NewHp, participant.MaxHp));
            if (participant.CurrentHp == 0)
            {
                participant.IsActive = false;
            }

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "HP updated to {Hp} for participant {ParticipantId} in combat {CombatId}",
                participant.CurrentHp,
                participantId,
                combatId);

            return MapParticipantToDto(participant);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating HP for participant {ParticipantId}", participantId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatDto?> NextTurnAsync(int combatId, int userId)
    {
        try
        {
            var combat = await this.dbContext.Combats
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == combatId);

            if (combat == null) return null;

            if (!await this.IsGmOfSessionAsync(combat.SessionId, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to advance turn in combat {CombatId}",
                    userId,
                    combatId);
                return null;
            }

            var activeParticipants = combat.Participants
                .Where(p => p.IsActive)
                .OrderBy(p => p.TurnOrder)
                .ToList();

            if (activeParticipants.Count == 0) return await this.LoadCombatDtoAsync(combatId);

            // Find the next active turn order after the current one (cyclically).
            int current = combat.CurrentTurnOrder;
            int maxOrder = activeParticipants.Max(p => p.TurnOrder);

            // Look for the next participant with TurnOrder > current; wrap around if needed.
            var next = activeParticipants.FirstOrDefault(p => p.TurnOrder > current)
                ?? activeParticipants.First(); // wrap around

            combat.CurrentTurnOrder = next.TurnOrder;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Combat {CombatId} advanced to turn order {TurnOrder}",
                combatId,
                combat.CurrentTurnOrder);

            return await this.LoadCombatDtoAsync(combatId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error advancing turn in combat {CombatId}", combatId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatDto?> EndCombatAsync(int combatId, EndCombatDto request, int userId)
    {
        try
        {
            var combat = await this.dbContext.Combats
                .FirstOrDefaultAsync(c => c.Id == combatId);

            if (combat == null) return null;

            if (!await this.IsGmOfSessionAsync(combat.SessionId, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to end combat {CombatId}",
                    userId,
                    combatId);
                return null;
            }

            combat.Status = 3; // Ended
            combat.EndedAt = DateTime.UtcNow;
            combat.VictorySide = request.VictorySide;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Combat {CombatId} ended. Victory: {VictorySide}",
                combatId,
                request.VictorySide ?? "none");

            return await this.LoadCombatDtoAsync(combatId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error ending combat {CombatId}", combatId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CombatDto?> ToggleParticipantActiveAsync(int combatId, int participantId, SetActiveDto request, int userId)
    {
        try
        {
            var combat = await this.dbContext.Combats
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == combatId);

            if (combat == null) return null;

            if (!await this.IsGmOfSessionAsync(combat.SessionId, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to toggle participant active in combat {CombatId}",
                    userId,
                    combatId);
                return null;
            }

            var participant = combat.Participants.FirstOrDefault(p => p.Id == participantId);
            if (participant == null) return null;

            participant.IsActive = request.IsActive;

            // If deactivating the current active turn participant, advance the turn.
            if (!request.IsActive && combat.CurrentTurnOrder == participant.TurnOrder && combat.Status == 2)
            {
                var activeParticipants = combat.Participants
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.TurnOrder)
                    .ToList();

                if (activeParticipants.Count > 0)
                {
                    var next = activeParticipants.FirstOrDefault(p => p.TurnOrder > combat.CurrentTurnOrder)
                        ?? activeParticipants.First();
                    combat.CurrentTurnOrder = next.TurnOrder;
                }
            }

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Participant {ParticipantId} active status set to {IsActive} in combat {CombatId}",
                participantId,
                request.IsActive,
                combatId);

            return await this.LoadCombatDtoAsync(combatId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error toggling participant {ParticipantId} active status", participantId);
            return null;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<bool> IsGmOfSessionAsync(int sessionId, int userId)
    {
        var session = await this.dbContext.Sessions
            .Include(s => s.Campaign)
                .ThenInclude(c => c.World)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return false;

        return session.Campaign.World?.UserId == userId
            || session.Campaign.CreatedBy == userId;
    }

    /// <summary>
    /// Overload that accepts either a session ID or a combat ID.
    /// </summary>
    private async Task<bool> IsGmOfSessionAsync(int id, int userId, bool isCombatId)
    {
        if (!isCombatId) return await this.IsGmOfSessionAsync(id, userId);

        var combat = await this.dbContext.Combats.FindAsync(id);
        if (combat == null) return false;

        return await this.IsGmOfSessionAsync(combat.SessionId, userId);
    }

    private async Task<bool> IsParticipantOfSessionAsync(int sessionId, int userId)
    {
        return await this.dbContext.SessionParticipants
            .AnyAsync(sp => sp.SessionId == sessionId
                && sp.WorldCharacter != null
                && sp.WorldCharacter.Character != null
                && sp.WorldCharacter.Character.UserId == userId);
    }

    private async Task<CombatDto?> LoadCombatDtoAsync(int combatId)
    {
        var combat = await this.dbContext.Combats
            .AsNoTracking()
            .Include(c => c.Groups)
            .Include(c => c.Participants)
                .ThenInclude(p => p.Group)
            .Include(c => c.Actions)
            .FirstOrDefaultAsync(c => c.Id == combatId);

        if (combat == null) return null;

        return new CombatDto
        {
            Id = combat.Id,
            SessionId = combat.SessionId,
            ChapterId = combat.ChapterId,
            Status = combat.Status,
            CurrentTurnOrder = combat.CurrentTurnOrder,
            StartedAt = combat.StartedAt,
            EndedAt = combat.EndedAt,
            VictorySide = combat.VictorySide,
            Groups = combat.Groups
                .OrderBy(g => g.DisplayOrder)
                .Select(g => new CombatGroupDto
                {
                    Id = g.Id,
                    CombatId = g.CombatId,
                    Name = g.Name,
                    Color = g.Color,
                    DisplayOrder = g.DisplayOrder,
                })
                .ToList(),
            Participants = combat.Participants
                .OrderBy(p => p.TurnOrder)
                .Select(MapParticipantToDto)
                .ToList(),
            Actions = combat.Actions
                .OrderBy(a => a.CreatedAt)
                .Select(MapActionToDto)
                .ToList(),
        };
    }

    private static CombatParticipantDto MapParticipantToDto(CombatParticipant p)
    {
        return new CombatParticipantDto
        {
            Id = p.Id,
            CombatId = p.CombatId,
            GroupId = p.GroupId,
            GroupName = p.Group?.Name ?? string.Empty,
            GroupColor = p.Group?.Color ?? string.Empty,
            Name = p.Name,
            IsPlayerCharacter = p.IsPlayerCharacter,
            CharacterId = p.CharacterId,
            NpcId = p.NpcId,
            UserId = p.UserId,
            CurrentHp = p.CurrentHp,
            MaxHp = p.MaxHp,
            Initiative = p.Initiative,
            TurnOrder = p.TurnOrder,
            IsActive = p.IsActive,
        };
    }

    private static CombatActionDto MapActionToDto(CombatAction a)
    {
        return new CombatActionDto
        {
            Id = a.Id,
            CombatId = a.CombatId,
            ParticipantName = a.ParticipantName,
            ActionType = a.ActionType,
            Description = a.Description,
            DiceExpression = a.DiceExpression,
            DiceResult = a.DiceResult,
            IsPrivate = a.IsPrivate,
            CreatedAt = a.CreatedAt,
        };
    }

    /// <summary>Parses "NdM" and rolls N dice with M faces, returning the sum.</summary>
    private static int RollDice(Random rng, string expression)
    {
        try
        {
            var lower = expression.ToLowerInvariant().Trim();
            var parts = lower.Split('d');
            if (parts.Length != 2) return rng.Next(1, 21);

            int count = int.TryParse(parts[0], out var n) ? Math.Max(1, n) : 1;
            int faces = int.TryParse(parts[1], out var m) ? Math.Max(2, m) : 20;

            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total += rng.Next(1, faces + 1);
            }

            return total;
        }
        catch
        {
            return rng.Next(1, 21);
        }
    }
}
