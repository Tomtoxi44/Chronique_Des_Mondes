// <copyright file="ProfileApiClient.cs" company="Chronique Des Mondes">
// Copyright (c) Chronique Des Mondes. All rights reserved.
// </copyright>

namespace Cdm.Web.Services;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
using Microsoft.AspNetCore.Components.Forms;

/// <summary>
/// API client for user profile operations
/// </summary>
public class ProfileApiClient
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileApiClient"/> class
    /// </summary>
    /// <param name="httpClient">HTTP client instance</param>
    public ProfileApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <summary>
    /// Gets the current user's profile
    /// </summary>
    /// <returns>Profile response or null if not found</returns>
    public async Task<ProfileResponse?> GetProfileAsync()
    {
        try
        {
            return await this.httpClient.GetFromJsonAsync<ProfileResponse>("api/users/profile");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    /// <summary>
    /// Updates the current user's profile
    /// </summary>
    /// <param name="request">Update profile request</param>
    /// <returns>Updated profile response or null if failed</returns>
    public async Task<ProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var response = await this.httpClient.PutAsJsonAsync("api/users/profile", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProfileResponse>();
            }

            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    /// <summary>
    /// Uploads an avatar for the current user
    /// </summary>
    /// <param name="file">Avatar file</param>
    /// <returns>Avatar URL or null if failed</returns>
    public async Task<string?> UploadAvatarAsync(IBrowserFile file)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 2 * 1024 * 1024));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "avatar", file.Name);

            var response = await this.httpClient.PostAsync("api/users/avatar", content);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                return result?["avatarUrl"];
            }

            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
