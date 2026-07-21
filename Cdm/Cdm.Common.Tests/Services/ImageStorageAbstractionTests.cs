// -----------------------------------------------------------------------
// <copyright file="ImageStorageAbstractionTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Tests.Services;

using Cdm.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="ImageValidation"/> and <see cref="LocalImageStorage"/>.
/// </summary>
public class ImageStorageAbstractionTests : IDisposable
{
    private readonly string tempRoot = Path.Combine(Path.GetTempPath(), "cdm-img-tests", Guid.NewGuid().ToString("N"));

    private static byte[] Png() => new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0, 1, 2, 3 };

    private static byte[] Jpeg() => new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };

    private static byte[] Webp() => new byte[] { 0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50, 1 };

    private LocalImageStorage NewStorage() =>
        new(new Mock<ILogger<LocalImageStorage>>().Object) { RootPath = this.tempRoot };

    [Theory]
    [InlineData("png")]
    [InlineData("jpeg")]
    [InlineData("webp")]
    public void TryValidate_KnownFormats_Succeeds(string format)
    {
        var bytes = format switch { "png" => Png(), "jpeg" => Jpeg(), _ => Webp() };

        var ok = ImageValidation.TryValidate(bytes, out var mime, out var ext, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.False(string.IsNullOrEmpty(mime));
        Assert.StartsWith(".", ext);
    }

    [Fact]
    public void TryValidate_Empty_Fails()
    {
        Assert.False(ImageValidation.TryValidate(Array.Empty<byte>(), out _, out _, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void TryValidate_UnknownBytes_Fails()
    {
        var junk = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        Assert.False(ImageValidation.TryValidate(junk, out _, out _, out _));
    }

    [Fact]
    public void TryValidate_TooLarge_Fails()
    {
        var big = new byte[ImageValidation.MaxFileSizeBytes + 1];
        big[0] = 0xFF; big[1] = 0xD8; big[2] = 0xFF; // JPEG header
        Assert.False(ImageValidation.TryValidate(big, out _, out _, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public async Task UploadAsync_ValidImage_WritesFileAndReturnsUrl()
    {
        var storage = this.NewStorage();

        var result = await storage.UploadAsync(Png(), "items", "42");

        Assert.True(result.Success);
        Assert.NotNull(result.Url);
        Assert.StartsWith("/uploads/items/", result.Url);
        Assert.EndsWith(".png", result.Url);

        var physical = Path.Combine(this.tempRoot, result.Url!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(physical));
    }

    [Fact]
    public async Task UploadAsync_InvalidImage_FailsWithoutWriting()
    {
        var storage = this.NewStorage();

        var result = await storage.UploadAsync(new byte[] { 1, 2, 3 }, "items", "42");

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.False(Directory.Exists(Path.Combine(this.tempRoot, "uploads", "items")));
    }

    [Fact]
    public async Task UploadAsync_SanitizesCategoryAndKey()
    {
        var storage = this.NewStorage();

        var result = await storage.UploadAsync(Png(), "../evil", "a/b c", default);

        Assert.True(result.Success);
        // No path traversal, no slashes/spaces in the resulting URL segment.
        Assert.DoesNotContain("..", result.Url);
        Assert.StartsWith("/uploads/evil/", result.Url);
    }

    [Fact]
    public async Task DeleteAsync_RemovesStoredFile()
    {
        var storage = this.NewStorage();
        var upload = await storage.UploadAsync(Jpeg(), "maps", "7");
        var physical = Path.Combine(this.tempRoot, upload.Url!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(physical));

        var deleted = await storage.DeleteAsync(upload.Url!);

        Assert.True(deleted);
        Assert.False(File.Exists(physical));
    }

    [Fact]
    public async Task DeleteAsync_EmptyOrForeignUrl_ReturnsTrueNoop()
    {
        var storage = this.NewStorage();
        Assert.True(await storage.DeleteAsync(string.Empty));
        Assert.True(await storage.DeleteAsync("https://cdn.example.com/x.png"));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(this.tempRoot))
            {
                Directory.Delete(this.tempRoot, true);
            }
        }
        catch
        {
            // best effort cleanup
        }

        GC.SuppressFinalize(this);
    }
}
