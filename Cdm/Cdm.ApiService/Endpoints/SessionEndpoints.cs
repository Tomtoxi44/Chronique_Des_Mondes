// -----------------------------------------------------------------------
// <copyright file="SessionEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Session endpoints for the API.
/// </summary>
public static class SessionEndpoints
{
    private sealed class SessionEndpointsLogger { }

    /// <summary>
    /// Maps session endpoints to the application.
    /// </summary>
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sessions")
            .WithTags("Sessions")
            .RequireAuthorization();

        // POST /api/sessions - Start a new session
        group.MapPost("/", StartSessionAsync)
            .WithName("StartSession")
            .WithOpenApi();

        // GET /api/sessions - Get my sessions
        group.MapGet("/", GetMySessionsAsync)
            .WithName("GetMySessions")
            .WithOpenApi();

        // GET /api/sessions/{id} - Get a specific session
        group.MapGet("/{id:int}", GetSessionAsync)
            .WithName("GetSession")
            .WithOpenApi();

        // GET /api/sessions/campaign/{campaignId}/active - Get active session for a campaign
        group.MapGet("/campaign/{campaignId:int}/active", GetActiveSessionByCampaignAsync)
            .WithName("GetActiveSessionByCampaign")
            .WithOpenApi();

        // PUT /api/sessions/{id}/end - End a session
        group.MapPut("/{id:int}/end", EndSessionAsync)
            .WithName("EndSession")
            .WithOpenApi();

        // POST /api/sessions/{id}/join - Join a session as a player
        group.MapPost("/{id:int}/join", JoinSessionAsync)
            .WithName("JoinSession")
            .WithOpenApi();

        // PUT /api/sessions/{id}/chapter - Update current chapter
        group.MapPut("/{id:int}/chapter", UpdateCurrentChapterAsync)
            .WithName("UpdateCurrentChapter")
            .WithOpenApi();

        // PUT /api/sessions/{id}/leave - Player leaves session
        group.MapPut("/{id:int}/leave", LeaveSessionAsync)
            .WithName("LeaveSession")
            .WithOpenApi();
    }

    private static async Task<IResult> StartSessionAsync(
        [FromBody] StartSessionDto dto,
        [FromServices] ISessionService sessionService,
        ILogger<SessionEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var session = await sessionService.StartSessionAsync(dto, userId.Value);
            if (session == null)
                return Results.BadRequest(new { Error = "Failed to start session. Check campaign ownership and active session status." });

            return Results.Created($"/api/sessions/{session.Id}", session);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting session");
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetMySessionsAsync(
        [FromServices] ISessionService sessionService,
        ILogger<SessionEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var sessions = await sessionService.GetMySessionsAsync(userId.Value);
            return Results.Ok(sessions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving sessions");
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetSessionAsync(
        int id,
        [FromServices] ISessionService sessionService,
        ILogger<SessionEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var session = await sessionService.GetSessionAsync(id, userId.Value);
            if (session == null) return Results.NotFound();

            return Results.Ok(session);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving session {SessionId}", id);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetActiveSessionByCampaignAsync(
        int campaignId,
        [FromServices] ISessionService sessionService,
        ILogger<SessionEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var session = await sessionService.GetActiveSessionByCampaignAsync(campaignId, userId.Value);
            if (session == null) return Results.NotFound();

            return Results.Ok(session);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving active session for campaign {CampaignId}", campaignId);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> EndSessionAsync(
        int id,
        [FromServices] ISessionService sessionService,
        ILogger<SessionEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var ended = await sessionService.EndSessionAsync(id, userId.Value);
            if (!ended) return Results.BadRequest(new { Error = "Failed to end session. Check session ownership." });

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ending session {SessionId}", id);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> JoinSessionAsync(
        int id,
        [FromBody] JoinSessionRequest request,
        [FromServices] ISessionService sessionService,
        ILogger<SessionEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var session = await sessionService.JoinSessionAsync(id, request.WorldCharacterId, userId.Value);
            if (session == null) return Results.BadRequest(new { Error = "Failed to join session." });

            return Results.Ok(session);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error joining session {SessionId}", id);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UpdateCurrentChapterAsync(
        int id,
        [FromBody] UpdateChapterRequest request,
        [FromServices] ISessionService sessionService,
        ILogger<SessionEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var session = await sessionService.UpdateCurrentChapterAsync(id, request.ChapterId, userId.Value);
            if (session == null) return Results.BadRequest(new { Error = "Failed to update chapter." });

            return Results.Ok(session);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating chapter for session {SessionId}", id);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> LeaveSessionAsync(
        int id,
        [FromServices] ISessionService sessionService,
        ILogger<SessionEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var left = await sessionService.LeaveSessionAsync(id, userId.Value);
            if (!left) return Results.BadRequest(new { Error = "Failed to leave session." });

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error leaving session {SessionId}", id);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var userId)) return null;
        return userId;
    }
}

internal record JoinSessionRequest(int WorldCharacterId);
internal record UpdateChapterRequest(int? ChapterId);
