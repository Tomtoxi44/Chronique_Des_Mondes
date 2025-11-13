// -----------------------------------------------------------------------
// <copyright file="ImageStorageService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Services;

using Cdm.Common.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing image storage operations.
/// </summary>
/// <param name="logger">Logger instance for structured logging.</param>
public class ImageStorageService(ILogger<ImageStorageService> logger) : IImageStorageService
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private readonly ILogger<ImageStorageService> logger = logger;

    /// <inheritdoc/>
    public async Task<string?> UploadCampaignCoverAsync(string base64Image, int campaignId)
    {
        try
        {
            this.logger.LogInformation("Starting upload for campaign {CampaignId}", campaignId);

            // Decode Base64 string
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64Image);
            }
            catch (FormatException ex)
            {
                this.logger.LogWarning(ex, "Invalid Base64 format for campaign {CampaignId}", campaignId);
                return null;
            }

            // Validate size
            if (imageBytes.Length > MaxFileSizeBytes)
            {
                this.logger.LogWarning(
                    "Image size {Size} bytes exceeds limit {Limit} bytes for campaign {CampaignId}",
                    imageBytes.Length,
                    MaxFileSizeBytes,
                    campaignId);
                return null;
            }

            // Validate image format
            var mimeType = this.GetImageMimeType(imageBytes);
            if (mimeType == null || !AllowedMimeTypes.Contains(mimeType))
            {
                this.logger.LogWarning(
                    "Invalid or unsupported MIME type {MimeType} for campaign {CampaignId}",
                    mimeType ?? "unknown",
                    campaignId);
                return null;
            }

            // Get file extension from MIME type
            var extension = this.GetExtensionFromMimeType(mimeType);

            // Generate unique filename
            var fileName = $"campaign-{campaignId}-{Guid.NewGuid()}{extension}";

            // Determine upload path
            var uploadsDirectory = Path.Combine("wwwroot", "uploads", "campaigns");
            Directory.CreateDirectory(uploadsDirectory);

            var filePath = Path.Combine(uploadsDirectory, fileName);

            // Save file
            await File.WriteAllBytesAsync(filePath, imageBytes);

            this.logger.LogInformation(
                "Successfully uploaded image {FileName} for campaign {CampaignId}",
                fileName,
                campaignId);

            // Return relative URL
            return $"/uploads/campaigns/{fileName}";
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error uploading image for campaign {CampaignId}",
                campaignId);
            return null;
        }
    }

    /// <inheritdoc/>
    public Task<bool> DeleteCampaignCoverAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return Task.FromResult(true);
            }

            // Convert URL to file path
            var relativePath = imageUrl.TrimStart('/');
            var filePath = Path.Combine("wwwroot", relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(filePath))
            {
                this.logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return Task.FromResult(false);
            }

            File.Delete(filePath);
            this.logger.LogInformation("Successfully deleted image {FilePath}", filePath);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting image {ImageUrl}", imageUrl);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Determines the MIME type of an image from its byte array.
    /// </summary>
    /// <param name="imageBytes">Image byte array.</param>
    /// <returns>MIME type string or null if unrecognized.</returns>
    private string? GetImageMimeType(byte[] imageBytes)
    {
        if (imageBytes.Length < 12)
        {
            return null;
        }

        // JPEG: FF D8 FF
        if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (imageBytes[0] == 0x89 &&
            imageBytes[1] == 0x50 &&
            imageBytes[2] == 0x4E &&
            imageBytes[3] == 0x47 &&
            imageBytes[4] == 0x0D &&
            imageBytes[5] == 0x0A &&
            imageBytes[6] == 0x1A &&
            imageBytes[7] == 0x0A)
        {
            return "image/png";
        }

        // WebP: 52 49 46 46 [4 bytes] 57 45 42 50
        if (imageBytes[0] == 0x52 &&
            imageBytes[1] == 0x49 &&
            imageBytes[2] == 0x46 &&
            imageBytes[3] == 0x46 &&
            imageBytes[8] == 0x57 &&
            imageBytes[9] == 0x45 &&
            imageBytes[10] == 0x42 &&
            imageBytes[11] == 0x50)
        {
            return "image/webp";
        }

        return null;
    }

    /// <summary>
    /// Gets the file extension from a MIME type.
    /// </summary>
    /// <param name="mimeType">MIME type string.</param>
    /// <returns>File extension with dot prefix.</returns>
    private string GetExtensionFromMimeType(string mimeType)
    {
        return mimeType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }
}
