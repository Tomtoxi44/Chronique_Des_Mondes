// -----------------------------------------------------------------------
// <copyright file="ChapterEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Chapter endpoints for the API.
/// </summary>
public static class ChapterEndpoints
{
    /// <summary>
    /// Marker class for logging.
    /// </summary>
    private sealed class ChapterEndpointsLogger { }

    /// <summary>
    /// Maps chapter endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapChapterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chapters")
            .WithTags("Chapters")
            .RequireAuthorization();

        // POST /api/chapters - Create a new chapter
        group.MapPost("/", CreateChapterAsync)
            .WithName("CreateChapter")
            .WithOpenApi();

        // GET /api/chapters/campaign/{campaignId} - Get chapters by campaign
        group.MapGet("/campaign/{campaignId:int}", GetChaptersByCampaignAsync)
            .WithName("GetChaptersByCampaign")
            .WithOpenApi();

        // GET /api/chapters/{id} - Get a specific chapter
        group.MapGet("/{id:int}", GetChapterByIdAsync)
            .WithName("GetChapterById")
            .WithOpenApi();

        // PUT /api/chapters/{id} - Update a chapter
        group.MapPut("/{id:int}", UpdateChapterAsync)
            .WithName("UpdateChapter")
            .WithOpenApi();

        // DELETE /api/chapters/{id} - Delete a chapter
        group.MapDelete("/{id:int}", DeleteChapterAsync)
            .WithName("DeleteChapter")
            .WithOpenApi();

        // POST /api/chapters/{id}/start - Start a chapter
        group.MapPost("/{id:int}/start", StartChapterAsync)
            .WithName("StartChapter")
            .WithOpenApi();

        // POST /api/chapters/{id}/complete - Complete a chapter
        group.MapPost("/{id:int}/complete", CompleteChapterAsync)
            .WithName("CompleteChapter")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateChapterAsync(
        [FromBody] CreateChapterDto request,
        [FromServices] IChapterService chapterService,
        ILogger<ChapterEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await chapterService.CreateChapterAsync(request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to create chapter" });

        return Results.Created($"/api/chapters/{result.Id}", result);
    }

    private static async Task<IResult> GetChaptersByCampaignAsync(
        int campaignId,
        [FromServices] IChapterService chapterService,
        ILogger<ChapterEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var chapters = await chapterService.GetChaptersByCampaignAsync(campaignId, userId.Value);
        return Results.Ok(chapters);
    }

    private static async Task<IResult> GetChapterByIdAsync(
        int id,
        [FromServices] IChapterService chapterService,
        ILogger<ChapterEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var chapter = await chapterService.GetChapterByIdAsync(id, userId.Value);
        if (chapter == null)
            return Results.NotFound(new { Error = "Chapter not found or access denied" });

        return Results.Ok(chapter);
    }

    private static async Task<IResult> UpdateChapterAsync(
        int id,
        [FromBody] CreateChapterDto request,
        [FromServices] IChapterService chapterService,
        ILogger<ChapterEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var chapter = await chapterService.UpdateChapterAsync(id, request, userId.Value);
        if (chapter == null)
            return Results.NotFound(new { Error = "Chapter not found or not authorized" });

        return Results.Ok(chapter);
    }

    private static async Task<IResult> DeleteChapterAsync(
        int id,
        [FromServices] IChapterService chapterService,
        ILogger<ChapterEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var success = await chapterService.DeleteChapterAsync(id, userId.Value);
        if (!success)
            return Results.NotFound(new { Error = "Chapter not found or not authorized" });

        return Results.NoContent();
    }

    private static async Task<IResult> StartChapterAsync(
        int id,
        [FromServices] IChapterService chapterService,
        ILogger<ChapterEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var chapter = await chapterService.StartChapterAsync(id, userId.Value);
        if (chapter == null)
            return Results.NotFound(new { Error = "Chapter not found or not authorized" });

        return Results.Ok(chapter);
    }

    private static async Task<IResult> CompleteChapterAsync(
        int id,
        [FromServices] IChapterService chapterService,
        ILogger<ChapterEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var chapter = await chapterService.CompleteChapterAsync(id, userId.Value);
        if (chapter == null)
            return Results.NotFound(new { Error = "Chapter not found or not authorized" });

        return Results.Ok(chapter);
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;
        return userId;
    }
}
