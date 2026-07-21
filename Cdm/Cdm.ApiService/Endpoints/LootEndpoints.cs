// -----------------------------------------------------------------------
// <copyright file="LootEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using System.Security.Claims;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for campaign loot: the GM prepares loot on a campaign/chapter and
/// distributes it to players' characters during a session.
/// </summary>
public static class LootEndpoints
{
    /// <summary>Maps the loot endpoints.</summary>
    public static void MapLootEndpoints(this IEndpointRouteBuilder app)
    {
        var campaignGroup = app.MapGroup("/api/campaigns/{campaignId:int}/loot")
            .WithTags("Loot")
            .RequireAuthorization();

        campaignGroup.MapGet("/", GetCampaignLootAsync).WithName("GetCampaignLoot");
        campaignGroup.MapPost("/", CreateLootAsync).WithName("CreateLoot");

        var lootGroup = app.MapGroup("/api/loot")
            .WithTags("Loot")
            .RequireAuthorization();

        lootGroup.MapPut("/{lootId:int}", UpdateLootAsync).WithName("UpdateLoot");
        lootGroup.MapDelete("/{lootId:int}", DeleteLootAsync).WithName("DeleteLoot");
        lootGroup.MapPost("/{lootId:int}/distribute", DistributeLootAsync).WithName("DistributeLoot");
    }

    private static async Task<IResult> GetCampaignLootAsync(int campaignId, [FromServices] ILootService loot, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await loot.GetCampaignLootAsync(campaignId, userId.Value));
    }

    private static async Task<IResult> CreateLootAsync(int campaignId, [FromBody] CreateLootDto dto, [FromServices] ILootService loot, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var created = await loot.CreateAsync(campaignId, dto, userId.Value);
        return created is null ? Results.BadRequest(new { error = "Création du loot impossible." }) : Results.Ok(created);
    }

    private static async Task<IResult> UpdateLootAsync(int lootId, [FromBody] CreateLootDto dto, [FromServices] ILootService loot, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var updated = await loot.UpdateAsync(lootId, dto, userId.Value);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }

    private static async Task<IResult> DeleteLootAsync(int lootId, [FromServices] ILootService loot, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var ok = await loot.DeleteAsync(lootId, userId.Value);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> DistributeLootAsync(
        int lootId,
        [FromServices] ILootService loot,
        ClaimsPrincipal user,
        [FromQuery] int worldCharacterId,
        [FromQuery] int? sessionId = null)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var (success, error, result) = await loot.DistributeAsync(lootId, worldCharacterId, sessionId, userId.Value);
        return success ? Results.Ok(result) : Results.BadRequest(new { error });
    }

}
