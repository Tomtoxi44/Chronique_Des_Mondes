namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing user avatar uploads
/// </summary>
public class AvatarService : IAvatarService
{
    private readonly ILogger<AvatarService> logger;
    private readonly string uploadsPath;
    private const long MaxFileSize = 2 * 1024 * 1024; // 2MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

    /// <summary>
    /// Initializes a new instance of the <see cref="AvatarService"/> class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="webHostEnvironment">Web host environment for file paths</param>
    public AvatarService(ILogger<AvatarService> logger, Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
    {
        this.logger = logger;
        this.uploadsPath = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "avatars");

        // Ensure directory exists
        if (!Directory.Exists(this.uploadsPath))
        {
            Directory.CreateDirectory(this.uploadsPath);
            this.logger.LogInformation("Created avatars directory at {Path}", this.uploadsPath);
        }
    }

    /// <inheritdoc/>
    public bool ValidateAvatarFile(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (file == null || file.Length == 0)
        {
            errorMessage = "No file provided";
            return false;
        }

        if (file.Length > MaxFileSize)
        {
            errorMessage = $"File size exceeds maximum limit of {MaxFileSize / 1024 / 1024}MB";
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            errorMessage = $"Invalid file format. Allowed formats: {string.Join(", ", AllowedExtensions)}";
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<string?> UploadAvatarAsync(int userId, IFormFile file)
    {
        this.logger.LogInformation("Uploading avatar for user {UserId}", userId);

        if (!this.ValidateAvatarFile(file, out var errorMessage))
        {
            this.logger.LogWarning("Avatar validation failed for user {UserId}: {Error}", userId, errorMessage);
            return null;
        }

        try
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}_avatar{extension}";
            var filePath = Path.Combine(this.uploadsPath, fileName);

            // Delete old avatar if exists
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                this.logger.LogInformation("Deleted old avatar for user {UserId}", userId);
            }

            // Save new avatar
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"/uploads/avatars/{fileName}";
            this.logger.LogInformation("Avatar uploaded successfully for user {UserId}: {AvatarUrl}", userId, avatarUrl);

            return avatarUrl;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error uploading avatar for user {UserId}", userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAvatarAsync(string avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            return;
        }

        try
        {
            // Extract filename from URL
            var fileName = Path.GetFileName(avatarUrl);
            var filePath = Path.Combine(this.uploadsPath, fileName);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                this.logger.LogInformation("Deleted avatar file: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting avatar {AvatarUrl}", avatarUrl);
        }
    }
}
