namespace Cdm.Common.Services;

using System.Threading.Tasks;

/// <summary>
/// Service for storing and managing images
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Uploads a campaign cover image
    /// </summary>
    /// <param name="base64Image">The image encoded in Base64</param>
    /// <param name="campaignId">The campaign identifier</param>
    /// <returns>The URL of the uploaded image, or null if upload failed</returns>
    Task<string?> UploadCampaignCoverAsync(string base64Image, int campaignId);

    /// <summary>
    /// Deletes a campaign cover image
    /// </summary>
    /// <param name="imageUrl">The image URL to delete</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteCampaignCoverAsync(string imageUrl);
}
