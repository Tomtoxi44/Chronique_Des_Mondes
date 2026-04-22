// -----------------------------------------------------------------------
// <copyright file="DndEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs.DnD5e;
using Cdm.Business.Abstraction.Services.DnD5e;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>D&amp;D 5e endpoints: reference data, world character stats, inventory and spells.</summary>
public static class DndEndpoints
{
    private sealed class DndEndpointsLogger { }

    public static void MapDndEndpoints(this IEndpointRouteBuilder app)
    {
        MapReferenceEndpoints(app);
        MapCharacterStatsEndpoints(app);
        MapInventoryEndpoints(app);
        MapSpellsEndpoints(app);
        MapDndNpcEndpoints(app);
    }

    // ── Reference Data ───────────────────────────────────────────────────

    private static void MapReferenceEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dnd")
            .WithTags("D&D 5e Reference")
            .RequireAuthorization();

        group.MapGet("/races", async ([FromServices] IDndReferenceService svc) =>
            Results.Ok(await svc.GetRacesAsync()))
            .WithName("GetDndRaces");

        group.MapGet("/races/{id:int}", async (int id, [FromServices] IDndReferenceService svc) =>
        {
            var race = await svc.GetRaceByIdAsync(id);
            return race is null ? Results.NotFound() : Results.Ok(race);
        }).WithName("GetDndRace");

        group.MapGet("/classes", async ([FromServices] IDndReferenceService svc) =>
            Results.Ok(await svc.GetClassesAsync()))
            .WithName("GetDndClasses");

        group.MapGet("/classes/{id:int}", async (int id, [FromServices] IDndReferenceService svc) =>
        {
            var cls = await svc.GetClassByIdAsync(id);
            return cls is null ? Results.NotFound() : Results.Ok(cls);
        }).WithName("GetDndClass");

        group.MapGet("/items", async ([FromQuery] string? category, [FromServices] IDndReferenceService svc) =>
            Results.Ok(await svc.GetItemsAsync(category)))
            .WithName("GetDndItems");

        group.MapGet("/items/{id:int}", async (int id, [FromServices] IDndReferenceService svc) =>
        {
            var item = await svc.GetItemByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetDndItem");

        group.MapGet("/spells", async ([FromQuery] int? level, [FromQuery] string? characterClass, [FromServices] IDndReferenceService svc) =>
            Results.Ok(await svc.GetSpellsAsync(level, characterClass)))
            .WithName("GetDndSpells");

        group.MapGet("/spells/{id:int}", async (int id, [FromServices] IDndReferenceService svc) =>
        {
            var spell = await svc.GetSpellByIdAsync(id);
            return spell is null ? Results.NotFound() : Results.Ok(spell);
        }).WithName("GetDndSpell");

        group.MapGet("/monsters", async ([FromServices] IDndReferenceService svc) =>
            Results.Ok(await svc.GetMonsterTemplatesAsync()))
            .WithName("GetDndMonsters");

        group.MapGet("/monsters/{id:int}", async (int id, [FromServices] IDndReferenceService svc) =>
        {
            var m = await svc.GetMonsterTemplateByIdAsync(id);
            return m is null ? Results.NotFound() : Results.Ok(m);
        }).WithName("GetDndMonster");
    }

    // ── Character Stats ──────────────────────────────────────────────────

    private static void MapCharacterStatsEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dnd/world-characters")
            .WithTags("D&D 5e Character Stats")
            .RequireAuthorization();

        group.MapGet("/{wcId:int}/stats", async (int wcId, HttpContext ctx, [FromServices] IDndCharacterService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            var stats = await svc.GetDndStatsAsync(wcId, uid.Value);
            return stats is null ? Results.NotFound() : Results.Ok(stats);
        }).WithName("GetDndCharacterStats");

        group.MapPut("/{wcId:int}/stats", async (int wcId, [FromBody] DndCharacterStatsDto dto, HttpContext ctx, [FromServices] IDndCharacterService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            dto.WorldCharacterId = wcId;
            var ok = await svc.ApplyDndStatsAsync(wcId, dto, uid.Value);
            return ok ? Results.Ok() : Results.Forbid();
        }).WithName("ApplyDndCharacterStats");
    }

    // ── Inventory ────────────────────────────────────────────────────────

    private static void MapInventoryEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dnd/world-characters")
            .WithTags("D&D 5e Inventory")
            .RequireAuthorization();

        group.MapGet("/{wcId:int}/inventory", async (int wcId, HttpContext ctx, [FromServices] IDndCharacterService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            return Results.Ok(await svc.GetInventoryAsync(wcId, uid.Value));
        }).WithName("GetDndInventory");

        group.MapPost("/{wcId:int}/inventory", async (int wcId, [FromBody] DndInventoryItemDto dto, HttpContext ctx, [FromServices] IDndCharacterService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            try
            {
                dto.WorldCharacterId = wcId;
                var item = await svc.AddInventoryItemAsync(wcId, dto, uid.Value);
                return Results.Created($"/api/dnd/world-characters/{wcId}/inventory/{item.Id}", item);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        }).WithName("AddDndInventoryItem");

        group.MapDelete("/{wcId:int}/inventory/{itemId:int}", async (int wcId, int itemId, HttpContext ctx, [FromServices] IDndCharacterService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            return await svc.RemoveInventoryItemAsync(wcId, itemId, uid.Value) ? Results.NoContent() : Results.NotFound();
        }).WithName("RemoveDndInventoryItem");
    }

    // ── Spells ───────────────────────────────────────────────────────────

    private static void MapSpellsEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dnd/world-characters")
            .WithTags("D&D 5e Character Spells")
            .RequireAuthorization();

        group.MapGet("/{wcId:int}/spells", async (int wcId, HttpContext ctx, [FromServices] IDndCharacterService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            return Results.Ok(await svc.GetSpellsAsync(wcId, uid.Value));
        }).WithName("GetDndCharacterSpells");

        group.MapPost("/{wcId:int}/spells", async (int wcId, [FromBody] DndCharacterSpellDto dto, HttpContext ctx, [FromServices] IDndCharacterService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            try
            {
                dto.WorldCharacterId = wcId;
                var spell = await svc.AddSpellAsync(wcId, dto, uid.Value);
                return Results.Created($"/api/dnd/world-characters/{wcId}/spells/{spell.Id}", spell);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        }).WithName("AddDndCharacterSpell");

        group.MapDelete("/{wcId:int}/spells/{spellId:int}", async (int wcId, int spellId, HttpContext ctx, [FromServices] IDndCharacterService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            return await svc.RemoveSpellAsync(wcId, spellId, uid.Value) ? Results.NoContent() : Results.NotFound();
        }).WithName("RemoveDndCharacterSpell");
    }

    // ── D&D NPC endpoints ────────────────────────────────────────────────

    private static void MapDndNpcEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dnd/npcs")
            .WithTags("D&D 5e NPCs")
            .RequireAuthorization();

        group.MapGet("/chapter/{chapterId:int}", async (int chapterId, HttpContext ctx, [FromServices] IDndNpcService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            return Results.Ok(await svc.GetDndNpcsAsync(chapterId));
        }).WithName("GetDndNpcs");

        group.MapGet("/{id:int}", async (int id, [FromServices] IDndNpcService svc) =>
        {
            var npc = await svc.GetDndNpcByIdAsync(id);
            return npc is null ? Results.NotFound() : Results.Ok(npc);
        }).WithName("GetDndNpc");

        group.MapPost("/chapter/{chapterId:int}", async (int chapterId, [FromBody] CreateDndNpcDto dto, HttpContext ctx, [FromServices] IDndNpcService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            try
            {
                var npc = await svc.CreateDndNpcAsync(chapterId, dto, uid.Value);
                return Results.Created($"/api/dnd/npcs/{npc.Id}", npc);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        }).WithName("CreateDndNpc");

        group.MapPut("/{id:int}", async (int id, [FromBody] CreateDndNpcDto dto, HttpContext ctx, [FromServices] IDndNpcService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            var result = await svc.UpdateDndNpcAsync(id, dto, uid.Value);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName("UpdateDndNpc");

        group.MapDelete("/{id:int}", async (int id, HttpContext ctx, [FromServices] IDndNpcService svc) =>
        {
            var uid = GetUserId(ctx); if (uid is null) return Results.Unauthorized();
            return await svc.DeleteDndNpcAsync(id, uid.Value) ? Results.NoContent() : Results.NotFound();
        }).WithName("DeleteDndNpc");
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var userId)) return null;
        return userId;
    }
}
