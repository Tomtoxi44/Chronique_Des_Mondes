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

        // DELETE /api/worlds/{id}/characters/{characterId} - Remove character from world
        group.MapDelete("/{id:int}/characters/{characterId:int}", RemoveCharacterFromWorldAsync)
            .WithName("RemoveCharacterFromWorld")
            .WithOpenApi();

        // POST /api/worlds/{id}/invite - Generate invite token (GM only)
        group.MapPost("/{id:int}/invite", GenerateWorldInviteTokenAsync)
            .WithName("GenerateWorldInviteToken")
            .WithOpenApi();

        // GET /api/worlds/join/{token} - Get world info from invite token (no auth required)
        app.MapGet("/api/worlds/join/{token}", GetWorldByInviteTokenAsync)
            .WithTags("Worlds")
            .WithName("GetWorldByInviteToken")
            .WithOpenApi();

        // POST /api/worlds/join - Join a world with a character
        group.MapPost("/join", JoinWorldAsync)
            .WithName("JoinWorld")
            .WithOpenApi();

        // GET /api/worlds/{id}/my-character - Get current user's world character
        group.MapGet("/{id:int}/my-character", GetMyWorldCharacterAsync)
            .WithName("GetMyWorldCharacter")
            .WithOpenApi();

        // PUT /api/worlds/{id}/my-character - Update current user's world character profile
        group.MapPut("/{id:int}/my-character", UpdateMyWorldCharacterAsync)
            .WithName("UpdateMyWorldCharacter")
            .WithOpenApi();

        // GET /api/worlds/{id}/campaigns - Get campaigns for a world (visible to members)
        group.MapGet("/{id:int}/campaigns", GetWorldCampaignsForMemberAsync)
            .WithName("GetWorldCampaignsForMember")
            .WithOpenApi();

        // GET /api/worlds/my-character/{wcId} - Get a world character by its ID (for the current user)
        group.MapGet("/my-character/{wcId:int}", GetWorldCharacterByIdAsync)
            .WithName("GetWorldCharacterById")
            .WithOpenApi();
    }

    private static async Task<IResult> GetWorldCharacterByIdAsync(
        int wcId,
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        if (userId == null) return Results.Unauthorized();
        var wc = await worldService.GetWorldCharacterByIdAsync(wcId, userId.Value);
        return wc is null ? Results.NotFound() : Results.Ok(wc);
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

    private static async Task<IResult> RemoveCharacterFromWorldAsync(
        int id,
        int characterId,
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

            logger.LogInformation(
                "User {UserId} removing character {CharacterId} from world {WorldId}",
                userId, characterId, id);

            var success = await worldService.RemoveCharacterFromWorldAsync(id, characterId, userId.Value);

            if (!success)
            {
                return Results.NotFound(new { Error = "Character not found in world or not authorized" });
            }

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing character {CharacterId} from world {WorldId}", characterId, id);
            return Results.Problem(
                title: "An error occurred while removing the character from the world",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GenerateWorldInviteTokenAsync(
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

            var token = await worldService.GenerateWorldInviteTokenAsync(id, userId.Value);
            if (token == null)
            {
                return Results.NotFound(new { Error = "World not found or not authorized" });
            }

            return Results.Ok(new { InviteToken = token });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating invite token for world {WorldId}", id);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetWorldByInviteTokenAsync(
        string token,
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger)
    {
        try
        {
            var world = await worldService.GetWorldByInviteTokenAsync(token);
            if (world == null)
            {
                return Results.NotFound(new { Error = "Invalid or expired invite token" });
            }

            return Results.Ok(world);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving world by invite token");
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> JoinWorldAsync(
        [FromBody] JoinWorldRequest request,
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

            var worldCharacter = await worldService.JoinWorldAsync(request.InviteToken, request.CharacterId, userId.Value);
            if (worldCharacter == null)
            {
                return Results.BadRequest(new { Error = "Invalid token, expired, character locked, or already in world" });
            }

            return Results.Ok(worldCharacter);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error joining world");
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
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

    private static async Task<IResult> GetMyWorldCharacterAsync(
        int id,
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var worldChar = await worldService.GetMyWorldCharacterAsync(id, userId.Value);
            if (worldChar == null) return Results.NotFound();

            return Results.Ok(worldChar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving world character for world {WorldId}", id);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UpdateMyWorldCharacterAsync(
        int id,
        [FromBody] UpdateWorldCharacterProfileRequest request,
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var dto = new Cdm.Business.Abstraction.DTOs.UpdateWorldCharacterProfileDto
            {
                Level = request.Level,
                CurrentHealth = request.CurrentHealth,
                MaxHealth = request.MaxHealth,
                GameSpecificData = request.GameSpecificData
            };

            var worldChar = await worldService.UpdateMyWorldCharacterAsync(id, dto, userId.Value);
            if (worldChar == null) return Results.NotFound();

            return Results.Ok(worldChar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating world character for world {WorldId}", id);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetWorldCampaignsForMemberAsync(
        int id,
        [FromServices] IWorldService worldService,
        ILogger<WorldEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var campaigns = await worldService.GetWorldCampaignsForMemberAsync(id, userId.Value);
            return Results.Ok(campaigns);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving campaigns for world {WorldId}", id);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
