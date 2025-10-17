// <copyright file="AvatarServiceTests.cs" company="Chronique Des Mondes">
// Copyright (c) Chronique Des Mondes. All rights reserved.
// </copyright>

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Common.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="AvatarService"/>.
/// </summary>
public class AvatarServiceTests : IDisposable
{
    private readonly Mock<ILogger<AvatarService>> loggerMock;
    private readonly Mock<IWebHostEnvironment> webHostEnvironmentMock;
    private readonly string testUploadsPath;
    private readonly AvatarService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvatarServiceTests"/> class.
    /// </summary>
    public AvatarServiceTests()
    {
        this.loggerMock = new Mock<ILogger<AvatarService>>();
        this.webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

        // Create a temporary test directory
        this.testUploadsPath = Path.Combine(Path.GetTempPath(), "AvatarServiceTests_" + Guid.NewGuid().ToString());
        var webRootPath = Path.Combine(this.testUploadsPath, "wwwroot");
        Directory.CreateDirectory(webRootPath);

        this.webHostEnvironmentMock.Setup(env => env.WebRootPath).Returns(webRootPath);

        this.service = new AvatarService(this.loggerMock.Object, this.webHostEnvironmentMock.Object);
    }

    /// <summary>
    /// Tests that ValidateAvatarFile returns false when file is null.
    /// </summary>
    [Fact]
    public void ValidateAvatarFile_FileIsNull_ReturnsFalse()
    {
        // Act
        var result = this.service.ValidateAvatarFile(null!, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Contains("No file provided", errorMessage);
    }

    /// <summary>
    /// Tests that ValidateAvatarFile returns false when file is empty.
    /// </summary>
    [Fact]
    public void ValidateAvatarFile_FileIsEmpty_ReturnsFalse()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var result = this.service.ValidateAvatarFile(fileMock.Object, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Contains("No file provided", errorMessage);
    }

    /// <summary>
    /// Tests that ValidateAvatarFile returns false when file exceeds max size.
    /// </summary>
    [Fact]
    public void ValidateAvatarFile_FileTooLarge_ReturnsFalse()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(3 * 1024 * 1024); // 3MB (exceeds 2MB limit)
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        // Act
        var result = this.service.ValidateAvatarFile(fileMock.Object, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Contains("File size exceeds maximum limit", errorMessage);
    }

    /// <summary>
    /// Tests that ValidateAvatarFile returns false when file has invalid extension.
    /// </summary>
    [Fact]
    public void ValidateAvatarFile_InvalidExtension_ReturnsFalse()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024); // 1KB
        fileMock.Setup(f => f.FileName).Returns("test.gif");

        // Act
        var result = this.service.ValidateAvatarFile(fileMock.Object, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Contains("Invalid file format", errorMessage);
    }

    /// <summary>
    /// Tests that ValidateAvatarFile returns true for valid JPG file.
    /// </summary>
    [Fact]
    public void ValidateAvatarFile_ValidJpg_ReturnsTrue()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024); // 1KB
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        // Act
        var result = this.service.ValidateAvatarFile(fileMock.Object, out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Empty(errorMessage);
    }

    /// <summary>
    /// Tests that ValidateAvatarFile returns true for valid PNG file.
    /// </summary>
    [Fact]
    public void ValidateAvatarFile_ValidPng_ReturnsTrue()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024); // 1KB
        fileMock.Setup(f => f.FileName).Returns("test.png");

        // Act
        var result = this.service.ValidateAvatarFile(fileMock.Object, out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Empty(errorMessage);
    }

    /// <summary>
    /// Tests that ValidateAvatarFile returns true for valid JPEG file.
    /// </summary>
    [Fact]
    public void ValidateAvatarFile_ValidJpeg_ReturnsTrue()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024); // 1KB
        fileMock.Setup(f => f.FileName).Returns("test.jpeg");

        // Act
        var result = this.service.ValidateAvatarFile(fileMock.Object, out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Empty(errorMessage);
    }

    /// <summary>
    /// Tests that ValidateAvatarFile is case insensitive for extensions.
    /// </summary>
    [Fact]
    public void ValidateAvatarFile_UppercaseExtension_ReturnsTrue()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024); // 1KB
        fileMock.Setup(f => f.FileName).Returns("test.JPG");

        // Act
        var result = this.service.ValidateAvatarFile(fileMock.Object, out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Empty(errorMessage);
    }

    /// <summary>
    /// Tests that UploadAvatarAsync creates file with correct naming pattern.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAvatarAsync_ValidFile_CreatesFileWithCorrectName()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var content = "Fake image content"u8.ToArray();
        var stream = new MemoryStream(content);

        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken token) =>
            {
                stream.CopyTo(target);
                return Task.CompletedTask;
            });

        // Act
        var result = await this.service.UploadAvatarAsync(123, fileMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/uploads/avatars/123_avatar.jpg", result);

        // Verify file was created
        var expectedFilePath = Path.Combine(this.webHostEnvironmentMock.Object.WebRootPath, "uploads", "avatars", "123_avatar.jpg");
        Assert.True(File.Exists(expectedFilePath));
    }

    /// <summary>
    /// Tests that UploadAvatarAsync returns null for invalid file.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAvatarAsync_InvalidFile_ReturnsNull()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0); // Invalid: empty file
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");

        // Act
        var result = await this.service.UploadAvatarAsync(123, fileMock.Object);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that UploadAvatarAsync replaces existing avatar file.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAvatarAsync_ExistingAvatar_ReplacesFile()
    {
        // Arrange
        var avatarsPath = Path.Combine(this.webHostEnvironmentMock.Object.WebRootPath, "uploads", "avatars");
        var existingFilePath = Path.Combine(avatarsPath, "456_avatar.jpg");
        
        // Create an existing file
        Directory.CreateDirectory(avatarsPath);
        await File.WriteAllTextAsync(existingFilePath, "Old content");

        var fileMock = new Mock<IFormFile>();
        var newContent = "New image content"u8.ToArray();
        var stream = new MemoryStream(newContent);

        fileMock.Setup(f => f.Length).Returns(newContent.Length);
        fileMock.Setup(f => f.FileName).Returns("newavatar.jpg");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken token) =>
            {
                stream.CopyTo(target);
                return Task.CompletedTask;
            });

        // Act
        var result = await this.service.UploadAvatarAsync(456, fileMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.True(File.Exists(existingFilePath));
        
        // Verify new content
        var fileContent = await File.ReadAllTextAsync(existingFilePath);
        Assert.Equal("New image content", fileContent);
    }

    /// <summary>
    /// Tests that DeleteAvatarAsync removes file when it exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteAvatarAsync_FileExists_RemovesFile()
    {
        // Arrange
        var avatarsPath = Path.Combine(this.webHostEnvironmentMock.Object.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(avatarsPath);
        
        var testFilePath = Path.Combine(avatarsPath, "789_avatar.jpg");
        await File.WriteAllTextAsync(testFilePath, "Test content");
        
        Assert.True(File.Exists(testFilePath));

        // Act
        await this.service.DeleteAvatarAsync("/uploads/avatars/789_avatar.jpg");

        // Assert
        Assert.False(File.Exists(testFilePath));
    }

    /// <summary>
    /// Tests that DeleteAvatarAsync handles non-existent file gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteAvatarAsync_FileDoesNotExist_NoException()
    {
        // Act & Assert (should not throw)
        await this.service.DeleteAvatarAsync("/uploads/avatars/nonexistent.jpg");
    }

    /// <summary>
    /// Tests that DeleteAvatarAsync handles null or empty URL gracefully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteAvatarAsync_NullUrl_NoException()
    {
        // Act & Assert (should not throw)
        await this.service.DeleteAvatarAsync(null!);
        await this.service.DeleteAvatarAsync(string.Empty);
        await this.service.DeleteAvatarAsync("   ");
    }

    /// <summary>
    /// Cleanup test directory after tests.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(this.testUploadsPath))
        {
            Directory.Delete(this.testUploadsPath, true);
        }
    }
}
