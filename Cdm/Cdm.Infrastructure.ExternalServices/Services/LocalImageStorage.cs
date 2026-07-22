// -----------------------------------------------------------------------
// <copyright file="LocalImageStorage.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Services;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

/// <summary>
/// Stores images on the local filesystem under <c>{RootPath}/uploads/{category}/</c> and
/// serves them from <c>/uploads/{category}/</c>. Default provider for development and CI.
/// </summary>
public class LocalImageStorage(ILogger<LocalImageStorage> logger) : IImageStorage
{
    private readonly ILogger<LocalImageStorage> logger = logger;

    /// <summary>Physical root that maps to the web root serving <c>/uploads</c>. Overridable for tests.</summary>
    public string RootPath { get; init; } = "wwwroot";

    /// <inheritdoc/>
    public async Task<ImageUploadResult> UploadAsync(byte[] imageBytes, string category, string entityKey, CancellationToken cancellationToken = default)
    {
        // Les limites dépendent de l'usage : un portrait est bridé, une carte non.
        if (!ImageValidation.TryValidate(imageBytes, ImagePolicy.For(category), out _, out var extension, out var error))
        {
            return ImageUploadResult.Fail(error!);
        }

        try
        {
            var cat = Sanitize(category);
            var key = Sanitize(entityKey);
            var directory = Path.Combine(this.RootPath, "uploads", cat);
            Directory.CreateDirectory(directory);

            var fileName = $"{key}-{Guid.NewGuid():N}{extension}";
            await File.WriteAllBytesAsync(Path.Combine(directory, fileName), imageBytes, cancellationToken);

            var url = $"/uploads/{cat}/{fileName}";
            this.logger.LogInformation("Stored image {Url} ({Size} bytes)", url, imageBytes.Length);
            return ImageUploadResult.Ok(url);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error storing local image for category {Category}", category);
            return ImageUploadResult.Fail("Erreur lors de l'enregistrement de l'image.");
        }
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/uploads/", StringComparison.Ordinal))
        {
            return Task.FromResult(true);
        }

        try
        {
            var relative = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var path = Path.Combine(this.RootPath, relative);
            if (File.Exists(path))
            {
                File.Delete(path);
                this.logger.LogInformation("Deleted local image {Url}", imageUrl);
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting local image {Url}", imageUrl);
            return Task.FromResult(false);
        }
    }

    /// <summary>Keeps only URL-safe characters to avoid path traversal or odd file names.</summary>
    private static string Sanitize(string value)
    {
        var cleaned = new string((value ?? string.Empty)
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray());
        return string.IsNullOrEmpty(cleaned) ? "x" : cleaned.ToLowerInvariant();
    }
}
