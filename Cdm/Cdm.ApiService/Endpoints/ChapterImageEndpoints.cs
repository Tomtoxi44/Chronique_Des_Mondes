// -----------------------------------------------------------------------
// <copyright file="ChapterImageEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using System.Security.Claims;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for a chapter's image gallery (maps/place visuals), GM-only.
/// </summary>
public static class ChapterImageEndpoints
{
    /// <summary>Maps the chapter image endpoints.</summary>
    public static void MapChapterImageEndpoints(this IEndpointRouteBuilder app)
    {
        var chapterGroup = app.MapGroup("/api/chapters/{chapterId:int}/images")
            .WithTags("ChapterImages")
            .RequireAuthorization();

        chapterGroup.MapGet("/", GetForChapterAsync).WithName("GetChapterImages");
        chapterGroup.MapPost("/", AddAsync).WithName("AddChapterImage");

        var imageGroup = app.MapGroup("/api/chapter-images")
            .WithTags("ChapterImages")
            .RequireAuthorization();

        imageGroup.MapDelete("/{imageId:int}", DeleteAsync).WithName("DeleteChapterImage");
    }

    private static async Task<IResult> GetForChapterAsync(int chapterId, [FromServices] IChapterImageService images, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await images.GetForChapterAsync(chapterId, userId.Value));
    }

    private static async Task<IResult> AddAsync(int chapterId, [FromBody] AddChapterImageDto dto, [FromServices] IChapterImageService images, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var created = await images.AddAsync(chapterId, dto, userId.Value);
        return created is null ? Results.BadRequest(new { error = "Ajout de l'image impossible." }) : Results.Ok(created);
    }

    private static async Task<IResult> DeleteAsync(int imageId, [FromServices] IChapterImageService images, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var ok = await images.DeleteAsync(imageId, userId.Value);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    private static int? GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
