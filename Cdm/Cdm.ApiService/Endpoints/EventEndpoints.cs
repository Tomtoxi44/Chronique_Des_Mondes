// -----------------------------------------------------------------------
// <copyright file="EventEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Event endpoints for the API.
/// </summary>
public static class EventEndpoints
{
    /// <summary>
    /// Marker class for logging.
    /// </summary>
    private sealed class EventEndpointsLogger { }

    /// <summary>
    /// Maps event endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events")
            .WithTags("Events")
            .RequireAuthorization();

        // POST /api/events - Create a new event
        group.MapPost("/", CreateEventAsync)
            .WithName("CreateEvent")
            .WithOpenApi();

        // GET /api/events/world/{worldId} - Get events by world
        group.MapGet("/world/{worldId:int}", GetEventsByWorldAsync)
            .WithName("GetEventsByWorld")
            .WithOpenApi();

        // GET /api/events/campaign/{campaignId} - Get events by campaign
        group.MapGet("/campaign/{campaignId:int}", GetEventsByCampaignAsync)
            .WithName("GetEventsByCampaign")
            .WithOpenApi();

        // GET /api/events/chapter/{chapterId} - Get events by chapter
        group.MapGet("/chapter/{chapterId:int}", GetEventsByChapterAsync)
            .WithName("GetEventsByChapter")
            .WithOpenApi();

        // GET /api/events/{id} - Get a specific event
        group.MapGet("/{id:int}", GetEventByIdAsync)
            .WithName("GetEventById")
            .WithOpenApi();

        // PUT /api/events/{id} - Update an event
        group.MapPut("/{id:int}", UpdateEventAsync)
            .WithName("UpdateEvent")
            .WithOpenApi();

        // DELETE /api/events/{id} - Delete an event
        group.MapDelete("/{id:int}", DeleteEventAsync)
            .WithName("DeleteEvent")
            .WithOpenApi();

        // PUT /api/events/{id}/active - Toggle event active status
        group.MapPut("/{id:int}/active", SetEventActiveAsync)
            .WithName("SetEventActive")
            .WithOpenApi();

        // POST /api/events/{id}/permanent - Mark event as permanent
        group.MapPost("/{id:int}/permanent", MarkAsPermanentAsync)
            .WithName("MarkEventAsPermanent")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateEventAsync(
        [FromBody] CreateEventDto request,
        [FromServices] IEventService eventService,
        ILogger<EventEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await eventService.CreateEventAsync(request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to create event" });

        return Results.Created($"/api/events/{result.Id}", result);
    }

    private static async Task<IResult> GetEventsByWorldAsync(
        int worldId,
        [FromServices] IEventService eventService,
        ILogger<EventEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var events = await eventService.GetEventsByWorldAsync(worldId, userId.Value);
        return Results.Ok(events);
    }

    private static async Task<IResult> GetEventsByCampaignAsync(
        int campaignId,
        [FromServices] IEventService eventService,
        ILogger<EventEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var events = await eventService.GetEventsByCampaignAsync(campaignId, userId.Value);
        return Results.Ok(events);
    }

    private static async Task<IResult> GetEventsByChapterAsync(
        int chapterId,
        [FromServices] IEventService eventService,
        ILogger<EventEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var events = await eventService.GetEventsByChapterAsync(chapterId, userId.Value);
        return Results.Ok(events);
    }

    private static async Task<IResult> GetEventByIdAsync(
        int id,
        [FromServices] IEventService eventService,
        ILogger<EventEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var eventDto = await eventService.GetEventByIdAsync(id, userId.Value);
        if (eventDto == null)
            return Results.NotFound(new { Error = "Event not found or access denied" });

        return Results.Ok(eventDto);
    }

    private static async Task<IResult> UpdateEventAsync(
        int id,
        [FromBody] CreateEventDto request,
        [FromServices] IEventService eventService,
        ILogger<EventEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var eventDto = await eventService.UpdateEventAsync(id, request, userId.Value);
        if (eventDto == null)
            return Results.NotFound(new { Error = "Event not found or not authorized" });

        return Results.Ok(eventDto);
    }

    private static async Task<IResult> DeleteEventAsync(
        int id,
        [FromServices] IEventService eventService,
        ILogger<EventEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var success = await eventService.DeleteEventAsync(id, userId.Value);
        if (!success)
            return Results.NotFound(new { Error = "Event not found or not authorized" });

        return Results.NoContent();
    }

    private static async Task<IResult> SetEventActiveAsync(
        int id,
        [FromBody] SetActiveRequest request,
        [FromServices] IEventService eventService,
        ILogger<EventEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var eventDto = await eventService.SetEventActiveAsync(id, request.IsActive, userId.Value);
        if (eventDto == null)
            return Results.NotFound(new { Error = "Event not found or not authorized" });

        return Results.Ok(eventDto);
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;
        return userId;
    }

    private static async Task<IResult> MarkAsPermanentAsync(
        int id,
        [FromServices] IEventService eventService,
        ILogger<EventEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await eventService.MarkAsPermanentAsync(id, userId.Value);
        if (result == null)
            return Results.NotFound(new { Error = "Event not found or unauthorized" });

        return Results.Ok(result);
    }

    /// <summary>
    /// Request model for setting active status.
    /// </summary>
    public record SetActiveRequest(bool IsActive);
}
