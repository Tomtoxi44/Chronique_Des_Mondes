namespace Cdm.ApiService.Endpoints.Models;

/// <summary>
/// Request model for joining a world with an invite token
/// </summary>
public class JoinWorldRequest
{
    /// <summary>
    /// Gets or sets the invite token
    /// </summary>
    public string InviteToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character ID to bring into the world
    /// </summary>
    public int CharacterId { get; set; }
}
