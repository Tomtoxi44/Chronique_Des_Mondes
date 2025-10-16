using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.State;
using Cdm.Web.Shared.DTOs.Auth;
using Microsoft.Extensions.Logging;

namespace Cdm.Web.Components.Pages.Auth;

/// <summary>
/// Handler for login business logic.
/// </summary>
public class LoginHandler
{
    private readonly IAuthApiClient _authApiClient;
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly ILogger<LoginHandler> _logger;
    
    public LoginHandler(
        IAuthApiClient authApiClient,
        CustomAuthStateProvider authStateProvider,
        ILogger<LoginHandler> logger)
    {
        _authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
        _authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        
        _logger.LogInformation("Attempting to login user: {Email}", request.Email);
        
        var response = await _authApiClient.LoginAsync(request);
        
        if (response != null)
        {
            _logger.LogInformation("Login successful for: {Email}", response.Email);
            
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
