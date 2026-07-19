namespace Cdm.ApiService.Endpoints;

using Cdm.ApiService.Filters;
using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Authentication endpoints for the API
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication endpoints to the application
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .RequireRateLimiting("auth"); // audit fix #3

        // POST /api/auth/register
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] IAuthService authService,
            [FromServices] IConfiguration configuration,
            ILogger<RegisterRequest> logger) =>
        {
            logger.LogInformation("Registration request received for email: {Email}", request.Email);

            var confirmUrlTemplate = BuildConfirmUrlTemplate(configuration);
            var result = await authService.RegisterAsync(request, confirmUrlTemplate);

            if (result.IsSuccess)
            {
                logger.LogInformation("Registration successful for user: {UserId}", result.Data!.UserId);
                return Results.Created($"/api/users/{result.Data.UserId}", result.Data);
            }

            if (result.ValidationErrors != null)
            {
                logger.LogWarning("Registration validation failed for email: {Email}", request.Email);
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "Validation failed",
                    ValidationErrors = result.ValidationErrors
                });
            }

            logger.LogWarning("Registration failed for email: {Email} - {Error}", request.Email, result.ErrorMessage);
            return Results.BadRequest(new ErrorResponse
            {
                Error = result.ErrorMessage ?? "Registration failed"
            });
        })
        .AddEndpointFilter<ValidationFilter<RegisterRequest>>() // audit fix #5
        .WithName("Register")
        .WithSummary("Register a new user")
        .WithDescription("Creates a new user account with email and password")
        .Produces<RegisterResponse>(StatusCodes.Status201Created)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        // POST /api/auth/login
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] IAuthService authService,
            ILogger<LoginRequest> logger) =>
        {
            logger.LogInformation("Login request received for email: {Email}", request.Email);

            var result = await authService.LoginAsync(request);

            if (result.IsSuccess)
            {
                logger.LogInformation("Login successful for user: {UserId}", result.Data!.UserId);
                return Results.Ok(result.Data);
            }

            logger.LogWarning("Login failed for email: {Email} - {Error}", request.Email, result.ErrorMessage);
            return Results.BadRequest(new ErrorResponse
            {
                Error = result.ErrorMessage ?? "Login failed"
            });
        })
        .AddEndpointFilter<ValidationFilter<LoginRequest>>() // audit fix #5
        .WithName("Login")
        .WithSummary("Authenticate a user")
        .WithDescription("Authenticate with email and password to receive a JWT token")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        // POST /api/auth/refresh
        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            [FromServices] IAuthService authService,
            ILogger<RefreshTokenRequest> logger) =>
        {
            logger.LogInformation("Token refresh request received");

            var result = await authService.RefreshTokenAsync(request.RefreshToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Token refreshed for user: {UserId}", result.Data!.UserId);
                return Results.Ok(result.Data);
            }

            logger.LogWarning("Token refresh failed: {Error}", result.ErrorMessage);
            return Results.Unauthorized();
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh a JWT access token")
        .WithDescription("Exchange a valid refresh token for a new access token and refresh token")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // POST /api/auth/forgot-password
        group.MapPost("/forgot-password", async (
            [FromBody] ForgotPasswordRequest request,
            [FromServices] IAuthService authService,
            [FromServices] IConfiguration configuration,
            ILogger<ForgotPasswordRequest> logger) =>
        {
            logger.LogInformation("Password reset requested for {Email}", request.Email);

            var webBaseUrl = configuration["App:WebBaseUrl"]?.TrimEnd('/')
                ?? "https://localhost:7165";
            var resetUrlTemplate = $"{webBaseUrl}/reset-password?token={{token}}";

            var result = await authService.RequestPasswordResetAsync(request, resetUrlTemplate);

            if (!result.IsSuccess)
            {
                return Results.Problem(
                    title: result.ErrorMessage ?? "Password reset request failed",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            // Réponse identique que le compte existe ou non (anti-énumération).
            return Results.Ok(new
            {
                Message = "Si un compte existe pour cette adresse, un email de réinitialisation vient d'être envoyé.",
            });
        })
        .AddEndpointFilter<ValidationFilter<ForgotPasswordRequest>>()
        .WithName("ForgotPassword")
        .WithSummary("Request a password reset link")
        .WithDescription("Sends a single-use reset link by email. Always returns 200 to avoid account enumeration.")
        .Produces(StatusCodes.Status200OK);

        // POST /api/auth/reset-password
        group.MapPost("/reset-password", async (
            [FromBody] ResetPasswordRequest request,
            [FromServices] IAuthService authService,
            ILogger<ResetPasswordRequest> logger) =>
        {
            var result = await authService.ResetPasswordAsync(request);

            if (result.IsSuccess)
            {
                logger.LogInformation("Password successfully reset");
                return Results.Ok(new { Message = "Votre mot de passe a été réinitialisé." });
            }

            logger.LogWarning("Password reset failed: {Error}", result.ErrorMessage);
            return Results.BadRequest(new ErrorResponse
            {
                Error = result.ErrorMessage ?? "Password reset failed",
            });
        })
        .AddEndpointFilter<ValidationFilter<ResetPasswordRequest>>()
        .WithName("ResetPassword")
        .WithSummary("Set a new password from a reset token")
        .Produces(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        // POST /api/auth/confirm-email
        group.MapPost("/confirm-email", async (
            [FromBody] ConfirmEmailRequest request,
            [FromServices] IAuthService authService,
            ILogger<ConfirmEmailRequest> logger) =>
        {
            var result = await authService.ConfirmEmailAsync(request.Token);

            if (result.IsSuccess)
            {
                logger.LogInformation("Email confirmed successfully");
                return Results.Ok(new { Message = "Votre adresse email a été confirmée." });
            }

            logger.LogWarning("Email confirmation failed: {Error}", result.ErrorMessage);
            return Results.BadRequest(new ErrorResponse
            {
                Error = result.ErrorMessage ?? "Email confirmation failed",
            });
        })
        .AddEndpointFilter<ValidationFilter<ConfirmEmailRequest>>()
        .WithName("ConfirmEmail")
        .WithSummary("Confirm an email address from a confirmation token")
        .Produces(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        // POST /api/auth/resend-confirmation — utilisateur authentifié uniquement
        group.MapPost("/resend-confirmation", async (
            [FromServices] IAuthService authService,
            [FromServices] IConfiguration configuration,
            HttpContext httpContext,
            ILogger<IAuthService> logger) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var confirmUrlTemplate = BuildConfirmUrlTemplate(configuration);
            var result = await authService.ResendConfirmationEmailAsync(userId, confirmUrlTemplate);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new ErrorResponse { Error = result.ErrorMessage ?? "Resend failed" });
            }

            // Data = 0 → envoyé ; Data > 0 → cooldown, secondes restantes.
            return Results.Ok(new { RetryAfterSeconds = result.Data });
        })
        .RequireAuthorization()
        .WithName("ResendConfirmation")
        .WithSummary("Resend the email confirmation link (rate-limited by a cooldown)")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// Construit le gabarit d'URL de la page de confirmation d'email à partir de la
    /// base du front (App:WebBaseUrl), avec le marqueur {token}.
    /// </summary>
    private static string BuildConfirmUrlTemplate(IConfiguration configuration)
    {
        var webBaseUrl = configuration["App:WebBaseUrl"]?.TrimEnd('/') ?? "https://localhost:7165";
        return $"{webBaseUrl}/confirm-email?token={{token}}";
    }
}