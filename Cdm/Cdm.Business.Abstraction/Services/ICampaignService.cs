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
    /// Updates an existing campaign.
    /// Only the creator can update their campaign.
    /// </summary>
    /// <param name="campaignId">The campaign ID to update.</param>
    /// <param name="dto">The update data.</param>
    /// <param name="userId">The user ID making the request (for authorization).</param>
    /// <returns>The updated campaign DTO, or null if not authorized.</returns>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Thrown when MaxPlayers is less than current player count.</exception>
    Task<CampaignDto?> UpdateCampaignAsync(int campaignId, UpdateCampaignDto dto, int userId);
}
