// -----------------------------------------------------------------------
// <copyright file="WorldEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.ApiService.Endpoints.Models;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// World endpoints for the API.
/// </summary>
public static class WorldEndpoints
{
    /// <summary>
    /// Marker class for logging.
    /// </summary>
    private sealed class WorldEndpointsLogger { }
    /// <summary>
    /// Maps world endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapWorldEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/worlds")
            .WithTags("Worlds")
            .RequireAuthorization();

        // POST /api/worlds - Create a new world
        group.MapPost("/", CreateWorldAsync)
            .WithName("CreateWorld")
            .WithOpenApi();

        // GET /api/worlds - Get all my worlds (as GM)
        group.MapGet("/", GetMyWorldsAsync)
            .WithName("GetMyWorlds")
            .WithOpenApi();

        // GET /api/worlds/all - Get all worlds I participate in
        group.MapGet("/all", GetAllMyWorldsAsync)
            .WithName("GetAllMyWorlds")
            .WithOpenApi();

        // GET /api/worlds/{id} - Get a specific world
        group.MapGet("/{id:int}", GetWorldByIdAsync)
            .WithName("GetWorldById")
            .WithOpenApi();

        // PUT /api/worlds/{id} - Update a world
        group.MapPut("/{id:int}", UpdateWorldAsync)
            .WithName("UpdateWorld")
            .WithOpenApi();

        // DELETE /api/worlds/{id} - Delete a world
        group.MapDelete("/{id:int}", DeleteWorldAsync)
            .WithName("DeleteWorld")
            .WithOpenApi();

        // GET /api/worlds/{id}/characters - Get characters in a world
        group.MapGet("/{id:int}/characters", GetWorldCharactersAsync)
            .WithName("GetWorldCharacters")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateWorldAsync(
        [FromBody] CreateWorldRequest request,
        [FromServices] IWorldService worldService,
        ILogger<CreateWorldRequest> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            logger.LogInformation("Creating world '{WorldName}' for user {UserId}", request.Name, userId);

            var createDto = new CreateWorldDto
            {
                Name = request.Name,
                Description = request.Description,
                GameType = request.GameType
            };

            var worldDto = await worldService.CreateWorldAsync(createDto, userId.Value);

            if (worldDto == null)
            {
                logger.LogWarning("Failed to create world '{WorldName}' for user {UserId}", request.Name, userId);
                return Results.BadRequest(new { Error = "Failed to create world. Please check your input and try again." });
            }

            logger.LogInformation("Successfully created world {WorldId} for user {UserId}", worldDto.Id, userId);

            return Results.Created($"/api/worlds/{worldDto.Id}", worldDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating world");
            return Results.Problem(
                title: "An error occurred while creating the world",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetMyWorldsAsync(
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var worlds = await worldService.GetMyWorldsAsync(userId.Value);
            return Results.Ok(worlds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving worlds");
            return Results.Problem(
                title: "An error occurred while retrieving worlds",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetAllMyWorldsAsync(
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var worlds = await worldService.GetWorldsForUserAsync(userId.Value);
            return Results.Ok(worlds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all worlds");
            return Results.Problem(
                title: "An error occurred while retrieving worlds",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetWorldByIdAsync(
        int id,
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var world = await worldService.GetWorldByIdAsync(id, userId.Value);
            if (world == null)
            {
                return Results.NotFound(new { Error = "World not found or access denied" });
            }

            return Results.Ok(world);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving world {WorldId}", id);
            return Results.Problem(
                title: "An error occurred while retrieving the world",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UpdateWorldAsync(
        int id,
        [FromBody] CreateWorldRequest request,
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var updateDto = new CreateWorldDto
            {
                Name = request.Name,
                Description = request.Description,
                GameType = request.GameType
            };

            var world = await worldService.UpdateWorldAsync(id, updateDto, userId.Value);
            if (world == null)
            {
                return Results.NotFound(new { Error = "World not found or not authorized" });
            }

            return Results.Ok(world);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating world {WorldId}", id);
            return Results.Problem(
                title: "An error occurred while updating the world",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> DeleteWorldAsync(
        int id,
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var success = await worldService.DeleteWorldAsync(id, userId.Value);
            if (!success)
            {
                return Results.NotFound(new { Error = "World not found or not authorized" });
            }

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting world {WorldId}", id);
            return Results.Problem(
                title: "An error occurred while deleting the world",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetWorldCharactersAsync(
        int id,
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var characters = await worldService.GetWorldCharactersAsync(id, userId.Value);
            return Results.Ok(characters);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving characters for world {WorldId}", id);
            return Results.Problem(
                title: "An error occurred while retrieving world characters",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        return userId;
    }
}
