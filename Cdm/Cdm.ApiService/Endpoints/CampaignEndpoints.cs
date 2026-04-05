// -----------------------------------------------------------------------
// <copyright file="CampaignEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.ApiService.Endpoints.Models;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Campaign endpoints for the API.
/// </summary>
public static class CampaignEndpoints
{
    /// <summary>
    /// Maps campaign endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapCampaignEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/campaigns")
            .WithTags("Campaigns")
            .RequireAuthorization();

        // GET /api/campaigns - Get all my campaigns
        group.MapGet("/", GetMyCampaignsAsync)
            .WithName("GetMyCampaigns")
            .WithOpenApi();

        // GET /api/campaigns/{id} - Get a specific campaign
        group.MapGet("/{id:int}", GetCampaignByIdAsync)
            .WithName("GetCampaignById")
            .WithOpenApi();

        // POST /api/campaigns
        group.MapPost("/", CreateCampaignAsync)
            .RequireAuthorization(policy => policy.RequireRole("GameMaster"))
            .WithName("CreateCampaign")
            .WithOpenApi();

        // PUT /api/campaigns/{id} - Update a campaign
        group.MapPut("/{id:int}", UpdateCampaignAsync)
            .RequireAuthorization(policy => policy.RequireRole("GameMaster"))
            .WithName("UpdateCampaign")
            .WithOpenApi();

        // DELETE /api/campaigns/{id} - Delete a campaign
        group.MapDelete("/{id:int}", DeleteCampaignAsync)
            .RequireAuthorization(policy => policy.RequireRole("GameMaster"))
            .WithName("DeleteCampaign")
            .WithOpenApi();
    }

    /// <summary>
    /// Creates a new campaign.
    /// </summary>
    /// <param name="request">The campaign creation request.</param>
    /// <param name="campaignService">The campaign service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The created campaign response.</returns>
    private static async Task<IResult> CreateCampaignAsync(
        [FromBody] CreateCampaignRequest request,
        [FromServices] ICampaignService campaignService,
        ILogger<CreateCampaignRequest> logger,
        HttpContext httpContext)
    {
        try
        {
            // Get user ID from claims
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            logger.LogInformation("Creating campaign '{CampaignName}' for user {UserId}", request.Name, userId);

            // Map request to DTO
            var createDto = new CreateCampaignDto
            {
                Name = request.Name,
                Description = request.Description,
                WorldId = request.WorldId,
                Visibility = request.Visibility,
                MaxPlayers = request.MaxPlayers,
                CoverImageBase64 = request.CoverImageBase64
            };

            // Create campaign
            var campaignDto = await campaignService.CreateCampaignAsync(createDto, userId);

            if (campaignDto == null)
            {
                logger.LogWarning("Failed to create campaign '{CampaignName}' for user {UserId}", request.Name, userId);
                return Results.BadRequest(new { Error = "Failed to create campaign. Please check your input and try again." });
            }

            logger.LogInformation("Successfully created campaign {CampaignId} for user {UserId}", campaignDto.Id, userId);

            // Map DTO to response
            var response = new CampaignResponse
            {
                Id = campaignDto.Id,
                Name = campaignDto.Name,
                Description = campaignDto.Description,
                GameType = campaignDto.GameType,
                Visibility = campaignDto.Visibility,
                MaxPlayers = campaignDto.MaxPlayers,
                CoverImageUrl = campaignDto.CoverImageUrl,
                CreatedBy = campaignDto.CreatedBy,
                CreatedAt = campaignDto.CreatedAt,
                UpdatedAt = campaignDto.UpdatedAt,
                IsActive = campaignDto.IsActive
            };

            return Results.Created($"/api/campaigns/{response.Id}", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating campaign");
            return Results.Problem(
                title: "An error occurred while creating the campaign",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets all campaigns for the current user.
    /// </summary>
    /// <param name="campaignService">The campaign service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A list of campaigns.</returns>
    private static async Task<IResult> GetMyCampaignsAsync(
        [FromServices] ICampaignService campaignService,
        ILogger<ICampaignService> logger,
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

            logger.LogInformation("Fetching campaigns for user {UserId}", userId);

            var campaigns = await campaignService.GetMyCampaignsAsync(userId);

            return Results.Ok(campaigns);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching campaigns");
            return Results.Problem(
                title: "An error occurred while fetching campaigns",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a specific campaign by ID.
    /// </summary>
    /// <param name="id">The campaign ID.</param>
    /// <param name="campaignService">The campaign service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The campaign if found and authorized.</returns>
    private static async Task<IResult> GetCampaignByIdAsync(
        [FromRoute] int id,
        [FromServices] ICampaignService campaignService,
        ILogger<ICampaignService> logger,
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

            logger.LogInformation("Fetching campaign {CampaignId} for user {UserId}", id, userId);

            var campaign = await campaignService.GetCampaignByIdAsync(id, userId);

            if (campaign == null)
            {
                logger.LogWarning("Campaign {CampaignId} not found or not authorized for user {UserId}", id, userId);
                return Results.NotFound(new { Error = "Campaign not found or access denied" });
            }

            return Results.Ok(campaign);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching campaign {CampaignId}", id);
            return Results.Problem(
                title: "An error occurred while fetching the campaign",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Updates an existing campaign.
    /// </summary>
    /// <param name="id">The campaign ID.</param>
    /// <param name="request">The campaign update request.</param>
    /// <param name="campaignService">The campaign service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The updated campaign response.</returns>
    private static async Task<IResult> UpdateCampaignAsync(
        [FromRoute] int id,
        [FromBody] CreateCampaignRequest request,
        [FromServices] ICampaignService campaignService,
        ILogger<CreateCampaignRequest> logger,
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

            logger.LogInformation("Updating campaign {CampaignId} for user {UserId}", id, userId);

            var updateDto = new CreateCampaignDto
            {
                Name = request.Name,
                Description = request.Description,
                WorldId = request.WorldId,
                Visibility = request.Visibility,
                MaxPlayers = request.MaxPlayers,
                CoverImageBase64 = request.CoverImageBase64
            };

            var campaignDto = await campaignService.UpdateCampaignAsync(id, updateDto, userId);

            if (campaignDto == null)
            {
                logger.LogWarning("Failed to update campaign {CampaignId} for user {UserId}", id, userId);
                return Results.NotFound(new { Error = "Campaign not found or not authorized" });
            }

            logger.LogInformation("Successfully updated campaign {CampaignId} for user {UserId}", id, userId);

            var response = new CampaignResponse
            {
                Id = campaignDto.Id,
                Name = campaignDto.Name,
                Description = campaignDto.Description,
                GameType = campaignDto.GameType,
                Visibility = campaignDto.Visibility,
                MaxPlayers = campaignDto.MaxPlayers,
                CoverImageUrl = campaignDto.CoverImageUrl,
                CreatedBy = campaignDto.CreatedBy,
                CreatedAt = campaignDto.CreatedAt,
                UpdatedAt = campaignDto.UpdatedAt,
                IsActive = campaignDto.IsActive
            };

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating campaign {CampaignId}", id);
            return Results.Problem(
                title: "An error occurred while updating the campaign",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Deletes a campaign (soft delete).
    /// </summary>
    /// <param name="id">The campaign ID.</param>
    /// <param name="campaignService">The campaign service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>No content if successful.</returns>
    private static async Task<IResult> DeleteCampaignAsync(
        [FromRoute] int id,
        [FromServices] ICampaignService campaignService,
        ILogger<ICampaignService> logger,
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

            logger.LogInformation("Deleting campaign {CampaignId} for user {UserId}", id, userId);

            var success = await campaignService.DeleteCampaignAsync(id, userId);

            if (!success)
            {
                logger.LogWarning("Failed to delete campaign {CampaignId} for user {UserId}", id, userId);
                return Results.NotFound(new { Error = "Campaign not found or not authorized" });
            }

            logger.LogInformation("Successfully deleted campaign {CampaignId} for user {UserId}", id, userId);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
            return Results.Problem(
                title: "An error occurred while deleting the campaign",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
