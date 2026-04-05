namespace Cdm.ApiService.Endpoints.Models;

using Cdm.Common.Enums;

/// <summary>
/// Request model for updating campaign status
/// </summary>
public class UpdateCampaignStatusRequest
{
    /// <summary>
    /// Gets or sets the new campaign status
    /// </summary>
    public CampaignStatus Status { get; set; }
}
