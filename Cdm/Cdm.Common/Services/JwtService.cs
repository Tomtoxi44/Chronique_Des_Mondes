namespace Cdm.Common.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
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
    /// <returns>JWT token string</returns>
    string GenerateToken(int userId, string email);

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
    /// <returns>User information if valid, null otherwise</returns>
    (int UserId, string Email)? GetUserInfoFromToken(string token);
}

/// <summary>
/// JWT service implementation
/// </summary>
public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationDays;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _logger = logger;
        
        // Get configuration from appsettings or User Secrets
        _secretKey = configuration["Jwt:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
        _issuer = configuration["Jwt:Issuer"] ?? "ChroniqueDesmonds";
        _audience = configuration["Jwt:Audience"] ?? "ChroniqueDesMondesWeb";
        _expirationDays = int.TryParse(configuration["Jwt:ExpirationDays"], out var days) ? days : 7;

        if (_secretKey.Length < 32)
        {
            _logger.LogWarning("JWT secret key is less than 32 characters, which is not secure for production");
        }
    }

    public string GenerateToken(int userId, string email)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(_expirationDays),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            _logger.LogInformation("JWT token generated for user {UserId}", userId);
            
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate JWT token for user {UserId}", userId);
            throw;
        }
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            _logger.LogDebug("Token validated successfully");
            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Token expired");
            return false;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogError("Invalid token signature");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return false;
        }
    }

    public (int UserId, string Email)? GetUserInfoFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;

            if (int.TryParse(userIdClaim, out var userId) && !string.IsNullOrEmpty(userEmail))
            {
                _logger.LogDebug("Successfully extracted user info from token: UserId={UserId}", userId);
                return (userId, userEmail);
            }
            
            _logger.LogWarning("Failed to extract complete user info from token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract user info from token");
            return null;
        }
    }
}
