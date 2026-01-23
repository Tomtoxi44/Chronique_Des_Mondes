namespace Cdm.Web.Services;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cdm.Common.Enums;
using Cdm.Web.Components.Campaigns;
using Cdm.Web.Services.Storage;

/// <summary>
/// Service for managing campaigns from the client side.
/// </summary>
/// <param name="httpClient">HTTP client for API calls.</param>
/// <param name="localStorage">Local storage service for JWT token.</param>
/// <param name="logger">Logger instance.</param>
public class CampaignService(
    HttpClient httpClient,
    ILocalStorageService localStorage,
    ILogger<CampaignService> logger) : ICampaignService
{
    private readonly HttpClient httpClient = httpClient;
    private readonly ILocalStorageService localStorage = localStorage;
    private readonly ILogger<CampaignService> logger = logger;

    /// <inheritdoc/>
    public async Task<CampaignResponse?> CreateCampaignAsync(CampaignForm.CreateCampaignModel model)
    {
        try
        {
            await this.AddAuthHeaderAsync();

            var request = new CreateCampaignRequest
            {
                Name = model.Name,
                Description = model.Description,
                GameType = model.GameType,
                Visibility = model.Visibility,
                MaxPlayers = model.MaxPlayers,
                CoverImageBase64 = model.CoverImageBase64
            };

            this.logger.LogInformation("Sending campaign creation request to API");

            var response = await this.httpClient.PostAsJsonAsync("api/campaigns", request);

            if (response.IsSuccessStatusCode)
            {
                var campaignResponse = await response.Content.ReadFromJsonAsync<CampaignResponse>();
                this.logger.LogInformation("Campaign created successfully with ID: {Id}", campaignResponse?.Id);
                return campaignResponse;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            this.logger.LogWarning(
                "Campaign creation failed with status {StatusCode}: {Error}",
                response.StatusCode,
                errorContent);

            return null;
        }
        catch (HttpRequestException ex)
        {
            this.logger.LogError(ex, "HTTP error creating campaign");
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error creating campaign");
            return null;
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

    /// <summary>
    /// Request model for creating a campaign.
    /// </summary>
    private class CreateCampaignRequest
    {
        /// <summary>
        /// Gets or sets the campaign name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the campaign description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the game type.
        /// </summary>
        public GameType GameType { get; set; }

        /// <summary>
        /// Gets or sets the visibility level.
        /// </summary>
        public Visibility Visibility { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of players.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Gets or sets the cover image Base64.
        /// </summary>
        public string? CoverImageBase64 { get; set; }
    }
}
