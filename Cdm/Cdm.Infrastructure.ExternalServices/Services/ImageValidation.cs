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
        => TryValidate(bytes, ImagePolicy.Default, out mimeType, out extension, out error);

    /// <summary>
    /// Validates raw image bytes against a usage-specific policy (portraits are capped more
    /// tightly than free illustrations — see <see cref="ImagePolicy"/>).
    /// </summary>
    /// <param name="bytes">The raw image bytes.</param>
    /// <param name="policy">The limits to enforce.</param>
    /// <param name="mimeType">The detected MIME type (empty on failure).</param>
    /// <param name="extension">The matching file extension including the dot (empty on failure).</param>
    /// <param name="error">A user-facing error message (null on success).</param>
    /// <returns><c>true</c> if the image is valid.</returns>
    public static bool TryValidate(byte[]? bytes, ImagePolicy policy, out string mimeType, out string extension, out string? error)
    {
        mimeType = string.Empty;
        extension = string.Empty;
        error = null;

        if (bytes is null || bytes.Length == 0)
        {
            error = "Image vide.";
            return false;
        }

        if (bytes.Length > policy.MaxFileSizeBytes)
        {
            error = policy.TooHeavyMessage();
            return false;
        }

        var mime = DetectMimeType(bytes);
        if (mime is null || !AllowedMimeTypes.Contains(mime))
        {
            error = "Format non supporté (JPEG, PNG ou WebP uniquement).";
            return false;
        }

        // Dimensions : on lit l'en-tête plutôt que de décoder l'image (pas de dépendance
        // graphique côté serveur). Si le format ne se laisse pas lire, on laisse passer —
        // le poids reste borné de toute façon.
        if (policy.MaxDimension > 0
            && TryReadDimensions(bytes, mime, out var width, out var height)
            && (width > policy.MaxDimension || height > policy.MaxDimension))
        {
            error = policy.TooLargeMessage(width, height);
            return false;
        }

        mimeType = mime;
        extension = ExtensionFor(mime);
        return true;
    }

    /// <summary>
    /// Reads an image's pixel dimensions from its header. Supports the three accepted
    /// formats; returns <c>false</c> when the header is truncated or unrecognized.
    /// </summary>
    public static bool TryReadDimensions(byte[] bytes, string mimeType, out int width, out int height)
    {
        width = 0;
        height = 0;

        switch (mimeType)
        {
            case "image/png":
                // IHDR : largeur et hauteur en big-endian aux octets 16..23.
                if (bytes.Length < 24)
                {
                    return false;
                }

                width = ReadBigEndianInt32(bytes, 16);
                height = ReadBigEndianInt32(bytes, 20);
                return width > 0 && height > 0;

            case "image/jpeg":
                return TryReadJpegDimensions(bytes, out width, out height);

            case "image/webp":
                return TryReadWebpDimensions(bytes, out width, out height);

            default:
                return false;
        }
    }

    private static bool TryReadJpegDimensions(byte[] bytes, out int width, out int height)
    {
        width = 0;
        height = 0;

        // On parcourt les segments jusqu'au marqueur SOFn, qui porte les dimensions.
        var i = 2;
        while (i + 9 < bytes.Length)
        {
            if (bytes[i] != 0xFF)
            {
                i++;
                continue;
            }

            var marker = bytes[i + 1];
            var segmentLength = (bytes[i + 2] << 8) | bytes[i + 3];

            // SOF0..SOF15, en excluant DHT (C4), JPG (C8) et DAC (CC) qui partagent la plage.
            if (marker >= 0xC0 && marker <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC)
            {
                height = (bytes[i + 5] << 8) | bytes[i + 6];
                width = (bytes[i + 7] << 8) | bytes[i + 8];
                return width > 0 && height > 0;
            }

            if (segmentLength <= 0)
            {
                return false;
            }

            i += 2 + segmentLength;
        }

        return false;
    }

    private static bool TryReadWebpDimensions(byte[] bytes, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (bytes.Length < 30)
        {
            return false;
        }

        // Le format du bloc dépend de la variante : VP8 (lossy), VP8L (lossless), VP8X (étendu).
        var variant = System.Text.Encoding.ASCII.GetString(bytes, 12, 4);
        switch (variant)
        {
            case "VP8 ":
                width = ((bytes[27] << 8) | bytes[26]) & 0x3FFF;
                height = ((bytes[29] << 8) | bytes[28]) & 0x3FFF;
                return width > 0 && height > 0;

            case "VP8L":
                var bits = bytes[21] | (bytes[22] << 8) | (bytes[23] << 16) | (bytes[24] << 24);
                width = (bits & 0x3FFF) + 1;
                height = ((bits >> 14) & 0x3FFF) + 1;
                return true;

            case "VP8X":
                width = (bytes[24] | (bytes[25] << 8) | (bytes[26] << 16)) + 1;
                height = (bytes[27] | (bytes[28] << 8) | (bytes[29] << 16)) + 1;
                return true;

            default:
                return false;
        }
    }

    private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

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
