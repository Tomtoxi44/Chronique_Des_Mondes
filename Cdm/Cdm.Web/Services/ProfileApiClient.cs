namespace Cdm.Web.Services;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
using Cdm.Web.Services.Storage;
using Microsoft.AspNetCore.Components.Forms;

/// <summary>
/// API client for user profile operations.
/// </summary>
public class ProfileApiClient
{
    /// <summary>Taille maximale d'un avatar — alignée sur ImageValidation.MaxFileSizeBytes côté API.</summary>
    private const long MaxAvatarBytes = 5 * 1024 * 1024;

    private readonly HttpClient httpClient;
    private readonly ILocalStorageService localStorage;
    private readonly ILogger<ProfileApiClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client instance.</param>
    /// <param name="localStorage">Local storage service for JWT token.</param>
    /// <param name="logger">Logger instance.</param>
    public ProfileApiClient(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        ILogger<ProfileApiClient> logger)
    {
        this.httpClient = httpClient;
        this.localStorage = localStorage;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <returns>Profile response or null if not found.</returns>
    public async Task<ProfileResponse?> GetProfileAsync()
    {
        try
        {
            await this.AddAuthHeaderAsync();
            return await this.httpClient.GetFromJsonAsync<ProfileResponse>("api/users/profile");
        }
        catch (HttpRequestException ex)
        {
            this.logger.LogError(ex, "Error fetching profile");
            return null;
        }
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="request">Update profile request.</param>
    /// <returns>Updated profile response or null if failed.</returns>
    public async Task<ProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            await this.AddAuthHeaderAsync();
            var response = await this.httpClient.PutAsJsonAsync("api/users/profile", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProfileResponse>();
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            this.logger.LogError(ex, "Error updating profile");
            return null;
        }
    }

    /// <summary>
    /// Uploads an avatar for the current user.
    /// </summary>
    /// <param name="file">Avatar file.</param>
    /// <returns>Avatar URL or null if failed.</returns>
    public async Task<string?> UploadAvatarAsync(IBrowserFile file)
    {
        try
        {
            // On lit d'abord le fichier en mémoire (même approche que AppImageUpload). Passer
            // directement le flux du navigateur à StreamContent ferait tenir la requête HTTP
            // ouverte pendant tout le transfert par le circuit SignalR : côté API on a mesuré
            // 14 s pour 2 Mo, et le moindre hoquet du circuit avortait l'appel — l'image
            // finissait dans le blob mais l'utilisateur voyait une erreur.
            using var buffer = new MemoryStream();
            await using (var stream = file.OpenReadStream(MaxAvatarBytes))
            {
                await stream.CopyToAsync(buffer);
            }

            await this.AddAuthHeaderAsync();
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(buffer.ToArray());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "avatar", file.Name);

            var response = await this.httpClient.PostAsync("api/users/profile/avatar", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                return result?["avatarUrl"];
            }

            this.logger.LogWarning("Avatar upload rejected by the API ({Status})", response.StatusCode);
            return null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
        {
            // IOException : lecture du fichier interrompue / trop volumineux.
            // TaskCanceledException : délai HttpClient dépassé.
            this.logger.LogError(ex, "Error uploading avatar");
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
}
