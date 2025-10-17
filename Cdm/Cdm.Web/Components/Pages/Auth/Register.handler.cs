using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.State;
using Cdm.Web.Shared.DTOs.Models;
using Cdm.Web.Shared.DTOs.ViewModels;
using Microsoft.Extensions.Logging;

namespace Cdm.Web.Components.Pages.Auth;

/// <summary>
/// Handler for registration business logic.
/// </summary>
public class RegisterHandler
{
    private readonly IAuthApiClient authApiClient;
    private readonly CustomAuthStateProvider authStateProvider;
    private readonly ILogger<RegisterHandler> logger;
    
    public RegisterHandler(
        IAuthApiClient authApiClient,
        CustomAuthStateProvider authStateProvider,
        ILogger<RegisterHandler> logger)
    {
        this.authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
        this.authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Handles user registration.
    /// </summary>
    /// <param name="request">The registration request containing email, password, and confirmation.</param>
    /// <returns>A RegisterResponse if successful, null otherwise.</returns>
    /// <exception cref="ApiException">Thrown when API call fails.</exception>
    public async Task<RegisterResponse?> HandleRegisterAsync(RegisterRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        this.logger.LogInformation("Attempting to register user: {Email}", request.Email);
        
        var response = await this.authApiClient.RegisterAsync(request);
        
        if (response != null)
        {
            this.logger.LogInformation("Registration successful for: {Email}", response.Email);
            
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

