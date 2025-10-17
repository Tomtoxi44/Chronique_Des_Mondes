namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;

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
            .WithTags("Authentication");

        // POST /api/auth/register
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] IAuthService authService,
            ILogger<RegisterRequest> logger) =>
        {
            logger.LogInformation("Registration request received for email: {Email}", request.Email);

            var result = await authService.RegisterAsync(request);

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
        .WithName("Login")
        .WithSummary("Authenticate a user")
        .WithDescription("Authenticate with email and password to receive a JWT token")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }
}