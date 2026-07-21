// -----------------------------------------------------------------------
// <copyright file="IImageStorage.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Services;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Result of an image upload.
/// </summary>
/// <param name="Success">Whether the upload succeeded.</param>
/// <param name="Url">The public URL of the stored image (null on failure).</param>
/// <param name="Error">A user-facing error message (null on success).</param>
public record ImageUploadResult(bool Success, string? Url, string? Error)
{
    /// <summary>Creates a failed result carrying an error message.</summary>
    public static ImageUploadResult Fail(string error) => new(false, null, error);

    /// <summary>Creates a successful result carrying the stored URL.</summary>
    public static ImageUploadResult Ok(string url) => new(true, url, null);
}

/// <summary>
/// Abstraction over image storage so the backing store (local disk in dev, Azure Blob in prod)
/// is transparent to callers. Images are grouped by a logical <c>category</c>
/// (e.g. "avatars", "items", "maps") and named after an <c>entityKey</c>.
/// </summary>
public interface IImageStorage
{
    /// <summary>
    /// Validates and stores an image, returning its public URL.
    /// </summary>
    /// <param name="imageBytes">The raw image bytes.</param>
    /// <param name="category">Logical category / folder (e.g. "items").</param>
    /// <param name="entityKey">A key used in the file name (e.g. the entity id).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload result.</returns>
    Task<ImageUploadResult> UploadAsync(byte[] imageBytes, string category, string entityKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a previously stored image by its public URL. No-op (returns true) for empty URLs.
    /// </summary>
    /// <param name="imageUrl">The public URL returned by <see cref="UploadAsync"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the image was deleted or did not need deleting.</returns>
    Task<bool> DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);
}
