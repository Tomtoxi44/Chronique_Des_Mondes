namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

/// <summary>
/// API endpoints for user profile management
/// </summary>
public static class ProfileEndpoints
{
    /// <summary>
    /// Maps profile-related endpoints to the application
    /// </summary>
    /// <param name="app">Web application instance</param>
    public static void MapProfileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users/profile")
            .WithTags("Profile")
            .RequireAuthorization();

        // GET /api/users/profile
        group.MapGet("/", GetProfile)
            .WithName("GetProfile")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get current user profile";
                operation.Description = "Retrieves the profile information for the authenticated user";
                return operation;
            });

        // PUT /api/users/profile
        group.MapPut("/", UpdateProfile)
            .WithName("UpdateProfile")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update current user profile";
                operation.Description = "Updates the profile information (username, preferences) for the authenticated user";
                return operation;
            });

        // POST /api/users/avatar
        group.MapPost("/avatar", UploadAvatar)
            .WithName("UploadAvatar")
            .DisableAntiforgery()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Upload user avatar";
                operation.Description = "Uploads a new avatar image for the authenticated user (max 2MB, jpg/png)";
                return operation;
            });
    }

    /// <summary>
    /// Gets the current user's profile
    /// </summary>
    private static async Task<IResult> GetProfile(
        ClaimsPrincipal user,
        [FromServices] IUserProfileService profileService,
        [FromServices] ILogger<IUserProfileService> logger)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            logger.LogWarning("Invalid user ID claim");
            return Results.Unauthorized();
        }

        var profile = await profileService.GetProfileAsync(userId);
        if (profile == null)
        {
            logger.LogWarning("Profile not found for user {UserId}", userId);
            return Results.NotFound(new { error = "Profile not found" });
        }

        return Results.Ok(profile);
    }

    /// <summary>
    /// Updates the current user's profile
    /// </summary>
    private static async Task<IResult> UpdateProfile(
        ClaimsPrincipal user,
        [FromBody] UpdateProfileRequest request,
        [FromServices] IUserProfileService profileService,
        [FromServices] ILogger<IUserProfileService> logger)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            logger.LogWarning("Invalid user ID claim");
            return Results.Unauthorized();
        }

        // Validate username if provided
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            if (request.Username.Length < 3 || request.Username.Length > 30)
            {
                return Results.BadRequest(new { error = "Username must be between 3 and 30 characters" });
            }
        }

        var profile = await profileService.UpdateProfileAsync(userId, request);
        if (profile == null)
        {
            logger.LogWarning("Profile update failed for user {UserId}", userId);
            return Results.BadRequest(new { error = "Profile update failed. Username may already be taken." });
        }

        return Results.Ok(profile);
    }

    /// <summary>
    /// Uploads a new avatar for the current user
    /// </summary>
    private static async Task<IResult> UploadAvatar(
        HttpRequest request,
        ClaimsPrincipal user,
        [FromServices] IImageStorage imageStorage,
        [FromServices] Cdm.Business.Abstraction.Services.IUserProfileService profileService,
        [FromServices] ILogger<Cdm.Business.Abstraction.Services.IUserProfileService> logger)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            logger.LogWarning("Invalid user ID claim");
            return Results.Unauthorized();
        }

        if (!request.HasFormContentType || !request.Form.Files.Any())
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        // Une photo de profil suit les limites « portrait » (poids et dimensions bridés).
        var policy = ImagePolicy.For("avatars");
        var file = request.Form.Files[0];
        if (file.Length == 0 || file.Length > policy.MaxFileSizeBytes)
        {
            return Results.BadRequest(new { error = file.Length == 0 ? "Fichier vide." : policy.TooHeavyMessage() });
        }

        // Store through the configured image storage (Azure Blob in prod, local in dev) so the
        // returned URL is usable cross-origin — the old local-only path returned relative URLs
        // that 404 when the Web app and the API are on different origins.
        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            bytes = ms.ToArray();
        }

        var result = await imageStorage.UploadAsync(bytes, "avatars", userId.Value.ToString());
        if (!result.Success || string.IsNullOrEmpty(result.Url))
        {
            logger.LogError("Avatar upload failed for user {UserId}: {Error}", userId, result.Error);
            return Results.BadRequest(new { error = result.Error ?? "Échec de l'envoi de l'avatar." });
        }

        // Persisting the URL is business logic → delegated to the profile service (layering).
        var (found, oldAvatarUrl) = await profileService.SetAvatarUrlAsync(userId.Value, result.Url);
        if (!found)
        {
            return Results.NotFound(new { error = "User not found" });
        }

        // Best-effort cleanup of the previous image.
        if (!string.IsNullOrWhiteSpace(oldAvatarUrl))
        {
            try { await imageStorage.DeleteAsync(oldAvatarUrl); } catch { /* non-blocking */ }
        }

        return Results.Ok(new { avatarUrl = result.Url });
    }
}
