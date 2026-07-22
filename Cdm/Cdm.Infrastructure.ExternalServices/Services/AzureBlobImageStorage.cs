// -----------------------------------------------------------------------
// <copyright file="AzureBlobImageStorage.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Services;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// Stores images in an Azure Blob Storage container. The container is expected to allow
/// anonymous blob read (so images display via their URL); writes/deletes go through the
/// injected <see cref="BlobContainerClient"/>, authenticated via managed identity in
/// production. Blob names use unguessable GUIDs.
/// </summary>
public class AzureBlobImageStorage(BlobContainerClient container, ILogger<AzureBlobImageStorage> logger) : IImageStorage
{
    private readonly BlobContainerClient container = container;
    private readonly ILogger<AzureBlobImageStorage> logger = logger;

    /// <inheritdoc/>
    public async Task<ImageUploadResult> UploadAsync(byte[] imageBytes, string category, string entityKey, CancellationToken cancellationToken = default)
    {
        if (!ImageValidation.TryValidate(imageBytes, out var mimeType, out var extension, out var error))
        {
            return ImageUploadResult.Fail(error!);
        }

        try
        {
            var blobName = $"{Sanitize(category)}/{Sanitize(entityKey)}-{Guid.NewGuid():N}{extension}";
            var blob = this.container.GetBlobClient(blobName);

            using var stream = new MemoryStream(imageBytes, writable: false);
            await blob.UploadAsync(
                stream,
                new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = mimeType } },
                cancellationToken);

            var url = blob.Uri.ToString();
            this.logger.LogInformation("Stored blob image {BlobName} ({Size} bytes)", blobName, imageBytes.Length);
            return ImageUploadResult.Ok(url);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error storing blob image for category {Category}", category);
            return ImageUploadResult.Fail("Erreur lors de l'enregistrement de l'image.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return true;
        }

        try
        {
            // Blob name is the URL path after the container name.
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            {
                return true;
            }

            var prefix = $"/{this.container.Name}/";
            var path = Uri.UnescapeDataString(uri.AbsolutePath);
            var idx = path.IndexOf(prefix, StringComparison.Ordinal);
            if (idx < 0)
            {
                // Not a blob of this container (e.g. a legacy local /uploads URL): nothing to delete here.
                return true;
            }

            var blobName = path[(idx + prefix.Length)..];
            await this.container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: cancellationToken);
            this.logger.LogInformation("Deleted blob image {BlobName}", blobName);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting blob image {Url}", imageUrl);
            return false;
        }
    }

    private static string Sanitize(string value)
    {
        var cleaned = new string((value ?? string.Empty)
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray());
        return string.IsNullOrEmpty(cleaned) ? "x" : cleaned.ToLowerInvariant();
    }
}
