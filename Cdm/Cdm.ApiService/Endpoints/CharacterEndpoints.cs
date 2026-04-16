// -----------------------------------------------------------------------
// <copyright file="CharacterEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

/// <summary>
/// Character endpoints for the API.
/// </summary>
public static class CharacterEndpoints
{
    /// <summary>
    /// Maps character endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapCharacterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/characters")
            .WithTags("Characters")
            .RequireAuthorization();

        // GET /api/characters
        group.MapGet("/", GetMyCharactersAsync)
            .WithName("GetMyCharacters")
            .WithOpenApi();

        // GET /api/characters/{id}
        group.MapGet("/{id:int}", GetCharacterByIdAsync)
            .WithName("GetCharacterById")
            .WithOpenApi();

        // POST /api/characters
        group.MapPost("/", CreateCharacterAsync)
            .WithName("CreateCharacter")
            .WithOpenApi();

        // PUT /api/characters/{id}
        group.MapPut("/{id:int}", UpdateCharacterAsync)
            .WithName("UpdateCharacter")
            .WithOpenApi();

        // DELETE /api/characters/{id}
        group.MapDelete("/{id:int}", DeleteCharacterAsync)
            .WithName("DeleteCharacter")
            .WithOpenApi();

        // POST /api/characters/{id}/game-profile
        group.MapPost("/{id:int}/game-profile", CreateGameProfileAsync)
            .WithName("CreateGameProfile")
            .WithOpenApi();

        // GET /api/characters/{id}/game-profile
        group.MapGet("/{id:int}/game-profile", GetGameProfileAsync)
            .WithName("GetGameProfile")
            .WithOpenApi();

        // PUT /api/characters/{id}/game-profile
        group.MapPut("/{id:int}/game-profile", UpdateGameProfileAsync)
            .WithName("UpdateGameProfile")
            .WithOpenApi();
    }

    /// <summary>
    /// Gets all characters for the current user.
    /// </summary>
    private static async Task<IResult> GetMyCharactersAsync(
        [FromServices] ICharacterService characterService,
        ILogger<ICharacterService> logger,
        HttpContext httpContext)
    {
        try
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            logger.LogInformation("Fetching characters for user {UserId}", userId);

            var characters = await characterService.GetMyCharactersAsync(userId);

            return Results.Ok(characters);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching characters");
            return Results.Problem("An error occurred while fetching characters");
        }
    }

    /// <summary>
    /// Gets a specific character by ID.
    /// </summary>
    private static async Task<IResult> GetCharacterByIdAsync(
        [FromRoute] int id,
        [FromServices] ICharacterService characterService,
        ILogger<ICharacterService> logger,
        HttpContext httpContext)
    {
        try
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            logger.LogInformation("Fetching character {CharacterId} for user {UserId}", id, userId);

            var character = await characterService.GetCharacterByIdAsync(id, userId);

            if (character == null)
            {
                logger.LogWarning("Character {CharacterId} not found for user {UserId}", id, userId);
                return Results.NotFound(new { Error = "Character not found" });
            }

            return Results.Ok(character);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching character {CharacterId}", id);
            return Results.Problem("An error occurred while fetching the character");
        }
    }

    /// <summary>
    /// Creates a new character.
    /// </summary>
    private static async Task<IResult> CreateCharacterAsync(
        [FromBody] CreateCharacterDto request,
        [FromServices] ICharacterService characterService,
        ILogger<ICharacterService> logger,
        HttpContext httpContext)
    {
        try
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            logger.LogInformation("Creating character '{CharacterName}' for user {UserId}", request.Name, userId);

            var character = await characterService.CreateCharacterAsync(request, userId);

            if (character == null)
            {
                logger.LogWarning("Failed to create character '{CharacterName}' for user {UserId}", request.Name, userId);
                return Results.BadRequest(new { Error = "Failed to create character. Please check your input and try again." });
            }

            logger.LogInformation("Successfully created character {CharacterId} for user {UserId}", character.Id, userId);

            return Results.Created($"/api/characters/{character.Id}", character);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating character for user");
            return Results.Problem("An error occurred while creating the character");
        }
    }

    /// <summary>
    /// Updates an existing character.
    /// </summary>
    private static async Task<IResult> UpdateCharacterAsync(
        [FromRoute] int id,
        [FromBody] UpdateCharacterDto request,
        [FromServices] ICharacterService characterService,
        ILogger<ICharacterService> logger,
        HttpContext httpContext)
    {
        try
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            logger.LogInformation("Updating character {CharacterId} for user {UserId}", id, userId);

            var character = await characterService.UpdateCharacterAsync(id, request, userId);

            if (character == null)
            {
                logger.LogWarning("Failed to update character {CharacterId} for user {UserId}", id, userId);
                return Results.NotFound(new { Error = "Character not found or you are not authorized to update it" });
            }

            logger.LogInformation("Successfully updated character {CharacterId} for user {UserId}", id, userId);

            return Results.Ok(character);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating character {CharacterId}", id);
            return Results.Problem("An error occurred while updating the character");
        }
    }

    /// <summary>
    /// Deletes a character (soft delete).
    /// </summary>
    private static async Task<IResult> DeleteCharacterAsync(
        [FromRoute] int id,
        [FromServices] ICharacterService characterService,
        ILogger<ICharacterService> logger,
        HttpContext httpContext)
    {
        try
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            logger.LogInformation("Deleting character {CharacterId} for user {UserId}", id, userId);

            var success = await characterService.DeleteCharacterAsync(id, userId);

            if (!success)
            {
                logger.LogWarning("Failed to delete character {CharacterId} for user {UserId}", id, userId);
                return Results.NotFound(new { Error = "Character not found or you are not authorized to delete it" });
            }

            logger.LogInformation("Successfully deleted character {CharacterId} for user {UserId}", id, userId);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting character {CharacterId}", id);
            return Results.Problem("An error occurred while deleting the character");
        }
    }

    /// <summary>
    /// Creates a game-specific profile for a character in a campaign world.
    /// The game profile structure depends on the campaign's game type (D&D 5e, Skyrim, etc.).
    /// </summary>
    private static async Task<IResult> CreateGameProfileAsync(
        [FromRoute] int id,
        [FromQuery] int campaignId,
        [FromBody] JsonElement gameProfileData,
        [FromServices] ICharacterService characterService,
        ILogger<ICharacterService> logger,
        HttpContext httpContext)
    {
        try
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Creating game profile for character {CharacterId} in campaign {CampaignId} for user {UserId}",
                id, campaignId, userId);

            var success = await characterService.CreateGameProfileAsync(id, campaignId, gameProfileData, userId);

            if (!success)
            {
                logger.LogWarning(
                    "Failed to create game profile for character {CharacterId} in campaign {CampaignId}",
                    id, campaignId);
                return Results.BadRequest(new { Error = "Failed to create game profile. Character may already be linked to this world, or the game type is unsupported." });
            }

            logger.LogInformation(
                "Successfully created game profile for character {CharacterId} in campaign {CampaignId}",
                id, campaignId);

            return Results.Created($"/api/characters/{id}/game-profile?campaignId={campaignId}", null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating game profile for character {CharacterId}", id);
            return Results.Problem("An error occurred while creating the game profile");
        }
    }

    /// <summary>
    /// Gets the game-specific profile for a character in a campaign world.
    /// </summary>
    private static async Task<IResult> GetGameProfileAsync(
        [FromRoute] int id,
        [FromQuery] int campaignId,
        [FromServices] ICharacterService characterService,
        ILogger<ICharacterService> logger,
        HttpContext httpContext)
    {
        try
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Fetching game profile for character {CharacterId} in campaign {CampaignId} for user {UserId}",
                id, campaignId, userId);

            var profile = await characterService.GetGameProfileAsync(id, campaignId, userId);

            if (profile == null)
            {
                logger.LogWarning(
                    "Game profile not found for character {CharacterId} in campaign {CampaignId}",
                    id, campaignId);
                return Results.NotFound(new { Error = "Game profile not found" });
            }

            return Results.Ok(profile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching game profile for character {CharacterId}", id);
            return Results.Problem("An error occurred while fetching the game profile");
        }
    }

    /// <summary>
    /// Updates the game-specific profile for a character in a campaign world.
    /// </summary>
    private static async Task<IResult> UpdateGameProfileAsync(
        [FromRoute] int id,
        [FromQuery] int campaignId,
        [FromBody] JsonElement gameProfileData,
        [FromServices] ICharacterService characterService,
        ILogger<ICharacterService> logger,
        HttpContext httpContext)
    {
        try
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Updating game profile for character {CharacterId} in campaign {CampaignId} for user {UserId}",
                id, campaignId, userId);

            var success = await characterService.UpdateGameProfileAsync(id, campaignId, gameProfileData, userId);

            if (!success)
            {
                logger.LogWarning(
                    "Failed to update game profile for character {CharacterId} in campaign {CampaignId}",
                    id, campaignId);
                return Results.NotFound(new { Error = "Game profile not found or you are not authorized to update it" });
            }

            logger.LogInformation(
                "Successfully updated game profile for character {CharacterId} in campaign {CampaignId}",
                id, campaignId);

            return Results.Ok(new { Message = "Game profile updated successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating game profile for character {CharacterId}", id);
            return Results.Problem("An error occurred while updating the game profile");
        }
    }
}
