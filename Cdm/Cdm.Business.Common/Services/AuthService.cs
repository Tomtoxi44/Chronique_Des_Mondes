namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

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

    public async Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request, string? confirmUrlTemplate = null)
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
            var token = this.jwtService.GenerateToken(user.Id, user.Email, roleNames, user.Nickname);

            // Generate and save refresh token
            var refreshToken = await this.CreateRefreshTokenAsync(user.Id);

            // Envoi de l'email de confirmation d'adresse (n'échoue pas l'inscription si l'envoi rate).
            if (!string.IsNullOrWhiteSpace(confirmUrlTemplate))
            {
                try
                {
                    await this.CreateAndSendConfirmationAsync(user, confirmUrlTemplate);
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning(ex, "Failed to send confirmation email to {Email}", user.Email);
                }
            }

            var response = new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Nickname = user.Nickname,
                Token = token,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiry = refreshToken.ExpiresAt,
                EmailConfirmed = user.EmailConfirmed,
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
            var token = this.jwtService.GenerateToken(user.Id, user.Email, roles, user.Nickname);

            // Generate and save refresh token
            var refreshToken = await this.CreateRefreshTokenAsync(user.Id);

            // Return success response
            var response = new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Nickname = user.Nickname,
                Token = token,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiry = refreshToken.ExpiresAt,
                EmailConfirmed = user.EmailConfirmed,
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

    public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var token = await this.context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null || !token.IsActive)
            {
                this.logger.LogWarning("Refresh token is invalid or expired");
                return ServiceResult<LoginResponse>.Failure("Invalid or expired refresh token");
            }

            var user = token.User;
            if (!user.IsActive)
            {
                return ServiceResult<LoginResponse>.Failure("Account is inactive");
            }

            // Revoke old refresh token
            token.RevokedAt = DateTime.UtcNow;
            this.context.RefreshTokens.Update(token);

            // Generate new tokens
            var roles = await this.context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            var newAccessToken = this.jwtService.GenerateToken(user.Id, user.Email, roles, user.Nickname);
            var newRefreshToken = await this.CreateRefreshTokenAsync(user.Id);

            await this.context.SaveChangesAsync();

            this.logger.LogInformation("Tokens refreshed for user {UserId}", user.Id);

            return ServiceResult<LoginResponse>.Success(new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Nickname = user.Nickname,
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiry = newRefreshToken.ExpiresAt,
                Message = "Token refreshed"
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during token refresh");
            return ServiceResult<LoginResponse>.Failure("An error occurred during token refresh");
        }
    }

    /// <summary>Durée de validité d'un lien de réinitialisation, en heures.</summary>
    private const int PasswordResetValidityHours = 1;

    public async Task<ServiceResult<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request, string resetUrlTemplate)
    {
        try
        {
            var user = await this.context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // Réponse volontairement identique que le compte existe ou non :
            // révéler l'inverse permettrait d'énumérer les comptes.
            if (user == null || !user.IsActive)
            {
                this.logger.LogInformation(
                    "Demande de réinitialisation pour une adresse inconnue ou inactive : {Email}",
                    request.Email);
                return ServiceResult<bool>.Success(true);
            }

            // Un seul lien actif à la fois : on invalide les précédents.
            var pending = await this.context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.UsedAt == null)
                .ToListAsync();

            foreach (var old in pending)
            {
                old.UsedAt = DateTime.UtcNow;
            }

            var tokenBytes = new byte[48];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }

            // Base64Url : le jeton transite dans une URL.
            var tokenString = Convert.ToBase64String(tokenBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            this.context.PasswordResetTokens.Add(new PasswordResetToken
            {
                Token = tokenString,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(PasswordResetValidityHours),
                CreatedAt = DateTime.UtcNow,
            });

            await this.context.SaveChangesAsync();

            var resetLink = resetUrlTemplate.Replace("{token}", Uri.EscapeDataString(tokenString));

            if (this.emailService != null)
            {
                await this.emailService.SendPasswordResetEmailAsync(
                    user.Email, user.Nickname, resetLink, PasswordResetValidityHours);
            }
            else
            {
                this.logger.LogWarning(
                    "Aucun service d'email enregistré : le lien de réinitialisation n'a pas pu être envoyé à {Email}",
                    user.Email);
            }

            this.logger.LogInformation("Lien de réinitialisation généré pour l'utilisateur {UserId}", user.Id);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Erreur lors de la demande de réinitialisation pour {Email}", request.Email);
            return ServiceResult<bool>.Failure("Une erreur est survenue lors de la demande de réinitialisation");
        }
    }

    public async Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var token = await this.context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == request.Token);

            if (token == null || !token.IsValid)
            {
                this.logger.LogWarning("Jeton de réinitialisation invalide ou expiré");
                return ServiceResult<bool>.Failure("Ce lien de réinitialisation est invalide ou a expiré");
            }

            var user = token.User;
            if (!user.IsActive)
            {
                return ServiceResult<bool>.Failure("Ce compte est inactif");
            }

            user.PasswordHash = this.passwordService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Le jeton est à usage unique.
            token.UsedAt = DateTime.UtcNow;

            // Par sécurité, on révoque les sessions ouvertes : si le compte avait été
            // compromis, l'attaquant perd ses jetons de rafraîchissement.
            var activeRefreshTokens = await this.context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await this.context.SaveChangesAsync();

            this.logger.LogInformation("Mot de passe réinitialisé pour l'utilisateur {UserId}", user.Id);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Erreur lors de la réinitialisation du mot de passe");
            return ServiceResult<bool>.Failure("Une erreur est survenue lors de la réinitialisation");
        }
    }

    /// <summary>Durée de validité d'un lien de confirmation d'email, en heures.</summary>
    private const int EmailConfirmationValidityHours = 24;

    /// <summary>Délai minimal (secondes) entre deux envois d'email de confirmation.</summary>
    private const int EmailConfirmationResendCooldownSeconds = 120;

    public async Task<ServiceResult<bool>> ConfirmEmailAsync(string token)
    {
        try
        {
            var confirmation = await this.context.EmailConfirmationTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (confirmation == null || !confirmation.IsValid)
            {
                this.logger.LogWarning("Email confirmation token invalid or expired");
                return ServiceResult<bool>.Failure("Ce lien de confirmation est invalide ou a expiré");
            }

            var user = confirmation.User;

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                user.EmailConfirmedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
            }

            confirmation.UsedAt = DateTime.UtcNow;

            await this.context.SaveChangesAsync();

            this.logger.LogInformation("Email confirmed for user {UserId}", user.Id);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during email confirmation");
            return ServiceResult<bool>.Failure("Une erreur est survenue lors de la confirmation");
        }
    }

    public async Task<ServiceResult<int>> ResendConfirmationEmailAsync(int userId, string confirmUrlTemplate)
    {
        try
        {
            var user = await this.context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.IsActive)
            {
                return ServiceResult<int>.Failure("Compte introuvable");
            }

            if (user.EmailConfirmed)
            {
                return ServiceResult<int>.Failure("Votre adresse est déjà confirmée");
            }

            // Cooldown : on regarde le dernier jeton émis pour cet utilisateur.
            var lastToken = await this.context.EmailConfirmationTokens
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastToken != null)
            {
                var elapsed = DateTime.UtcNow - lastToken.CreatedAt;
                var remaining = EmailConfirmationResendCooldownSeconds - (int)elapsed.TotalSeconds;
                if (remaining > 0)
                {
                    this.logger.LogInformation(
                        "Resend confirmation throttled for user {UserId}, {Remaining}s remaining",
                        userId, remaining);
                    return ServiceResult<int>.Success(remaining);
                }
            }

            await this.CreateAndSendConfirmationAsync(user, confirmUrlTemplate);

            return ServiceResult<int>.Success(0);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error resending confirmation email for user {UserId}", userId);
            return ServiceResult<int>.Failure("Une erreur est survenue lors de l'envoi");
        }
    }

    /// <summary>
    /// Génère un jeton de confirmation, invalide les précédents, et envoie l'email.
    /// </summary>
    private async Task CreateAndSendConfirmationAsync(User user, string confirmUrlTemplate)
    {
        // Un seul lien actif à la fois.
        var pending = await this.context.EmailConfirmationTokens
            .Where(t => t.UserId == user.Id && t.UsedAt == null)
            .ToListAsync();

        foreach (var old in pending)
        {
            old.UsedAt = DateTime.UtcNow;
        }

        var tokenBytes = new byte[48];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }

        var tokenString = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        this.context.EmailConfirmationTokens.Add(new EmailConfirmationToken
        {
            Token = tokenString,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(EmailConfirmationValidityHours),
            CreatedAt = DateTime.UtcNow,
        });

        await this.context.SaveChangesAsync();

        var confirmLink = confirmUrlTemplate.Replace("{token}", Uri.EscapeDataString(tokenString));

        if (this.emailService != null)
        {
            await this.emailService.SendEmailConfirmationAsync(
                user.Email, user.Nickname, confirmLink, EmailConfirmationValidityHours);
        }
        else
        {
            this.logger.LogWarning(
                "Aucun service d'email : lien de confirmation non envoyé à {Email}", user.Email);
        }
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(int userId)
    {
        var tokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var tokenString = Convert.ToBase64String(tokenBytes);

        var refreshToken = new RefreshToken
        {
            Token = tokenString,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        this.context.RefreshTokens.Add(refreshToken);
        await this.context.SaveChangesAsync();

        return refreshToken;
    }
}
