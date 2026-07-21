// -----------------------------------------------------------------------
// <copyright file="CodexEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using System.Security.Claims;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Enums;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for the user's personal codex of item templates.
/// </summary>
public static class CodexEndpoints
{
    private sealed class CodexEndpointsLogger
    {
    }

    /// <summary>Maps the codex endpoints.</summary>
    public static void MapCodexEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/codex")
            .WithTags("Codex")
            .RequireAuthorization();

        group.MapGet("/", GetMyItemsAsync).WithName("GetMyCodexItems");
        group.MapGet("/{id:int}", GetItemAsync).WithName("GetCodexItem");
        group.MapPost("/", CreateItemAsync).WithName("CreateCodexItem");
        group.MapPut("/{id:int}", UpdateItemAsync).WithName("UpdateCodexItem");
        group.MapDelete("/{id:int}", DeleteItemAsync).WithName("DeleteCodexItem");
        group.MapPost("/{id:int}/add-to-character/{worldCharacterId:int}", AddToCharacterAsync)
            .WithName("AddCodexItemToCharacter");
        group.MapPut("/{id:int}/share", SetSharedAsync)
            .WithName("ShareCodexItem");

        // ── Marketplace ──────────────────────────────────────────────────
        var market = app.MapGroup("/api/marketplace")
            .WithTags("Marketplace")
            .RequireAuthorization();

        market.MapGet("/items", GetMarketplaceItemsAsync).WithName("GetMarketplaceItems");
        market.MapPost("/items/{id:int}/import", ImportItemAsync).WithName("ImportMarketplaceItem");
    }

    private static async Task<IResult> SetSharedAsync(
        int id,
        [FromServices] ICodexService codex,
        ClaimsPrincipal user,
        [FromQuery] bool shared = true)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var ok = await codex.SetSharedAsync(id, userId.Value, shared);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> GetMarketplaceItemsAsync(
        [FromServices] ICodexService codex,
        ClaimsPrincipal user,
        [FromQuery] GameType? gameType = null,
        [FromQuery] string? search = null)
    {
        if (user.GetUserId() is null)
        {
            return Results.Unauthorized();
        }

        var items = await codex.GetMarketplaceItemsAsync(gameType, search);
        return Results.Ok(items);
    }

    private static async Task<IResult> ImportItemAsync(int id, [FromServices] ICodexService codex, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var imported = await codex.ImportToMyCodexAsync(id, userId.Value);
        return imported is null ? Results.BadRequest(new { error = "Import impossible." }) : Results.Ok(imported);
    }


    private static async Task<IResult> GetMyItemsAsync(
        [FromServices] ICodexService codex,
        ClaimsPrincipal user,
        [FromQuery] GameType? gameType = null)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var items = await codex.GetMyItemsAsync(userId.Value, gameType);
        return Results.Ok(items);
    }

    private static async Task<IResult> GetItemAsync(int id, [FromServices] ICodexService codex, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var item = await codex.GetByIdAsync(id, userId.Value);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateItemAsync(
        [FromBody] CreateCodexItemDto dto,
        [FromServices] ICodexService codex,
        ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var created = await codex.CreateAsync(dto, userId.Value);
        return created is null
            ? Results.Problem("Création impossible.", statusCode: StatusCodes.Status500InternalServerError)
            : Results.Ok(created);
    }

    private static async Task<IResult> UpdateItemAsync(
        int id,
        [FromBody] CreateCodexItemDto dto,
        [FromServices] ICodexService codex,
        ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var updated = await codex.UpdateAsync(id, dto, userId.Value);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }

    private static async Task<IResult> DeleteItemAsync(int id, [FromServices] ICodexService codex, ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var ok = await codex.DeleteAsync(id, userId.Value);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> AddToCharacterAsync(
        int id,
        int worldCharacterId,
        [FromServices] ICodexService codex,
        ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var (ok, error) = await codex.AddToCharacterInventoryAsync(id, worldCharacterId, userId.Value);
        return ok ? Results.Ok() : Results.BadRequest(new { error });
    }
}
