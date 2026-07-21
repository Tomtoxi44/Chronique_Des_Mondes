namespace Cdm.Business.Abstraction.Services;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;

/// <summary>
/// Service interface for managing user profiles
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Gets the profile information for a specific user
    /// </summary>
    /// <param name="userId">User unique identifier</param>
    /// <returns>User profile information</returns>
    Task<ProfileResponse?> GetProfileAsync(int userId);

    /// <summary>
    /// Updates the profile information for a specific user
    /// </summary>
    /// <param name="userId">User unique identifier</param>
    /// <param name="request">Profile update data</param>
    /// <returns>Updated profile information or null if validation fails</returns>
    Task<ProfileResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    /// <summary>
    /// Checks if a username is already taken by another user
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <param name="currentUserId">Current user ID to exclude from check</param>
    /// <returns>True if username is available, false otherwise</returns>
    Task<bool> IsUsernameAvailableAsync(string username, int currentUserId);

    /// <summary>
    /// Sets the user's avatar URL.
    /// </summary>
    /// <param name="userId">User unique identifier.</param>
    /// <param name="avatarUrl">The new avatar URL.</param>
    /// <returns><c>Found</c> is false if the user does not exist; <c>OldAvatarUrl</c> is the
    /// previous URL (for the caller to clean up the old image), or null.</returns>
    Task<(bool Found, string? OldAvatarUrl)> SetAvatarUrlAsync(int userId, string avatarUrl);
}
