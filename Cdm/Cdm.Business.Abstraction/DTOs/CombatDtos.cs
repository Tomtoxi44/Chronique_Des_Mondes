// -----------------------------------------------------------------------
// <copyright file="CombatDtos.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

/// <summary>Request DTO to create a new combat encounter.</summary>
public class CreateCombatDto
{
    public int SessionId { get; set; }
    public int? ChapterId { get; set; }
    public List<CreateCombatGroupDto> Groups { get; set; } = new();
}

/// <summary>Request DTO to create a group within a combat.</summary>
public class CreateCombatGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
    public List<CreateParticipantDto> Participants { get; set; } = new();
}

/// <summary>Request DTO to add a participant to a combat group.</summary>
public class CreateParticipantDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsPlayerCharacter { get; set; }
    public int? CharacterId { get; set; }
    public int? NpcId { get; set; }
    public int? UserId { get; set; }
    public int MaxHp { get; set; }
}

/// <summary>Request DTO to start the initiative phase.</summary>
public class StartInitiativeDto
{
    /// <summary>Mode: "manual", "declare", or "roll".</summary>
    public string Mode { get; set; } = "manual";

    /// <summary>Dice expression used in "roll" mode (e.g. "1d20", "1d6").</summary>
    public string? DiceExpression { get; set; }
}

/// <summary>Request DTO to set a participant's initiative value.</summary>
public class SetInitiativeDto
{
    public int Value { get; set; }
}

/// <summary>Request DTO to update a participant's current HP.</summary>
public class UpdateHpDto
{
    public int NewHp { get; set; }
}

/// <summary>Request DTO to record a combat action.</summary>
public class CreateCombatActionDto
{
    public string ParticipantName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DiceExpression { get; set; }
    public int? DiceResult { get; set; }
    public bool IsPrivate { get; set; }
}

/// <summary>Request DTO to end a combat encounter.</summary>
public class EndCombatDto
{
    public string? VictorySide { get; set; }
}

// ── Read DTOs ────────────────────────────────────────────────────────────────

/// <summary>Full read DTO for a combat encounter.</summary>
public class CombatDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int? ChapterId { get; set; }
    public int Status { get; set; }
    public int CurrentTurnOrder { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? VictorySide { get; set; }
    public List<CombatGroupDto> Groups { get; set; } = new();
    public List<CombatParticipantDto> Participants { get; set; } = new();
    public List<CombatActionDto> Actions { get; set; } = new();
}

/// <summary>Read DTO for a combat group.</summary>
public class CombatGroupDto
{
    public int Id { get; set; }
    public int CombatId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

/// <summary>Read DTO for a combat participant.</summary>
public class CombatParticipantDto
{
    public int Id { get; set; }
    public int CombatId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupColor { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPlayerCharacter { get; set; }
    public int? CharacterId { get; set; }
    public int? NpcId { get; set; }
    public int? UserId { get; set; }
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int? Initiative { get; set; }
    public int TurnOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Read DTO for a combat action entry.</summary>
public class CombatActionDto
{
    public int Id { get; set; }
    public int CombatId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DiceExpression { get; set; }
    public int? DiceResult { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime CreatedAt { get; set; }
}
