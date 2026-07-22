// -----------------------------------------------------------------------
// <copyright file="MarketplaceEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using System.Security.Claims;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Enums;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for sharing / browsing / importing worlds, campaigns and base characters on the marketplace.
/// (Codex item marketplace lives in <see cref="CodexEndpoints"/>.)
/// </summary>
public static class MarketplaceEndpoints
{
    /// <summary>Maps the marketplace endpoints for worlds, campaigns and characters.</summary>
    public static void MapMarketplaceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/marketplace")
            .WithTags("Marketplace")
            .RequireAuthorization();

        // Browse.
        group.MapGet("/worlds", GetSharedWorldsAsync).WithName("GetSharedWorlds");
        group.MapGet("/campaigns", GetSharedCampaignsAsync).WithName("GetSharedCampaigns");
        group.MapGet("/characters", GetSharedCharactersAsync).WithName("GetSharedCharacters");

        // Import.
        group.MapPost("/worlds/{id:int}/import", ImportWorldAsync).WithName("ImportSharedWorld");
        group.MapPost("/campaigns/{id:int}/import", ImportCampaignAsync).WithName("ImportSharedCampaign");
        group.MapPost("/characters/{id:int}/import", ImportCharacterAsync).WithName("ImportSharedCharacter");

        // Share toggles.
        group.MapPut("/worlds/{id:int}/share", SetWorldSharedAsync).WithName("ShareWorld");
        group.MapPut("/campaigns/{id:int}/share", SetCampaignSharedAsync).WithName("ShareCampaign");
        group.MapPut("/characters/{id:int}/share", SetCharacterSharedAsync).WithName("ShareCharacter");
    }

    private static async Task<IResult> GetSharedWorldsAsync(
        [FromServices] IMarketplaceService market,
        ClaimsPrincipal user,
        [FromQuery] GameType? gameType = null,
        [FromQuery] string? search = null)
    {
        if (user.GetUserId() is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await market.GetSharedWorldsAsync(gameType, search));
    }

    private static async Task<IResult> GetSharedCampaignsAsync(
        [FromServices] IMarketplaceService market,
        ClaimsPrincipal user,
        [FromQuery] GameType? gameType = null,
        [FromQuery] string? search = null)
    {
        if (user.GetUserId() is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await market.GetSharedCampaignsAsync(gameType, search));
    }

    private static async Task<IResult> GetSharedCharactersAsync(
        [FromServices] IMarketplaceService market,
        ClaimsPrincipal user,
        [FromQuery] string? search = null)
    {
        if (user.GetUserId() is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await market.GetSharedCharactersAsync(search));
    }

    private static async Task<IResult> ImportWorldAsync(int id, [FromServices] IMarketplaceService market, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var (ok, error) = await market.ImportWorldAsync(id, userId.Value);
        return ok ? Results.Ok() : Results.BadRequest(new { error });
    }

    private static async Task<IResult> ImportCampaignAsync(
        int id,
        [FromServices] IMarketplaceService market,
        ClaimsPrincipal user,
        [FromQuery] int targetWorldId)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var (ok, error) = await market.ImportCampaignAsync(id, targetWorldId, userId.Value);
        return ok ? Results.Ok() : Results.BadRequest(new { error });
    }

    private static async Task<IResult> ImportCharacterAsync(int id, [FromServices] IMarketplaceService market, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var (ok, error) = await market.ImportCharacterAsync(id, userId.Value);
        return ok ? Results.Ok() : Results.BadRequest(new { error });
    }

    private static async Task<IResult> SetWorldSharedAsync(
        int id,
        [FromServices] IMarketplaceService market,
        ClaimsPrincipal user,
        [FromQuery] bool shared = true)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var ok = await market.SetWorldSharedAsync(id, userId.Value, shared);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> SetCampaignSharedAsync(
        int id,
        [FromServices] IMarketplaceService market,
        ClaimsPrincipal user,
        [FromQuery] bool shared = true)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var ok = await market.SetCampaignSharedAsync(id, userId.Value, shared);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> SetCharacterSharedAsync(
        int id,
        [FromServices] IMarketplaceService market,
        ClaimsPrincipal user,
        [FromQuery] bool shared = true)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var ok = await market.SetCharacterSharedAsync(id, userId.Value, shared);
        return ok ? Results.NoContent() : Results.NotFound();
    }

}
