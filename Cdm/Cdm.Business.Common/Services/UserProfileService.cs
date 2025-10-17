namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
using Cdm.Business.Abstraction.Services;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing user profiles
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly DbContext dbContext;
    private readonly ILogger<UserProfileService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileService"/> class
    /// </summary>
    /// <param name="dbContext">Database context</param>
    /// <param name="logger">Logger instance</param>
    public UserProfileService(DbContext dbContext, ILogger<UserProfileService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ProfileResponse?> GetProfileAsync(int userId)
    {
        this.logger.LogInformation("Getting profile for user {UserId}", userId);

        var user = await this.dbContext.Set<User>()
            .Where(u => u.Id == userId)
            .Select(u => new ProfileResponse
            {
                Id = u.Id,
                Email = u.Email,
                Nickname = u.Nickname,
                Username = u.Username,
                AvatarUrl = u.AvatarUrl,
                Preferences = u.Preferences,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            this.logger.LogWarning("User {UserId} not found", userId);
        }

        return user;
    }

    /// <inheritdoc/>
    public async Task<ProfileResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        this.logger.LogInformation("Updating profile for user {UserId}", userId);

        var user = await this.dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            this.logger.LogWarning("User {UserId} not found for update", userId);
            return null;
        }

        // Check username uniqueness if provided and different
        if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != user.Username)
        {
            var isAvailable = await this.IsUsernameAvailableAsync(request.Username, userId);
            if (!isAvailable)
            {
                this.logger.LogWarning("Username {Username} is already taken", request.Username);
                return null;
            }

            user.Username = request.Username;
        }

        // Update preferences if provided
        if (request.Preferences != null)
        {
            user.Preferences = request.Preferences;
        }

        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation("Profile updated successfully for user {UserId}", userId);

            return await this.GetProfileAsync(userId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsUsernameAvailableAsync(string username, int currentUserId)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        var exists = await this.dbContext.Set<User>()
            .AnyAsync(u => u.Username == username && u.Id != currentUserId);

        return !exists;
    }
}
