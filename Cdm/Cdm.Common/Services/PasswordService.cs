namespace Cdm.Common.Services;

using BCrypt.Net;
using Microsoft.Extensions.Logging;

/// <summary>
/// Password service interface for hashing and verification
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hash a password using BCrypt with work factor 12
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Hashed password</param>
    /// <returns>True if password matches hash</returns>
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// Password service implementation using BCrypt with work factor 12
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(ILogger<PasswordService> logger)
    {
        _logger = logger;
    }

    public string HashPassword(string password)
    {
        try
        {
            var hash = BCrypt.HashPassword(password, WorkFactor);
            _logger.LogDebug("Password hashed successfully with work factor {WorkFactor}", WorkFactor);
            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hash password");
            throw;
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var isValid = BCrypt.Verify(password, hash);
            _logger.LogDebug("Password verification result: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify password");
            return false;
        }
    }
}
