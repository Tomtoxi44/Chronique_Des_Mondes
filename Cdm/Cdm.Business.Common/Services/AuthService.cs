namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
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
    private readonly AppDbContext context;
    private readonly IPasswordService passwordService;
    private readonly IJwtService jwtService;
    private readonly IEmailService? emailService;
    private readonly ILogger<AuthService> logger;

    public AuthService(
        AppDbContext context,
        IPasswordService passwordService,
        IJwtService jwtService,
        ILogger<AuthService> logger,
        IEmailService? emailService = null)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        this.jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.emailService = emailService;
    }

    public async Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            this.logger.LogInformation("Starting user registration for email: {Email}", request.Email);

            // Check if email already exists
            var existingUser = await this.context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                this.logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
                return ServiceResult<RegisterResponse>.Failure("Email already exists");
            }

            // Hash password with BCrypt (work factor 12)
            var passwordHash = this.passwordService.HashPassword(request.Password);

            // Create new user entity
            var user = new User
            {
                Email = request.Email,
                Nickname = request.Nickname,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Save to database
            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();

            this.logger.LogInformation("User registered successfully with ID: {UserId}", user.Id);

            // Assign default roles: Player + GameMaster (all registered users can manage their own campaigns)
            var roles = await this.context.Roles
                .Where(r => r.Name == "Player" || r.Name == "GameMaster")
                .ToListAsync();

            foreach (var role in roles)
            {
                this.context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }

            if (roles.Count > 0)
            {
                await this.context.SaveChangesAsync();
                this.logger.LogInformation("Roles [{Roles}] assigned to user {UserId}",
                    string.Join(", ", roles.Select(r => r.Name)), user.Id);
            }

            // Reuse already-loaded roles to generate JWT token (avoids a second DB query)
            var roleNames = roles.Select(r => r.Name).ToList();

            // Generate JWT token with roles
            var token = this.jwtService.GenerateToken(user.Id, user.Email, roleNames);

            // Send welcome email (optional)
            if (this.emailService != null)
            {
                try
                {
                    // TODO: Implement welcome email
                    // await this.emailService.SendWelcomeEmailAsync(user.Email);
                    this.logger.LogInformation("Welcome email sent to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
                    // Don't fail registration if email fails
                }
            }

            // Return success response
            var response = new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Nickname = user.Nickname,
                Token = token,
                Message = "Account created successfully"
            };

            return ServiceResult<RegisterResponse>.Success(response);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            return ServiceResult<RegisterResponse>.Failure("An error occurred during registration");
        }
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            this.logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Find user by email
            var user = await this.context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                this.logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                return ServiceResult<LoginResponse>.Failure("Invalid email or password");
            }

            // Check if account is active
            if (!user.IsActive)
            {
                this.logger.LogWarning("Login failed: Account inactive for email {Email}", request.Email);
                return ServiceResult<LoginResponse>.Failure("Account is inactive");
            }

            // Verify password
            var isPasswordValid = this.passwordService.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                this.logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
                return ServiceResult<LoginResponse>.Failure("Invalid email or password");
            }

            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            this.context.Users.Update(user);
            await this.context.SaveChangesAsync();

            this.logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            // Get user roles for JWT token
            var roles = await this.context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            // Generate JWT token with roles
            var token = this.jwtService.GenerateToken(user.Id, user.Email, roles);

            // Return success response
            var response = new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Nickname = user.Nickname,
                Token = token,
                Message = "Login successful"
            };

            return ServiceResult<LoginResponse>.Success(response);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return ServiceResult<LoginResponse>.Failure("An error occurred during login");
        }
    }
}

