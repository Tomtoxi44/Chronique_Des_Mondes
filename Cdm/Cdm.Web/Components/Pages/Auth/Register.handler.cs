using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.State;
using Cdm.Web.Shared.DTOs.Auth;
using Microsoft.Extensions.Logging;

namespace Cdm.Web.Components.Pages.Auth;

/// <summary>
/// Handler for registration business logic.
/// </summary>
public class RegisterHandler
{
    private readonly IAuthApiClient _authApiClient;
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly ILogger<RegisterHandler> _logger;
    
    public RegisterHandler(
        IAuthApiClient authApiClient,
        CustomAuthStateProvider authStateProvider,
        ILogger<RegisterHandler> logger)
    {
        _authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
        _authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        
        _logger.LogInformation("Attempting to register user: {Email}", request.Email);
        
        var response = await _authApiClient.RegisterAsync(request);
        
        if (response != null)
        {
            _logger.LogInformation("Registration successful for: {Email}", response.Email);
            
            await _authStateProvider.MarkUserAsAuthenticatedAsync(
                response.UserId,
                response.Email,
                response.Token);
            
            // Small delay to ensure localStorage is synchronized
            await Task.Delay(100);
        }
        
        return response;
    }
}
