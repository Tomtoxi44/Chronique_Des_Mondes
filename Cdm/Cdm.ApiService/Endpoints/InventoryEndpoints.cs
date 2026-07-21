// -----------------------------------------------------------------------
// <copyright file="InventoryEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using System.Security.Claims;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Universal (all game systems) inventory endpoints for a world character.
/// </summary>
public static class InventoryEndpoints
{
    /// <summary>Maps the unified inventory endpoints.</summary>
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var charGroup = app.MapGroup("/api/world-characters/{worldCharacterId:int}/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        charGroup.MapGet("/", GetForCharacterAsync).WithName("GetCharacterInventory");
        charGroup.MapGet("/gm", GetForCharacterAsGmAsync).WithName("GetCharacterInventoryAsGm");
        charGroup.MapPost("/", AddAsync).WithName("AddInventoryItem");

        var itemGroup = app.MapGroup("/api/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        itemGroup.MapPut("/{itemId:int}", UpdateAsync).WithName("UpdateInventoryItem");
        itemGroup.MapDelete("/{itemId:int}", DeleteAsync).WithName("DeleteInventoryItem");
    }

    private static async Task<IResult> GetForCharacterAsync(int worldCharacterId, [FromServices] IInventoryService inventory, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await inventory.GetForCharacterAsync(worldCharacterId, userId.Value));
    }

    private static async Task<IResult> GetForCharacterAsGmAsync(int worldCharacterId, [FromServices] IInventoryService inventory, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await inventory.GetForCharacterAsGmAsync(worldCharacterId, userId.Value));
    }

    private static async Task<IResult> AddAsync(int worldCharacterId, [FromBody] CreateInventoryItemDto dto, [FromServices] IInventoryService inventory, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var created = await inventory.AddAsync(worldCharacterId, dto, userId.Value);
        return created is null ? Results.BadRequest(new { error = "Ajout impossible." }) : Results.Ok(created);
    }

    private static async Task<IResult> UpdateAsync(int itemId, [FromBody] CreateInventoryItemDto dto, [FromServices] IInventoryService inventory, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var updated = await inventory.UpdateAsync(itemId, dto, userId.Value);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }

    private static async Task<IResult> DeleteAsync(int itemId, [FromServices] IInventoryService inventory, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var ok = await inventory.DeleteAsync(itemId, userId.Value);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    private static int? GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
