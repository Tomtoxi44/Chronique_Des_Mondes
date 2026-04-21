namespace Cdm.Common.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

/// <summary>
/// JWT service interface for token generation and validation
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate a JWT token for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="email">User email</param>
    /// <param name="roles">List of role names assigned to the user</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(int userId, string email, List<string> roles, string? nickname = null);

    /// <summary>
    /// Validate a JWT token
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>True if token is valid</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Extract user information from a JWT token
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>User information with roles if valid, null otherwise</returns>
    (int UserId, string Email, List<string> Roles)? GetUserInfoFromToken(string token);
}

/// <summary>
/// JWT service implementation
/// </summary>
public class JwtService : IJwtService
{
    private readonly string secretKey;
    private readonly string issuer;
    private readonly string audience;
    private readonly int expirationHours;
    private readonly ILogger<JwtService> logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Get configuration from appsettings or User Secrets - fail-fast if missing
        this.secretKey = configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT secret key configuration ('Jwt:SecretKey') is missing. Please set a secure value in your configuration.");
        this.issuer = configuration["Jwt:Issuer"] ?? "ChroniqueDesMondes";
        this.audience = configuration["Jwt:Audience"] ?? "ChroniqueDesMondesWeb";
        this.expirationHours = int.TryParse(configuration["Jwt:ExpirationHours"], out var hours) ? hours : 1;

        if (this.secretKey.Length < 32)
        {
            this.logger.LogWarning("JWT secret key is less than 32 characters, which is not secure for production");
        }
    }

    public string GenerateToken(int userId, string email, List<string> roles, string? nickname = null)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.secretKey);
            
            // Build claims list with user info
            var claimsList = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, nickname ?? email),
                new Claim(JwtRegisteredClaimNames.Name, nickname ?? email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claimsList.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimsList),
                Expires = DateTime.UtcNow.AddHours(this.expirationHours),
                Issuer = this.issuer,
                Audience = this.audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            this.logger.LogInformation("JWT token generated for user {UserId} with roles: {Roles}", userId, string.Join(", ", roles));
            
            return tokenString;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to generate JWT token for user {UserId}", userId);
            throw;
        }
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.secretKey);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = this.issuer,
                ValidateAudience = true,
                ValidAudience = this.audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            this.logger.LogDebug("Token validated successfully");
            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            this.logger.LogWarning("Token expired");
            return false;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            this.logger.LogError("Invalid token signature");
            return false;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Token validation failed");
            return false;
        }
    }

    public (int UserId, string Email, List<string> Roles)? GetUserInfoFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.secretKey);
            
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = this.issuer,
                ValidateAudience = true,
                ValidAudience = this.audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (int.TryParse(userIdClaim, out var userId) && !string.IsNullOrEmpty(userEmail))
            {
                this.logger.LogDebug("Successfully extracted user info from token: UserId={UserId}, Roles={Roles}", userId, string.Join(", ", roles));
                return (userId, userEmail, roles);
            }
            
            this.logger.LogWarning("Failed to extract complete user info from token");
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to extract user info from token");
            return null;
        }
    }
}
