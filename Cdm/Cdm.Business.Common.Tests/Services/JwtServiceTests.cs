// <copyright file="JwtServiceTests.cs" company="Chronique Des Mondes">
// Copyright (c) Chronique Des Mondes. All rights reserved.
// </copyright>

namespace Cdm.Business.Common.Tests.Services;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Cdm.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for JwtService.
/// </summary>
public class JwtServiceTests
{
    private readonly Mock<IConfiguration> mockConfiguration;
    private readonly Mock<ILogger<JwtService>> mockLogger;
    private readonly JwtService jwtService;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtServiceTests"/> class.
    /// </summary>
    public JwtServiceTests()
    {
        this.mockConfiguration = new Mock<IConfiguration>();
        this.mockLogger = new Mock<ILogger<JwtService>>();

        // Configure mock to return a valid JWT secret key
        var mockJwtSection = new Mock<IConfigurationSection>();
        mockJwtSection.Setup(x => x.Value).Returns("ThisIsAVerySecureSecretKeyForJwtTokenGeneration123456");
        this.mockConfiguration.Setup(x => x.GetSection("Jwt:SecretKey")).Returns(mockJwtSection.Object);

        this.jwtService = new JwtService(this.mockConfiguration.Object, this.mockLogger.Object);
    }

    /// <summary>
    /// Tests that GenerateToken includes all role claims in the token.
    /// </summary>
    [Fact]
    public void GenerateToken_WithMultipleRoles_IncludesAllRoleClaims()
    {
        // Arrange
        int userId = 123;
        string email = "test@test.com";
        var roles = new List<string> { "Player", "GameMaster" };

        // Act
        var token = this.jwtService.GenerateToken(userId, email, roles);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Equal(2, roleClaims.Count);
        Assert.Contains(roleClaims, c => c.Value == "Player");
        Assert.Contains(roleClaims, c => c.Value == "GameMaster");
    }

    /// <summary>
    /// Tests that GenerateToken works with a single role.
    /// </summary>
    [Fact]
    public void GenerateToken_WithSingleRole_IncludesRoleClaim()
    {
        // Arrange
        int userId = 456;
        string email = "player@test.com";
        var roles = new List<string> { "Player" };

        // Act
        var token = this.jwtService.GenerateToken(userId, email, roles);

        // Assert
        Assert.NotNull(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Single(roleClaims);
        Assert.Equal("Player", roleClaims[0].Value);
    }

    /// <summary>
    /// Tests that GenerateToken works with no roles.
    /// </summary>
    [Fact]
    public void GenerateToken_WithNoRoles_CreatesValidToken()
    {
        // Arrange
        int userId = 789;
        string email = "norole@test.com";
        var roles = new List<string>();

        // Act
        var token = this.jwtService.GenerateToken(userId, email, roles);

        // Assert
        Assert.NotNull(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Empty(roleClaims);

        // Verify user ID and email claims are still present
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

        Assert.NotNull(userIdClaim);
        Assert.Equal(userId.ToString(), userIdClaim.Value);
        Assert.NotNull(emailClaim);
        Assert.Equal(email, emailClaim.Value);
    }

    /// <summary>
    /// Tests that GetUserInfoFromToken extracts roles correctly.
    /// </summary>
    [Fact]
    public void GetUserInfoFromToken_WithMultipleRoles_ReturnsAllRoles()
    {
        // Arrange
        int userId = 999;
        string email = "admin@test.com";
        var roles = new List<string> { "Player", "GameMaster", "Admin" };

        var token = this.jwtService.GenerateToken(userId, email, roles);

        // Act
        var (extractedUserId, extractedEmail, extractedRoles) = this.jwtService.GetUserInfoFromToken(token);

        // Assert
        Assert.Equal(userId, extractedUserId);
        Assert.Equal(email, extractedEmail);
        Assert.NotNull(extractedRoles);
        Assert.Equal(3, extractedRoles.Count);
        Assert.Contains("Player", extractedRoles);
        Assert.Contains("GameMaster", extractedRoles);
        Assert.Contains("Admin", extractedRoles);
    }

    /// <summary>
    /// Tests that GetUserInfoFromToken returns empty list when no roles in token.
    /// </summary>
    [Fact]
    public void GetUserInfoFromToken_WithNoRoles_ReturnsEmptyRoleList()
    {
        // Arrange
        int userId = 111;
        string email = "norole@test.com";
        var roles = new List<string>();

        var token = this.jwtService.GenerateToken(userId, email, roles);

        // Act
        var (extractedUserId, extractedEmail, extractedRoles) = this.jwtService.GetUserInfoFromToken(token);

        // Assert
        Assert.Equal(userId, extractedUserId);
        Assert.Equal(email, extractedEmail);
        Assert.NotNull(extractedRoles);
        Assert.Empty(extractedRoles);
    }

    /// <summary>
    /// Tests that token contains standard claims.
    /// </summary>
    [Fact]
    public void GenerateToken_IncludesStandardClaims()
    {
        // Arrange
        int userId = 222;
        string email = "standard@test.com";
        var roles = new List<string> { "Player" };

        // Act
        var token = this.jwtService.GenerateToken(userId, email, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Verify standard claims exist
        Assert.NotNull(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier));
        Assert.NotNull(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email));
        Assert.NotNull(jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti));
    }
}
