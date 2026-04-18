namespace Cdm.Web.Models;

/// <summary>
/// View model aggregating summary data for the user dashboard.
/// </summary>
public class DashboardModel
{
    /// <summary>Total number of worlds owned by the user.</summary>
    public int WorldCount { get; set; }

    /// <summary>Total number of characters owned by the user.</summary>
    public int CharacterCount { get; set; }

    /// <summary>Total number of campaigns the user participates in.</summary>
    public int CampaignCount { get; set; }

    /// <summary>Most recently created or updated worlds.</summary>
    public List<WorldModel> RecentWorlds { get; set; } = [];

    /// <summary>Most recently created or updated characters.</summary>
    public List<CharacterModel> RecentCharacters { get; set; } = [];

    /// <summary>Whether the user has any worlds.</summary>
    public bool HasWorlds => WorldCount > 0;

    /// <summary>Whether the user has any characters.</summary>
    public bool HasCharacters => CharacterCount > 0;

    /// <summary>Whether the user has any campaigns.</summary>
    public bool HasCampaigns => CampaignCount > 0;

    /// <summary>Whether the dashboard has any data at all.</summary>
    public bool HasData => HasWorlds || HasCharacters || HasCampaigns;
}
