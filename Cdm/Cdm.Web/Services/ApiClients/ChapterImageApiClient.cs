using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

/// <summary>
/// HTTP client for a chapter's image gallery (maps/place visuals), GM-only.
/// </summary>
public class ChapterImageApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChapterImageApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public ChapterImageApiClient(HttpClient httpClient, ILogger<ChapterImageApiClient> logger, ILocalStorageService localStorage)
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

    public async Task<List<ChapterImageDto>> GetForChapterAsync(int chapterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/chapters/{chapterId}/images");
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<ChapterImageDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching images for chapter {ChapterId}", chapterId);
            return new();
        }
    }

    public async Task<ChapterImageDto?> AddAsync(int chapterId, AddChapterImageDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/chapters/{chapterId}/images", dto);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ChapterImageDto>() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding image to chapter {ChapterId}", chapterId);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int imageId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/chapter-images/{imageId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chapter image {ImageId}", imageId);
            return false;
        }
    }
}
