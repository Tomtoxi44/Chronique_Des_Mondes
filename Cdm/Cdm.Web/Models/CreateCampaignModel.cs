using Cdm.Common.Enums;

namespace Cdm.Web.Models;

/// <summary>
/// Form model for creating a new campaign.
/// </summary>
public class CreateCampaignModel
{
    /// <summary>Campaign name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Campaign description.</summary>
    public string? Description { get; set; }

    /// <summary>Game type for the campaign.</summary>
    public GameType GameType { get; set; } = GameType.Generic;

    /// <summary>Visibility level.</summary>
    public Visibility Visibility { get; set; } = Visibility.Public;

    /// <summary>Maximum number of players.</summary>
    public int MaxPlayers { get; set; } = 6;

    /// <summary>Cover image as Base64 string.</summary>
    public string? CoverImageBase64 { get; set; }
}
