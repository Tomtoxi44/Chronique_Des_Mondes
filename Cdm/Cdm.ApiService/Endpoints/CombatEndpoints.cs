// -----------------------------------------------------------------------
// <copyright file="CombatEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// REST endpoints for managing combat encounters.
/// </summary>
public static class CombatEndpoints
{
    private sealed class CombatEndpointsLogger { }

    /// <summary>
    /// Maps combat endpoints to the application.
    /// </summary>
    public static void MapCombatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/combat")
            .WithTags("Combat")
            .RequireAuthorization();

        // POST /api/combat — Create a combat encounter
        group.MapPost("/", CreateCombatAsync)
            .WithName("CreateCombat")
            .WithOpenApi();

        // GET /api/combat/{id} — Get a combat by ID
        group.MapGet("/{id:int}", GetCombatAsync)
            .WithName("GetCombat")
            .WithOpenApi();

        // GET /api/combat/session/{sessionId}/active — Get the active combat for a session
        group.MapGet("/session/{sessionId:int}/active", GetActiveCombatAsync)
            .WithName("GetActiveCombat")
            .WithOpenApi();

        // POST /api/combat/{id}/initiative/start — Transition to initiative phase
        group.MapPost("/{id:int}/initiative/start", StartInitiativePhaseAsync)
            .WithName("StartInitiativePhase")
            .WithOpenApi();

        // PUT /api/combat/{id}/initiative/{participantId} — Set a participant's initiative
        group.MapPut("/{id:int}/initiative/{participantId:int}", SetInitiativeAsync)
            .WithName("SetInitiative")
            .WithOpenApi();

        // POST /api/combat/{id}/start — Start the active combat phase
        group.MapPost("/{id:int}/start", StartCombatAsync)
            .WithName("StartCombat")
            .WithOpenApi();

        // POST /api/combat/{id}/action — Record a combat action
        group.MapPost("/{id:int}/action", RecordActionAsync)
            .WithName("RecordCombatAction")
            .WithOpenApi();

        // PUT /api/combat/{id}/hp/{participantId} — Update participant HP
        group.MapPut("/{id:int}/hp/{participantId:int}", UpdateHpAsync)
            .WithName("UpdateCombatHp")
            .WithOpenApi();

        // POST /api/combat/{id}/next-turn — Advance to the next turn
        group.MapPost("/{id:int}/next-turn", NextTurnAsync)
            .WithName("NextCombatTurn")
            .WithOpenApi();

        // POST /api/combat/{id}/end — End the combat encounter
        group.MapPost("/{id:int}/end", EndCombatAsync)
            .WithName("EndCombat")
            .WithOpenApi();

        // PUT /api/combat/{id}/participants/{participantId}/active — Toggle participant active status
        group.MapPut("/{id:int}/participants/{participantId:int}/active", ToggleParticipantActiveAsync)
            .WithName("ToggleParticipantActive")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateCombatAsync(
        [FromBody] CreateCombatDto request,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.CreateCombatAsync(request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to create combat. Check session ID and authorization." });

        return Results.Created($"/api/combat/{result.Id}", result);
    }

    private static async Task<IResult> GetCombatAsync(
        int id,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.GetCombatAsync(id, userId.Value);
        if (result == null)
            return Results.NotFound(new { Error = "Combat not found or access denied." });

        return Results.Ok(result);
    }

    private static async Task<IResult> GetActiveCombatAsync(
        int sessionId,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.GetActiveCombatForSessionAsync(sessionId, userId.Value);
        if (result == null)
            return Results.NotFound(new { Error = "No active combat found for this session." });

        return Results.Ok(result);
    }

    private static async Task<IResult> StartInitiativePhaseAsync(
        int id,
        [FromBody] StartInitiativeDto request,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.StartInitiativePhaseAsync(id, request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to start initiative phase. Check combat ID and authorization." });

        return Results.Ok(result);
    }

    private static async Task<IResult> SetInitiativeAsync(
        int id,
        int participantId,
        [FromBody] SetInitiativeDto request,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.SetInitiativeAsync(id, participantId, request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to set initiative. Check participant ID and authorization." });

        return Results.Ok(result);
    }

    private static async Task<IResult> StartCombatAsync(
        int id,
        HttpRequest req,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        StartCombatDto? request = null;
        if (req.ContentLength > 0)
        {
            try { request = await req.ReadFromJsonAsync<StartCombatDto>(); } catch { }
        }

        var result = await combatService.StartCombatAsync(id, request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to start combat. Check combat ID and authorization." });

        return Results.Ok(result);
    }

    private static async Task<IResult> RecordActionAsync(
        int id,
        [FromBody] CreateCombatActionDto request,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.RecordActionAsync(id, request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to record action. Check combat ID." });

        return Results.Created($"/api/combat/{id}/action/{result.Id}", result);
    }

    private static async Task<IResult> UpdateHpAsync(
        int id,
        int participantId,
        [FromBody] UpdateHpDto request,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.UpdateHpAsync(id, participantId, request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to update HP. Check participant ID and authorization." });

        return Results.Ok(result);
    }

    private static async Task<IResult> NextTurnAsync(
        int id,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.NextTurnAsync(id, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to advance turn. Check combat ID and authorization." });

        return Results.Ok(result);
    }

    private static async Task<IResult> EndCombatAsync(
        int id,
        [FromBody] EndCombatDto request,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.EndCombatAsync(id, request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to end combat. Check combat ID and authorization." });

        return Results.Ok(result);
    }

    private static async Task<IResult> ToggleParticipantActiveAsync(
        int id,
        int participantId,
        [FromBody] SetActiveDto request,
        [FromServices] ICombatService combatService,
        ILogger<CombatEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await combatService.ToggleParticipantActiveAsync(id, participantId, request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to toggle participant active status." });

        return Results.Ok(result);
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var userId)) return null;
        return userId;
    }
}
