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

        // POST /api/campaigns
        group.MapPost("/", CreateCampaignAsync)
            .RequireAuthorization(policy => policy.RequireRole("GameMaster"))
            .WithName("CreateCampaign")
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
                GameType = request.GameType,
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
}
