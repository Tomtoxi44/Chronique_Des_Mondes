namespace Cdm.Business.Abstraction.Services;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Service interface for managing user avatars
/// </summary>
public interface IAvatarService
{
    /// <summary>
    /// Uploads and saves a user avatar image
    /// </summary>
    /// <param name="userId">User unique identifier</param>
    /// <param name="file">Avatar image file</param>
    /// <returns>Avatar URL or null if upload fails</returns>
    Task<string?> UploadAvatarAsync(int userId, IFormFile file);

    /// <summary>
    /// Validates avatar file format and size
    /// </summary>
    /// <param name="file">Avatar image file</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>True if file is valid, false otherwise</returns>
    bool ValidateAvatarFile(IFormFile file, out string errorMessage);

    /// <summary>
    /// Deletes a user's avatar file
    /// </summary>
    /// <param name="avatarUrl">Avatar URL or path to delete</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteAvatarAsync(string avatarUrl);
}
