// -----------------------------------------------------------------------
// <copyright file="CombatHub.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

/// <summary>
/// SignalR hub for real-time combat management.
/// Handles turn order, initiative, attacks, damage, and combat state.
/// </summary>
[Authorize]
public class CombatHub : Hub
{
    private readonly ILogger<CombatHub> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatHub"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public CombatHub(ILogger<CombatHub> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Joins a combat session.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task JoinCombat(string combatId)
    {
        var userId = this.GetUserId();
        var userName = this.Context.User?.Identity?.Name ?? "Unknown";
        var groupName = $"combat_{combatId}";

        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupName);

        this.logger.LogInformation(
            "User {UserId} ({UserName}) joined combat {CombatId}",
            userId,
            userName,
            combatId);

        await this.Clients.OthersInGroup(groupName).SendAsync(
            "UserJoinedCombat",
            new { UserId = userId, UserName = userName, Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Leaves a combat session.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task LeaveCombat(string combatId)
    {
        var userId = this.GetUserId();
        var userName = this.Context.User?.Identity?.Name ?? "Unknown";
        var groupName = $"combat_{combatId}";

        await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupName);

        this.logger.LogInformation(
            "User {UserId} ({UserName}) left combat {CombatId}",
            userId,
            userName,
            combatId);

        await this.Clients.Group(groupName).SendAsync(
            "UserLeftCombat",
            new { UserId = userId, UserName = userName, Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Starts a combat encounter.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <param name="participants">List of participants with initial initiative.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task StartCombat(string combatId, List<CombatParticipant> participants)
    {
        var userId = this.GetUserId();
        var groupName = $"combat_{combatId}";

        this.logger.LogInformation(
            "User {UserId} started combat {CombatId} with {Count} participants",
            userId,
            combatId,
            participants.Count);

        // Order by initiative descending
        var orderedParticipants = participants.OrderByDescending(p => p.Initiative).ToList();

        await this.Clients.Group(groupName).SendAsync(
            "CombatStarted",
            new
            {
                CombatId = combatId,
                Participants = orderedParticipants,
                CurrentTurn = 0,
                StartedBy = userId,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Advances to the next turn in combat.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <param name="currentTurnIndex">The current turn index.</param>
    /// <param name="totalParticipants">Total number of participants.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task NextTurn(string combatId, int currentTurnIndex, int totalParticipants)
    {
        var userId = this.GetUserId();
        var groupName = $"combat_{combatId}";
        var nextTurnIndex = (currentTurnIndex + 1) % totalParticipants;

        this.logger.LogInformation(
            "Combat {CombatId} advanced to turn {NextTurn}",
            combatId,
            nextTurnIndex);

        await this.Clients.Group(groupName).SendAsync(
            "TurnChanged",
            new
            {
                CombatId = combatId,
                CurrentTurn = nextTurnIndex,
                ChangedBy = userId,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Updates a participant's initiative.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <param name="participantId">The participant identifier.</param>
    /// <param name="newInitiative">The new initiative value.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task UpdateInitiative(string combatId, int participantId, int newInitiative)
    {
        var userId = this.GetUserId();
        var groupName = $"combat_{combatId}";

        this.logger.LogInformation(
            "Combat {CombatId} - Participant {ParticipantId} initiative updated to {Initiative}",
            combatId,
            participantId,
            newInitiative);

        await this.Clients.Group(groupName).SendAsync(
            "InitiativeUpdated",
            new
            {
                CombatId = combatId,
                ParticipantId = participantId,
                NewInitiative = newInitiative,
                UpdatedBy = userId,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Broadcasts an attack action.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <param name="attackerId">The attacker character ID.</param>
    /// <param name="targetId">The target character ID.</param>
    /// <param name="attackType">Type of attack (e.g., "melee", "ranged", "spell").</param>
    /// <param name="attackRoll">The attack roll result.</param>
    /// <param name="targetAC">The target's armor class.</param>
    /// <param name="isHit">Whether the attack hit.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task Attack(string combatId, int attackerId, int targetId, string attackType, int attackRoll, int targetAC, bool isHit)
    {
        var userId = this.GetUserId();
        var groupName = $"combat_{combatId}";

        this.logger.LogInformation(
            "Combat {CombatId} - Character {Attacker} attacked {Target}: {Result}",
            combatId,
            attackerId,
            targetId,
            isHit ? "HIT" : "MISS");

        await this.Clients.Group(groupName).SendAsync(
            "AttackPerformed",
            new
            {
                CombatId = combatId,
                AttackerId = attackerId,
                TargetId = targetId,
                AttackType = attackType,
                AttackRoll = attackRoll,
                TargetAC = targetAC,
                IsHit = isHit,
                PerformedBy = userId,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Broadcasts damage dealt to a character.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <param name="targetId">The target character ID.</param>
    /// <param name="damage">Amount of damage dealt.</param>
    /// <param name="damageType">Type of damage (e.g., "slashing", "fire", "poison").</param>
    /// <param name="remainingHP">Remaining hit points after damage.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task DealDamage(string combatId, int targetId, int damage, string damageType, int remainingHP)
    {
        var userId = this.GetUserId();
        var groupName = $"combat_{combatId}";

        this.logger.LogInformation(
            "Combat {CombatId} - Character {Target} took {Damage} {DamageType} damage (HP: {RemainingHP})",
            combatId,
            targetId,
            damage,
            damageType,
            remainingHP);

        await this.Clients.Group(groupName).SendAsync(
            "DamageDealt",
            new
            {
                CombatId = combatId,
                TargetId = targetId,
                Damage = damage,
                DamageType = damageType,
                RemainingHP = remainingHP,
                IsKO = remainingHP <= 0,
                DealtBy = userId,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Ends the combat encounter.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <param name="victorySide">Which side won (e.g., "players", "enemies", "draw").</param>
    /// <param name="summary">Combat summary/notes.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task EndCombat(string combatId, string victorySide, string? summary)
    {
        var userId = this.GetUserId();
        var groupName = $"combat_{combatId}";

        this.logger.LogInformation(
            "Combat {CombatId} ended - Victory: {VictorySide}",
            combatId,
            victorySide);

        await this.Clients.Group(groupName).SendAsync(
            "CombatEnded",
            new
            {
                CombatId = combatId,
                VictorySide = victorySide,
                Summary = summary,
                EndedBy = userId,
                Timestamp = DateTime.UtcNow
            });
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

/// <summary>
/// Represents a combat participant with initiative.
/// </summary>
public class CombatParticipant
{
    /// <summary>
    /// Gets or sets the participant ID (character or NPC ID).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the participant name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the initiative roll value.
    /// </summary>
    public int Initiative { get; set; }

    /// <summary>
    /// Gets or sets whether this is a player character.
    /// </summary>
    public bool IsPlayerCharacter { get; set; }
}
