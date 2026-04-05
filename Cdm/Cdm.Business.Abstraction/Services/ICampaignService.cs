namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing campaigns
/// </summary>
public interface ICampaignService
{
    /// <summary>
    /// Creates a new campaign
    /// </summary>
    /// <param name="request">The campaign creation request</param>
    /// <param name="userId">The user identifier of the campaign creator</param>
    /// <returns>The created campaign</returns>
    Task<CampaignDto?> CreateCampaignAsync(CreateCampaignDto request, int userId);

    /// <summary>
    /// Gets all campaigns created by a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <returns>A list of campaigns created by the user</returns>
    Task<IEnumerable<CampaignDto>> GetMyCampaignsAsync(int userId);

    /// <summary>
    /// Gets a campaign by its identifier
    /// </summary>
    /// <param name="campaignId">The campaign identifier</param>
    /// <param name="userId">The user identifier requesting the campaign</param>
    /// <returns>The campaign if found and authorized, null otherwise</returns>
    Task<CampaignDto?> GetCampaignByIdAsync(int campaignId, int userId);

    /// <summary>
    /// Updates an existing campaign
    /// </summary>
    /// <param name="campaignId">The campaign identifier</param>
    /// <param name="dto">The campaign update data</param>
    /// <param name="userId">The user identifier</param>
    /// <returns>The updated campaign if successful, null otherwise</returns>
    Task<CampaignDto?> UpdateCampaignAsync(int campaignId, CreateCampaignDto dto, int userId);

    /// <summary>
    /// Deletes a campaign (soft delete)
    /// </summary>
    /// <param name="campaignId">The campaign identifier</param>
    /// <param name="userId">The user identifier</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteCampaignAsync(int campaignId, int userId);
}
