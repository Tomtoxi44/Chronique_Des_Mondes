using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

/// <summary>
/// HTTP client for the generic image upload endpoint.
/// </summary>
public class ImageApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public ImageApiClient(HttpClient httpClient, ILogger<ImageApiClient> logger, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _logger = logger;
        _localStorage = localStorage;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Uploads an image to the given category and returns its public URL (or an error).
    /// </summary>
    public async Task<(string? Url, string? Error)> UploadAsync(Stream content, string fileName, string contentType, string category)
    {
        try
        {
            await AddAuthHeaderAsync();

            using var form = new MultipartFormDataContent();
            var fileContent = new StreamContent(content);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType);
            form.Add(fileContent, "file", string.IsNullOrEmpty(fileName) ? "image" : fileName);

            var response = await _httpClient.PostAsync($"api/images/{category}", form);
            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<UploadResponse>();
                return (payload?.Url, null);
            }

            var problem = await response.Content.ReadFromJsonAsync<UploadResponse>();
            return (null, problem?.Error ?? "Échec de l'envoi de l'image.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image (category {Category})", category);
            return (null, "Échec de l'envoi de l'image.");
        }
    }

    private sealed class UploadResponse
    {
        public string? Url { get; set; }

        public string? Error { get; set; }
    }
}
