using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.State;
using Cdm.Web.Shared.DTOs.Models;
using Cdm.Web.Shared.DTOs.ViewModels;
using Microsoft.Extensions.Logging;

namespace Cdm.Web.Components.Pages.Auth;

/// <summary>
/// Handler for login business logic.
/// </summary>
public class LoginHandler
{
    private readonly IAuthApiClient authApiClient;
    private readonly CustomAuthStateProvider authStateProvider;
    private readonly ILogger<LoginHandler> logger;
    
    public LoginHandler(
        IAuthApiClient authApiClient,
        CustomAuthStateProvider authStateProvider,
        ILogger<LoginHandler> logger)
    {
        this.authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
        this.authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Handles user login authentication.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>A LoginResponse if successful, null otherwise.</returns>
    /// <exception cref="ApiException">Thrown when API call fails.</exception>
    public async Task<LoginResponse?> HandleLoginAsync(LoginRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        this.logger.LogInformation("Attempting to login user: {Email}", request.Email);
        
        var response = await this.authApiClient.LoginAsync(request);
        
        if (response != null)
        {
            this.logger.LogInformation("Login successful for: {Email}", response.Email);
            
            await this.authStateProvider.MarkUserAsAuthenticatedAsync(
                response.UserId,
                response.Email,
                response.Token);
            
            // Small delay to ensure localStorage is synchronized
            await Task.Delay(100);
        }
        
        return response;
    }
}

