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
