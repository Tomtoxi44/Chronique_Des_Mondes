namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.Services;
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
        [FromServices] IAvatarService avatarService,
        [FromServices] DbContext dbContext,
        [FromServices] ILogger<IAvatarService> logger)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            logger.LogWarning("Invalid user ID claim");
            return Results.Unauthorized();
        }

        if (!request.HasFormContentType || !request.Form.Files.Any())
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var file = request.Form.Files[0];

        if (!avatarService.ValidateAvatarFile(file, out var errorMessage))
        {
            logger.LogWarning("Avatar validation failed: {Error}", errorMessage);
            return Results.BadRequest(new { error = errorMessage });
        }

        var avatarUrl = await avatarService.UploadAvatarAsync(userId, file);
        if (avatarUrl == null)
        {
            logger.LogError("Avatar upload failed for user {UserId}", userId);
            return Results.StatusCode(500);
        }

        // Update user's avatar URL in database
        var userEntity = await dbContext.Set<Cdm.Data.Common.Models.User>()
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (userEntity == null)
        {
            logger.LogWarning("User not found after avatar upload for user {UserId}", userId);
            return Results.NotFound(new { error = "User not found" });
        }

        // Delete old avatar if exists
        if (!string.IsNullOrWhiteSpace(userEntity.AvatarUrl))
        {
            await avatarService.DeleteAvatarAsync(userEntity.AvatarUrl);
        }

        userEntity.AvatarUrl = avatarUrl;
        userEntity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Results.Ok(new { avatarUrl });
    }
}
