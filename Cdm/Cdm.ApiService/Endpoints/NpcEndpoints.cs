// -----------------------------------------------------------------------
// <copyright file="NpcEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// NPC (Non-Player Character) endpoints for the API.
/// </summary>
public static class NpcEndpoints
{
    private sealed class NpcEndpointsLogger { }

    /// <summary>
    /// Maps NPC endpoints to the application.
    /// </summary>
    public static void MapNpcEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/npcs")
            .WithTags("NPCs")
            .RequireAuthorization();

        // POST /api/npcs — Create an NPC
        group.MapPost("/", CreateNpcAsync)
            .WithName("CreateNpc")
            .WithOpenApi();

        // GET /api/npcs/chapter/{chapterId} — Get all NPCs for a chapter
        group.MapGet("/chapter/{chapterId:int}", GetNpcsByChapterAsync)
            .WithName("GetNpcsByChapter")
            .WithOpenApi();

        // GET /api/npcs/{id} — Get a specific NPC
        group.MapGet("/{id:int}", GetNpcByIdAsync)
            .WithName("GetNpcById")
            .WithOpenApi();

        // PUT /api/npcs/{id} — Update an NPC
        group.MapPut("/{id:int}", UpdateNpcAsync)
            .WithName("UpdateNpc")
            .WithOpenApi();

        // DELETE /api/npcs/{id} — Delete an NPC
        group.MapDelete("/{id:int}", DeleteNpcAsync)
            .WithName("DeleteNpc")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateNpcAsync(
        [FromBody] CreateNpcDto request,
        [FromServices] INpcService npcService,
        ILogger<NpcEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await npcService.CreateNpcAsync(request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to create NPC. Check chapter ID and authorization." });

        return Results.Created($"/api/npcs/{result.Id}", result);
    }

    private static async Task<IResult> GetNpcsByChapterAsync(
        int chapterId,
        [FromServices] INpcService npcService,
        ILogger<NpcEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var npcs = await npcService.GetNpcsByChapterAsync(chapterId, userId.Value);
        return Results.Ok(npcs);
    }

    private static async Task<IResult> GetNpcByIdAsync(
        int id,
        [FromServices] INpcService npcService,
        ILogger<NpcEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var npc = await npcService.GetNpcByIdAsync(id, userId.Value);
        if (npc == null)
            return Results.NotFound(new { Error = "NPC not found or access denied." });

        return Results.Ok(npc);
    }

    private static async Task<IResult> UpdateNpcAsync(
        int id,
        [FromBody] CreateNpcDto request,
        [FromServices] INpcService npcService,
        ILogger<NpcEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await npcService.UpdateNpcAsync(id, request, userId.Value);
        if (result == null)
            return Results.NotFound(new { Error = "NPC not found or not authorized." });

        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteNpcAsync(
        int id,
        [FromServices] INpcService npcService,
        ILogger<NpcEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var success = await npcService.DeleteNpcAsync(id, userId.Value);
        if (!success)
            return Results.NotFound(new { Error = "NPC not found or not authorized." });

        return Results.NoContent();
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var userId)) return null;
        return userId;
    }
}
