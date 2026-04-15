// -----------------------------------------------------------------------
// <copyright file="ChapterApiClient.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Services.ApiClients;

using Cdm.Business.Abstraction.DTOs;
using System.Net.Http.Json;

/// <summary>
/// API client for chapter operations.
/// </summary>
public class ChapterApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<ChapterApiClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="logger">The logger.</param>
    public ChapterApiClient(HttpClient httpClient, ILogger<ChapterApiClient> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all chapters for a specific campaign.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <returns>List of chapters.</returns>
    public async Task<List<ChapterDto>> GetChaptersByCampaignAsync(int campaignId)
    {
        try
        {
            this.logger.LogInformation("Fetching chapters for campaign {CampaignId}", campaignId);
            var response = await this.httpClient.GetAsync($"/api/chapters/campaign/{campaignId}");
            
            if (response.IsSuccessStatusCode)
            {
                var chapters = await response.Content.ReadFromJsonAsync<List<ChapterDto>>();
                return chapters ?? new List<ChapterDto>();
            }

            this.logger.LogWarning("Failed to fetch chapters for campaign {CampaignId}. Status: {StatusCode}", 
                campaignId, response.StatusCode);
            return new List<ChapterDto>();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching chapters for campaign {CampaignId}", campaignId);
            return new List<ChapterDto>();
        }
    }

    /// <summary>
    /// Gets a specific chapter by ID.
    /// </summary>
    /// <param name="id">The chapter ID.</param>
    /// <returns>The chapter DTO or null if not found.</returns>
    public async Task<ChapterDto?> GetChapterByIdAsync(int id)
    {
        try
        {
            this.logger.LogInformation("Fetching chapter {ChapterId}", id);
            var response = await this.httpClient.GetAsync($"/api/chapters/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChapterDto>();
            }

            this.logger.LogWarning("Chapter {ChapterId} not found. Status: {StatusCode}", id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching chapter {ChapterId}", id);
            return null;
        }
    }

    /// <summary>
    /// Creates a new chapter.
    /// </summary>
    /// <param name="createDto">The chapter creation data.</param>
    /// <returns>The created chapter DTO.</returns>
    public async Task<ChapterDto?> CreateChapterAsync(CreateChapterDto createDto)
    {
        try
        {
            this.logger.LogInformation("Creating chapter '{Title}' for campaign {CampaignId}", 
                createDto.Title, createDto.CampaignId);
            
            var response = await this.httpClient.PostAsJsonAsync("/api/chapters", createDto);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChapterDto>();
            }

            this.logger.LogWarning("Failed to create chapter. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating chapter '{Title}'", createDto.Title);
            return null;
        }
    }

    /// <summary>
    /// Updates an existing chapter.
    /// </summary>
    /// <param name="id">The chapter ID.</param>
    /// <param name="updateDto">The updated chapter data.</param>
    /// <returns>The updated chapter DTO.</returns>
    public async Task<ChapterDto?> UpdateChapterAsync(int id, CreateChapterDto updateDto)
    {
        try
        {
            this.logger.LogInformation("Updating chapter {ChapterId}", id);
            
            var response = await this.httpClient.PutAsJsonAsync($"/api/chapters/{id}", updateDto);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChapterDto>();
            }

            this.logger.LogWarning("Failed to update chapter {ChapterId}. Status: {StatusCode}", 
                id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating chapter {ChapterId}", id);
            return null;
        }
    }

    /// <summary>
    /// Deletes a chapter.
    /// </summary>
    /// <param name="id">The chapter ID.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public async Task<bool> DeleteChapterAsync(int id)
    {
        try
        {
            this.logger.LogInformation("Deleting chapter {ChapterId}", id);
            
            var response = await this.httpClient.DeleteAsync($"/api/chapters/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            this.logger.LogWarning("Failed to delete chapter {ChapterId}. Status: {StatusCode}", 
                id, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting chapter {ChapterId}", id);
            return false;
        }
    }

    /// <summary>
    /// Starts a chapter (marks it as active).
    /// </summary>
    /// <param name="id">The chapter ID.</param>
    /// <returns>The updated chapter DTO.</returns>
    public async Task<ChapterDto?> StartChapterAsync(int id)
    {
        try
        {
            this.logger.LogInformation("Starting chapter {ChapterId}", id);
            
            var response = await this.httpClient.PostAsync($"/api/chapters/{id}/start", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChapterDto>();
            }

            this.logger.LogWarning("Failed to start chapter {ChapterId}. Status: {StatusCode}", 
                id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error starting chapter {ChapterId}", id);
            return null;
        }
    }

    /// <summary>
    /// Completes a chapter (marks it as finished).
    /// </summary>
    /// <param name="id">The chapter ID.</param>
    /// <returns>The updated chapter DTO.</returns>
    public async Task<ChapterDto?> CompleteChapterAsync(int id)
    {
        try
        {
            this.logger.LogInformation("Completing chapter {ChapterId}", id);
            
            var response = await this.httpClient.PostAsync($"/api/chapters/{id}/complete", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChapterDto>();
            }

            this.logger.LogWarning("Failed to complete chapter {ChapterId}. Status: {StatusCode}", 
                id, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error completing chapter {ChapterId}", id);
            return null;
        }
    }
}
