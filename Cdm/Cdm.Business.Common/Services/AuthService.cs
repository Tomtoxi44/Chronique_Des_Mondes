namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService? _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context,
        IPasswordService passwordService,
        IJwtService jwtService,
        ILogger<AuthService> logger,
        IEmailService? emailService = null)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Starting user registration for email: {Email}", request.Email);

            // Check if email already exists
            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
                return ServiceResult<RegisterResponse>.Failure("Email already exists");
            }

            // Hash password with BCrypt (work factor 12)
            var passwordHash = _passwordService.HashPassword(request.Password);

            // Create new user entity
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Save to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully with ID: {UserId}", user.Id);

            // Generate JWT token
            var token = _jwtService.GenerateToken(user.Id, user.Email);

            // Send welcome email (optional)
            if (_emailService != null)
            {
                try
                {
                    // TODO: Implement welcome email
                    // await _emailService.SendWelcomeEmailAsync(user.Email);
                    _logger.LogInformation("Welcome email sent to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
                    // Don't fail registration if email fails
                }
            }

            // Return success response
            var response = new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Token = token,
                Message = "Account created successfully"
            };

            return ServiceResult<RegisterResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            return ServiceResult<RegisterResponse>.Failure("An error occurred during registration");
        }
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Find user by email
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                return ServiceResult<LoginResponse>.Failure("Invalid email or password");
            }

            // Check if account is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: Account inactive for email {Email}", request.Email);
                return ServiceResult<LoginResponse>.Failure("Account is inactive");
            }

            // Verify password
            var isPasswordValid = _passwordService.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
                return ServiceResult<LoginResponse>.Failure("Invalid email or password");
            }

            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            // Generate JWT token
            var token = _jwtService.GenerateToken(user.Id, user.Email);

            // Return success response
            var response = new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Token = token,
                Message = "Login successful"
            };

            return ServiceResult<LoginResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return ServiceResult<LoginResponse>.Failure("An error occurred during login");
        }
    }
}
