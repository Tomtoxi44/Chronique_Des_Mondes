// -----------------------------------------------------------------------
// <copyright file="ImageEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using System.Security.Claims;
using Cdm.Common.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Generic image upload endpoints backed by <see cref="IImageStorage"/>.
/// </summary>
public static class ImageEndpoints
{
    private sealed class ImageEndpointsLogger
    {
    }

    private static readonly HashSet<string> AllowedCategories =
        new(StringComparer.OrdinalIgnoreCase) { "session", "maps", "items", "misc" };

    /// <summary>Maps the image endpoints.</summary>
    public static void MapImageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/images")
            .WithTags("Images")
            .RequireAuthorization();

        // POST /api/images/{category} — multipart upload, returns { url }.
        group.MapPost("/{category}", UploadImageAsync)
            .WithName("UploadImage");
    }

    private static async Task<IResult> UploadImageAsync(
        string category,
        HttpRequest request,
        ClaimsPrincipal user,
        [FromServices] IImageStorage imageStorage,
        [FromServices] ILogger<ImageEndpointsLogger> logger)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
        {
            return Results.Unauthorized();
        }

        if (!AllowedCategories.Contains(category))
        {
            return Results.BadRequest(new { error = "Catégorie d'image invalide." });
        }

        // Reading Form.Files directly (rather than an IFormFile parameter) avoids the
        // minimal-API antiforgery requirement, consistent with the avatar endpoint.
        if (!request.HasFormContentType || request.Form.Files.Count == 0)
        {
            return Results.BadRequest(new { error = "Aucun fichier fourni." });
        }

        var file = request.Form.Files[0];
        if (file.Length == 0 || file.Length > ImageValidation.MaxFileSizeBytes)
        {
            return Results.BadRequest(new { error = "Fichier vide ou trop lourd (maximum 5 Mo)." });
        }

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var result = await imageStorage.UploadAsync(ms.ToArray(), category, userId.ToString());
            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            logger.LogInformation("User {UserId} uploaded an image in category {Category}", userId, category);
            return Results.Ok(new { url = result.Url });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading image (category {Category})", category);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
