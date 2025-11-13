// -----------------------------------------------------------------------
// <copyright file="ImageStorageServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Tests.Services;

using Cdm.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="ImageStorageService"/>.
/// </summary>
public class ImageStorageServiceTests : IDisposable
{
    private readonly Mock<ILogger<ImageStorageService>> loggerMock;
    private readonly ImageStorageService service;
    private readonly string testDirectory = Path.Combine("wwwroot", "uploads", "campaigns");

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageStorageServiceTests"/> class.
    /// </summary>
    public ImageStorageServiceTests()
    {
        this.loggerMock = new Mock<ILogger<ImageStorageService>>();
        this.service = new ImageStorageService(this.loggerMock.Object);

        // Clean up test directory before each test
        if (Directory.Exists(this.testDirectory))
        {
            Directory.Delete(this.testDirectory, true);
        }
    }

    /// <summary>
    /// Tests that UploadCampaignCoverAsync successfully uploads a valid JPEG image.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UploadCampaignCoverAsync_ValidJpegImage_Success()
    {
        // Arrange - 1x1 transparent PNG (smallest valid PNG)
        var base64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
        var campaignId = 123;

        // Act
        var result = await this.service.UploadCampaignCoverAsync(base64Image, campaignId);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("/uploads/campaigns/campaign-", result);
        Assert.Contains($"campaign-{campaignId}", result);

        // Verify file was created
        var filePath = Path.Combine("wwwroot", result.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(filePath));
    }

    /// <summary>
    /// Tests that UploadCampaignCoverAsync returns null for invalid Base64 format.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UploadCampaignCoverAsync_InvalidBase64_ReturnsNull()
    {
        // Arrange
        var invalidBase64 = "this is not valid base64!@#$%";
        var campaignId = 456;

        // Act
        var result = await this.service.UploadCampaignCoverAsync(invalidBase64, campaignId);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that UploadCampaignCoverAsync returns null for invalid image format.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UploadCampaignCoverAsync_InvalidImageFormat_ReturnsNull()
    {
        // Arrange - Valid Base64 but not an image (plain text "Hello")
        var base64Text = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Hello"));
        var campaignId = 789;

        // Act
        var result = await this.service.UploadCampaignCoverAsync(base64Text, campaignId);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that UploadCampaignCoverAsync returns null when image exceeds size limit.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UploadCampaignCoverAsync_ExceedsSize_ReturnsNull()
    {
        // Arrange - Create a large byte array (6 MB, exceeds 5 MB limit)
        var largeImageBytes = new byte[6 * 1024 * 1024];
        
        // Create a valid PNG header
        largeImageBytes[0] = 0x89;
        largeImageBytes[1] = 0x50;
        largeImageBytes[2] = 0x4E;
        largeImageBytes[3] = 0x47;
        largeImageBytes[4] = 0x0D;
        largeImageBytes[5] = 0x0A;
        largeImageBytes[6] = 0x1A;
        largeImageBytes[7] = 0x0A;

        var base64Image = Convert.ToBase64String(largeImageBytes);
        var campaignId = 999;

        // Act
        var result = await this.service.UploadCampaignCoverAsync(base64Image, campaignId);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that DeleteCampaignCoverAsync successfully deletes an existing image.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteCampaignCoverAsync_ExistingImage_ReturnsTrue()
    {
        // Arrange - Create a test file
        Directory.CreateDirectory(this.testDirectory);
        var testFileName = "test-image.jpg";
        var testFilePath = Path.Combine(this.testDirectory, testFileName);
        await File.WriteAllTextAsync(testFilePath, "test content");

        var imageUrl = $"/uploads/campaigns/{testFileName}";

        // Act
        var result = await this.service.DeleteCampaignCoverAsync(imageUrl);

        // Assert
        Assert.True(result);
        Assert.False(File.Exists(testFilePath));
    }

    /// <summary>
    /// Tests that DeleteCampaignCoverAsync returns false for non-existent image.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteCampaignCoverAsync_NonExistentImage_ReturnsFalse()
    {
        // Arrange
        var imageUrl = "/uploads/campaigns/non-existent-image.jpg";

        // Act
        var result = await this.service.DeleteCampaignCoverAsync(imageUrl);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that DeleteCampaignCoverAsync returns true for null or empty URL.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteCampaignCoverAsync_NullOrEmptyUrl_ReturnsTrue()
    {
        // Act & Assert
        Assert.True(await this.service.DeleteCampaignCoverAsync(null!));
        Assert.True(await this.service.DeleteCampaignCoverAsync(string.Empty));
        Assert.True(await this.service.DeleteCampaignCoverAsync("   "));
    }

    /// <summary>
    /// Cleans up test files after each test.
    /// </summary>
    public void Dispose()
    {
        // Clean up test directory after each test
        if (Directory.Exists("wwwroot"))
        {
            Directory.Delete("wwwroot", true);
        }

        GC.SuppressFinalize(this);
    }
}
