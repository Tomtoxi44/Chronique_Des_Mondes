namespace Cdm.ApiService.Endpoints.Models;

/// <summary>
/// Request model for joining a campaign with an invite token
/// </summary>
public class JoinCampaignRequest
{
    /// <summary>
    /// Gets or sets the invite token
    /// </summary>
    public string InviteToken { get; set; } = string.Empty;
}
