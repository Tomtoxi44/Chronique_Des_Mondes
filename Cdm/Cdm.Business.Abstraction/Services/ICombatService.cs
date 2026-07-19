// -----------------------------------------------------------------------
// <copyright file="ICombatService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Defines operations for managing generic combat encounters.
/// </summary>
public interface ICombatService
{
    /// <summary>Creates a new combat encounter with groups and participants.</summary>
    Task<CombatDto?> CreateCombatAsync(CreateCombatDto request, int userId);

    /// <summary>Returns a full combat encounter by ID.</summary>
    Task<CombatDto?> GetCombatAsync(int combatId, int userId);

    /// <summary>Returns the currently active (Status &lt; 3) combat for a session.</summary>
    Task<CombatDto?> GetActiveCombatForSessionAsync(int sessionId, int userId);

    /// <summary>Transitions the combat to the Initiative phase (Status = 1).</summary>
    Task<CombatDto?> StartInitiativePhaseAsync(int combatId, StartInitiativeDto request, int userId);

    /// <summary>Sets the initiative value for a single participant.</summary>
    Task<CombatDto?> SetInitiativeAsync(int combatId, int participantId, SetInitiativeDto request, int userId);

    /// <summary>
    /// Auto-rolls initiative for every active participant (1d20 + Dexterity modifier), server-side.
    /// Only the GM of the session may trigger it.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <param name="userId">The requesting user (must be GM).</param>
    /// <returns>The updated combat, or null if not found/unauthorized.</returns>
    Task<CombatDto?> RollInitiativeAsync(int combatId, int userId);

    /// <summary>
    /// Resolves an attack from one participant against another, server-side: rolls 1d20 + attack
    /// bonus against the target's armor class and, on a hit, rolls damage (with critical dice and
    /// resistance/vulnerability) and applies it. The GM or the attacker's owner may trigger it.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <param name="attackerParticipantId">The attacking participant's identifier.</param>
    /// <param name="request">The attack parameters (target, bonus, damage dice/type).</param>
    /// <param name="userId">The requesting user.</param>
    /// <returns>The updated combat, or null if not found/unauthorized.</returns>
    Task<CombatDto?> ResolveAttackAsync(int combatId, int attackerParticipantId, ResolveAttackDto request, int userId);

    /// <summary>
    /// Overrides a participant's defensive stats (armor class, Dexterity modifier, resistances,
    /// vulnerabilities), so the GM can correct the values auto-resolved at combat creation.
    /// Only the GM of the session may do this.
    /// </summary>
    /// <param name="combatId">The combat identifier.</param>
    /// <param name="participantId">The participant identifier.</param>
    /// <param name="request">The new defensive stats.</param>
    /// <param name="userId">The requesting user (must be GM).</param>
    /// <returns>The updated combat, or null if not found/unauthorized.</returns>
    Task<CombatDto?> UpdateParticipantDefenseAsync(int combatId, int participantId, UpdateParticipantDefenseDto request, int userId);

    /// <summary>Sorts participants by initiative and transitions the combat to Active (Status = 2).</summary>
    Task<CombatDto?> StartCombatAsync(int combatId, StartCombatDto? request, int userId);

    /// <summary>Records an action in the combat log.</summary>
    Task<CombatActionDto?> RecordActionAsync(int combatId, CreateCombatActionDto request, int userId);

    /// <summary>Updates the current HP of a participant.</summary>
    Task<CombatParticipantDto?> UpdateHpAsync(int combatId, int participantId, UpdateHpDto request, int userId);

    /// <summary>Advances to the next active participant's turn.</summary>
    Task<CombatDto?> NextTurnAsync(int combatId, int userId);

    /// <summary>Ends the combat encounter (Status = 3) with an optional victory side.</summary>
    Task<CombatDto?> EndCombatAsync(int combatId, EndCombatDto request, int userId);

    /// <summary>Toggles a participant's active status (active or eliminated).</summary>
    Task<CombatDto?> ToggleParticipantActiveAsync(int combatId, int participantId, SetActiveDto request, int userId);
}
