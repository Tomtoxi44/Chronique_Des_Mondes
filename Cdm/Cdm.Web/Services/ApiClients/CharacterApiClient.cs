using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.Storage;
using Cdm.Web.Shared.DTOs.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cdm.Web.Services.ApiClients;

/// <summary>
/// API client interface for character operations
/// </summary>
public interface ICharacterApiClient
{
    /// <summary>
    /// Gets all characters for the current user
    /// </summary>
    Task<IEnumerable<CharacterDto>?> GetMyCharactersAsync();

    /// <summary>Gets the game profile for a character in a campaign.</summary>
    Task<JsonElement?> GetGameProfileAsync(int characterId, int campaignId);

    /// <summary>Creates the game profile for a character in a campaign.</summary>
    Task<bool> CreateGameProfileAsync(int characterId, int campaignId, object profileData);

    /// <summary>Updates the game profile for a character in a campaign.</summary>
    Task<bool> UpdateGameProfileAsync(int characterId, int campaignId, object profileData);

    /// <summary>
    /// Gets a specific character by ID
    /// </summary>
    Task<CharacterDto?> GetCharacterByIdAsync(int characterId);

    /// <summary>
    /// Creates a new character
    /// </summary>
    Task<CharacterDto?> CreateCharacterAsync(CreateCharacterDto request);

    /// <summary>
    /// Updates an existing character
    /// </summary>
    Task<CharacterDto?> UpdateCharacterAsync(int characterId, UpdateCharacterDto request);

    /// <summary>
    /// Deletes a character
    /// </summary>
    Task<bool> DeleteCharacterAsync(int characterId);
}

/// <summary>
/// API client for character operations
/// </summary>
public class CharacterApiClient : BaseApiClient, ICharacterApiClient
{
    public CharacterApiClient(
        HttpClient httpClient,
        ILogger<CharacterApiClient> logger,
        ILocalStorageService localStorage)
        : base(httpClient, logger, localStorage)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CharacterDto>?> GetMyCharactersAsync()
    {
        this.logger.LogInformation("Fetching user's characters");

        var response = await GetAsync<IEnumerable<CharacterDto>>("/api/characters");

        if (response != null)
        {
            this.logger.LogInformation("Successfully fetched {Count} characters", response.Count());
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<CharacterDto?> GetCharacterByIdAsync(int characterId)
    {
        this.logger.LogInformation("Fetching character {CharacterId}", characterId);

        var response = await GetAsync<CharacterDto>($"/api/characters/{characterId}");

        if (response != null)
        {
            this.logger.LogInformation("Successfully fetched character {CharacterId}", characterId);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<CharacterDto?> CreateCharacterAsync(CreateCharacterDto request)
    {
        this.logger.LogInformation("Creating character: {Name}", request.Name);

        var response = await PostAsync<CreateCharacterDto, CharacterDto>(
            "/api/characters",
            request);

        if (response != null)
        {
            this.logger.LogInformation("Character created successfully: {CharacterId}", response.Id);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<CharacterDto?> UpdateCharacterAsync(int characterId, UpdateCharacterDto request)
    {
        this.logger.LogInformation("Updating character {CharacterId}", characterId);

        var response = await PutAsync<UpdateCharacterDto, CharacterDto>(
            $"/api/characters/{characterId}",
            request);

        if (response != null)
        {
            this.logger.LogInformation("Character updated successfully: {CharacterId}", characterId);
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteCharacterAsync(int characterId)
    {
        this.logger.LogInformation("Deleting character {CharacterId}", characterId);

        var success = await DeleteAsync($"/api/characters/{characterId}");

        if (success)
        {
            this.logger.LogInformation("Character deleted successfully: {CharacterId}", characterId);
        }

        return success;
    }

    /// <inheritdoc/>
    public async Task<JsonElement?> GetGameProfileAsync(int characterId, int campaignId)
    {
        this.logger.LogInformation("Fetching game profile for character {CharacterId} in campaign {CampaignId}", characterId, campaignId);
        try
        {
            await AddAuthorizationHeaderAsync();
            var response = await this.httpClient.GetAsync($"/api/characters/{characterId}/game-profile?campaignId={campaignId}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching game profile for character {CharacterId}", characterId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CreateGameProfileAsync(int characterId, int campaignId, object profileData)
    {
        this.logger.LogInformation("Creating game profile for character {CharacterId} in campaign {CampaignId}", characterId, campaignId);
        try
        {
            await AddAuthorizationHeaderAsync();
            var response = await this.httpClient.PostAsJsonAsync(
                $"/api/characters/{characterId}/game-profile?campaignId={campaignId}", profileData);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating game profile for character {CharacterId}", characterId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateGameProfileAsync(int characterId, int campaignId, object profileData)
    {
        this.logger.LogInformation("Updating game profile for character {CharacterId} in campaign {CampaignId}", characterId, campaignId);
        try
        {
            await AddAuthorizationHeaderAsync();
            var response = await this.httpClient.PutAsJsonAsync(
                $"/api/characters/{characterId}/game-profile?campaignId={campaignId}", profileData);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating game profile for character {CharacterId}", characterId);
            return false;
        }
    }
}
