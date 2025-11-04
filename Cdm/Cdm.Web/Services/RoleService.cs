namespace Cdm.Web.Services;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Cdm.Web.Services.Storage;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation of role service for client-side operations
/// </summary>
public class RoleService : IRoleService
{
    private readonly HttpClient httpClient;
    private readonly ILocalStorageService localStorage;
    private readonly ILogger<RoleService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleService"/> class
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="localStorage">Local storage service for JWT token</param>
    /// <param name="logger">Logger instance</param>
    public RoleService(HttpClient httpClient, ILocalStorageService localStorage, ILogger<RoleService> logger)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all roles assigned to the current authenticated user
    /// </summary>
    /// <returns>List of user roles with details</returns>
    public async Task<List<UserRoleInfo>?> GetMyRolesAsync()
    {
        try
        {
            this.logger.LogInformation("Fetching user roles from API");
            await this.AddAuthHeaderAsync();

            var response = await this.httpClient.GetFromJsonAsync<GetMyRolesResponse>("/api/users/my-roles");

            if (response?.Roles != null)
            {
                this.logger.LogInformation("Successfully fetched {Count} roles", response.Roles.Count);
                return response.Roles.Select(r => new UserRoleInfo
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    AssignedAt = r.AssignedAt
                }).ToList();
            }

            return new List<UserRoleInfo>();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching user roles");
            return null;
        }
    }

    /// <summary>
    /// Requests the GameMaster role for the current user
    /// </summary>
    /// <returns>True if successful, false if already has the role or error occurred</returns>
    public async Task<(bool Success, string Message)> RequestGameMasterRoleAsync()
    {
        try
        {
            this.logger.LogInformation("Requesting GameMaster role from API");
            await this.AddAuthHeaderAsync();

            var response = await this.httpClient.PostAsJsonAsync("/api/users/request-gamemaster-role", new { });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RequestGameMasterRoleResponse>();
                this.logger.LogInformation("GameMaster role request: {Success}", result?.Success);
                return (result?.Success ?? false, result?.Message ?? "Unknown response");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var result = await response.Content.ReadFromJsonAsync<RequestGameMasterRoleResponse>();
                this.logger.LogWarning("GameMaster role already assigned");
                return (false, result?.Message ?? "Role already assigned");
            }
            else
            {
                this.logger.LogError("Failed to request GameMaster role: {StatusCode}", response.StatusCode);
                return (false, "Failed to request role");
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error requesting GameMaster role");
            return (false, "An error occurred");
        }
    }

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    /// <param name="roleName">Role name to check (Player, GameMaster, Admin)</param>
    /// <returns>True if user has the role</returns>
    public async Task<bool> HasRoleAsync(string roleName)
    {
        try
        {
            var roles = await this.GetMyRolesAsync();
            return roles?.Any(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)) ?? false;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error checking if user has role {RoleName}", roleName);
            return false;
        }
    }

    /// <summary>
    /// Adds JWT authorization header to HTTP requests.
    /// </summary>
    private async Task AddAuthHeaderAsync()
    {
        var token = await this.localStorage.GetItemAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // Response DTOs matching API

    private class GetMyRolesResponse
    {
        public int UserId { get; set; }
        public List<RoleInfo> Roles { get; set; } = new();
    }

    private class RoleInfo
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }

    private class RequestGameMasterRoleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
