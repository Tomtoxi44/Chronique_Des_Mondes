// -----------------------------------------------------------------------
// <copyright file="AchievementEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Achievement endpoints for the API.
/// </summary>
public static class AchievementEndpoints
{
    /// <summary>
    /// Marker class for logging.
    /// </summary>
    private sealed class AchievementEndpointsLogger { }

    /// <summary>
    /// Maps achievement endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapAchievementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/achievements")
            .WithTags("Achievements")
            .RequireAuthorization();

        // POST /api/achievements - Create a new achievement
        group.MapPost("/", CreateAchievementAsync)
            .WithName("CreateAchievement")
            .WithOpenApi();

        // GET /api/achievements/world/{worldId} - Get achievements by world
        group.MapGet("/world/{worldId:int}", GetAchievementsByWorldAsync)
            .WithName("GetAchievementsByWorld")
            .WithOpenApi();

        // GET /api/achievements/campaign/{campaignId} - Get achievements by campaign
        group.MapGet("/campaign/{campaignId:int}", GetAchievementsByCampaignAsync)
            .WithName("GetAchievementsByCampaign")
            .WithOpenApi();

        // GET /api/achievements/chapter/{chapterId} - Get achievements by chapter
        group.MapGet("/chapter/{chapterId:int}", GetAchievementsByChapterAsync)
            .WithName("GetAchievementsByChapter")
            .WithOpenApi();

        // GET /api/achievements/{id} - Get a specific achievement
        group.MapGet("/{id:int}", GetAchievementByIdAsync)
            .WithName("GetAchievementById")
            .WithOpenApi();

        // PUT /api/achievements/{id} - Update an achievement
        group.MapPut("/{id:int}", UpdateAchievementAsync)
            .WithName("UpdateAchievement")
            .WithOpenApi();

        // DELETE /api/achievements/{id} - Delete an achievement
        group.MapDelete("/{id:int}", DeleteAchievementAsync)
            .WithName("DeleteAchievement")
            .WithOpenApi();

        // POST /api/achievements/{id}/award - Award an achievement to a user
        group.MapPost("/{id:int}/award", AwardAchievementAsync)
            .WithName("AwardAchievement")
            .WithOpenApi();

        // GET /api/achievements/user - Get current user's achievements
        group.MapGet("/user", GetMyAchievementsAsync)
            .WithName("GetMyAchievements")
            .WithOpenApi();

        // GET /api/achievements/user/{userId}/world/{worldId} - Get user achievements in a world
        group.MapGet("/user/{userId:int}/world/{worldId:int}", GetUserAchievementsInWorldAsync)
            .WithName("GetUserAchievementsInWorld")
            .WithOpenApi();

        // DELETE /api/achievements/user/{userAchievementId} - Revoke an achievement
        group.MapDelete("/user/{userAchievementId:int}", RevokeAchievementAsync)
            .WithName("RevokeAchievement")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateAchievementAsync(
        [FromBody] CreateAchievementDto request,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var result = await achievementService.CreateAchievementAsync(request, userId.Value);
        if (result == null)
            return Results.BadRequest(new { Error = "Failed to create achievement" });

        return Results.Created($"/api/achievements/{result.Id}", result);
    }

    private static async Task<IResult> GetAchievementsByWorldAsync(
        int worldId,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var achievements = await achievementService.GetAchievementsByWorldAsync(worldId, userId.Value);
        return Results.Ok(achievements);
    }

    private static async Task<IResult> GetAchievementsByCampaignAsync(
        int campaignId,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var achievements = await achievementService.GetAchievementsByCampaignAsync(campaignId, userId.Value);
        return Results.Ok(achievements);
    }

    private static async Task<IResult> GetAchievementsByChapterAsync(
        int chapterId,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var achievements = await achievementService.GetAchievementsByChapterAsync(chapterId, userId.Value);
        return Results.Ok(achievements);
    }

    private static async Task<IResult> GetAchievementByIdAsync(
        int id,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var achievement = await achievementService.GetAchievementByIdAsync(id, userId.Value);
        if (achievement == null)
            return Results.NotFound(new { Error = "Achievement not found or access denied" });

        return Results.Ok(achievement);
    }

    private static async Task<IResult> UpdateAchievementAsync(
        int id,
        [FromBody] CreateAchievementDto request,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var achievement = await achievementService.UpdateAchievementAsync(id, request, userId.Value);
        if (achievement == null)
            return Results.NotFound(new { Error = "Achievement not found or not authorized" });

        return Results.Ok(achievement);
    }

    private static async Task<IResult> DeleteAchievementAsync(
        int id,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var success = await achievementService.DeleteAchievementAsync(id, userId.Value);
        if (!success)
            return Results.NotFound(new { Error = "Achievement not found or not authorized" });

        return Results.NoContent();
    }

    private static async Task<IResult> AwardAchievementAsync(
        int id,
        [FromBody] AwardAchievementRequest request,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var userAchievement = await achievementService.AwardAchievementAsync(id, request.TargetUserId, userId.Value, request.Message);
        if (userAchievement == null)
            return Results.BadRequest(new { Error = "Failed to award achievement" });

        return Results.Ok(userAchievement);
    }

    private static async Task<IResult> GetMyAchievementsAsync(
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var achievements = await achievementService.GetUserAchievementsAsync(userId.Value);
        return Results.Ok(achievements);
    }

    private static async Task<IResult> GetUserAchievementsInWorldAsync(
        int userId,
        int worldId,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var currentUserId = GetUserId(httpContext);
        if (currentUserId == null) return Results.Unauthorized();

        var achievements = await achievementService.GetUserAchievementsInWorldAsync(userId, worldId);
        return Results.Ok(achievements);
    }

    private static async Task<IResult> RevokeAchievementAsync(
        int userAchievementId,
        [FromServices] IAchievementService achievementService,
        ILogger<AchievementEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();

        var success = await achievementService.RevokeAchievementAsync(userAchievementId, userId.Value);
        if (!success)
            return Results.NotFound(new { Error = "User achievement not found or not authorized" });

        return Results.NoContent();
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;
        return userId;
    }

    /// <summary>
    /// Request model for awarding an achievement.
    /// </summary>
    public record AwardAchievementRequest(int TargetUserId, string? Message);
}
