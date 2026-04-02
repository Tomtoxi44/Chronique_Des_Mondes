namespace Cdm.Business.Abstraction.DTOs;

using System.ComponentModel.DataAnnotations;
using Cdm.Common.Enums;

/// <summary>
/// Data Transfer Object for creating a new Campaign
/// </summary>
public class CreateCampaignDto
{
    /// <summary>
    /// Gets or sets the campaign name (3-100 characters)
    /// </summary>
    [Required(ErrorMessage = "Campaign name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Campaign name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the campaign description (max 5000 characters)
    /// </summary>
    [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the world ID this campaign belongs to
    /// </summary>
    [Required(ErrorMessage = "World ID is required")]
    public int WorldId { get; set; }

    /// <summary>
    /// Gets or sets the campaign visibility (default: Private)
    /// </summary>
    public Visibility Visibility { get; set; } = Visibility.Private;

    /// <summary>
    /// Gets or sets the maximum number of players (1-20, default: 6)
    /// </summary>
    [Range(1, 20, ErrorMessage = "Maximum players must be between 1 and 20")]
    public int MaxPlayers { get; set; } = 6;

    /// <summary>
    /// Gets or sets the cover image as Base64 string (optional)
    /// </summary>
    public string? CoverImageBase64 { get; set; }
}
