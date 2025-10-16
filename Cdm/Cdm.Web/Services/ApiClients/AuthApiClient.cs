using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.Storage;
using Cdm.Web.Shared.DTOs.Auth;

namespace Cdm.Web.Services.ApiClients;

public interface IAuthApiClient
{
    Task<RegisterResponse?> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

public class AuthApiClient : BaseApiClient, IAuthApiClient
{
    public AuthApiClient(
        HttpClient httpClient,
        ILogger<AuthApiClient> logger,
        ILocalStorageService localStorage)
        : base(httpClient, logger, localStorage)
    {
    }
    
    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Attempting to register user: {Email}", request.Email);
        
        var response = await PostAsync<RegisterRequest, RegisterResponse>(
            "/api/auth/register", 
            request);
        
        if (response != null)
        {
            _logger.LogInformation("User registered successfully: {Email}", response.Email);
        }
        
        return response;
    }
    
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Attempting to login user: {Email}", request.Email);
        
        var response = await PostAsync<LoginRequest, LoginResponse>(
            "/api/auth/login", 
            request);
        
        if (response != null)
        {
            _logger.LogInformation("User logged in successfully: {Email}", response.Email);
        }
        
        return response;
    }
}
