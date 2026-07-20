// -----------------------------------------------------------------------
// <copyright file="StatisticsEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Statistics and reporting endpoints.
/// </summary>
public static class StatisticsEndpoints
{
    private sealed class StatisticsEndpointsLogger { }

    /// <summary>
    /// Maps statistics endpoints to the application.
    /// </summary>
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/statistics")
            .WithTags("Statistics")
            .RequireAuthorization();

        // GET /api/statistics/dice - dice statistics for the current user
        group.MapGet("/dice", GetMyDiceStatsAsync)
            .WithName("GetMyDiceStats")
            .WithOpenApi();

        // GET /api/statistics/participation - participation statistics for the current user
        group.MapGet("/participation", GetMyParticipationStatsAsync)
            .WithName("GetMyParticipationStats")
            .WithOpenApi();
    }

    private static async Task<IResult> GetMyParticipationStatsAsync(
        [FromServices] IStatisticsService statisticsService,
        ILogger<StatisticsEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var stats = await statisticsService.GetParticipationStatsForUserAsync(userId);
            return Results.Ok(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving participation statistics");
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetMyDiceStatsAsync(
        [FromServices] IStatisticsService statisticsService,
        ILogger<StatisticsEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var stats = await statisticsService.GetDiceStatsForUserAsync(userId);
            return Results.Ok(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving dice statistics");
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
