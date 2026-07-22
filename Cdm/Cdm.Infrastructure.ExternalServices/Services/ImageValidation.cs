// -----------------------------------------------------------------------
// <copyright file="ImageValidation.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Services;

using System.Linq;

/// <summary>
/// Shared validation for uploaded images: size limit and format detection by magic bytes
/// (never trusts a client-provided content-type).
/// </summary>
public static class ImageValidation
{
    /// <summary>Maximum accepted image size, in bytes (5 MB).</summary>
    public const int MaxFileSizeBytes = 5 * 1024 * 1024;

    /// <summary>Accepted MIME types.</summary>
    public static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

    /// <summary>
    /// Validates raw image bytes. On success, returns the detected MIME type and file extension.
    /// </summary>
    /// <param name="bytes">The raw image bytes.</param>
    /// <param name="mimeType">The detected MIME type (empty on failure).</param>
    /// <param name="extension">The matching file extension including the dot (empty on failure).</param>
    /// <param name="error">A user-facing error message (null on success).</param>
    /// <returns><c>true</c> if the image is valid.</returns>
    public static bool TryValidate(byte[]? bytes, out string mimeType, out string extension, out string? error)
    {
        mimeType = string.Empty;
        extension = string.Empty;
        error = null;

        if (bytes is null || bytes.Length == 0)
        {
            error = "Image vide.";
            return false;
        }

        if (bytes.Length > MaxFileSizeBytes)
        {
            error = "Image trop lourde (maximum 5 Mo).";
            return false;
        }

        var mime = DetectMimeType(bytes);
        if (mime is null || !AllowedMimeTypes.Contains(mime))
        {
            error = "Format non supporté (JPEG, PNG ou WebP uniquement).";
            return false;
        }

        mimeType = mime;
        extension = ExtensionFor(mime);
        return true;
    }

    /// <summary>Detects the MIME type of an image from its magic bytes, or null if unrecognized.</summary>
    public static string? DetectMimeType(byte[] bytes)
    {
        if (bytes.Length < 12)
        {
            return null;
        }

        // JPEG: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
            bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
        {
            return "image/png";
        }

        // WebP: "RIFF"...."WEBP"
        if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
        {
            return "image/webp";
        }

        return null;
    }

    /// <summary>Returns the file extension (with dot) for a supported MIME type.</summary>
    public static string ExtensionFor(string mimeType) => mimeType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => ".jpg",
    };
}
