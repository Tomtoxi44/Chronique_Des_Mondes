namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cdm.Common.Enums;

/// <summary>
/// Campaign entity representing a role-playing game campaign
/// </summary>
[Table("Campaigns")]
public class Campaign
{
    /// <summary>
    /// Campaign unique identifier
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Campaign name (3-100 characters)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Campaign description (max 5000 characters)
    /// </summary>
    [MaxLength(5000)]
    [Column(TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

    /// <summary>
    /// Campaign visibility level (Private or Public)
    /// </summary>
    [Required]
    public Visibility Visibility { get; set; } = Visibility.Private;

    /// <summary>
    /// Maximum number of players allowed (1-20)
    /// </summary>
    [Required]
    [Range(1, 20)]
    public int MaxPlayers { get; set; } = 6;

    /// <summary>
    /// Cover image URL or path (optional)
    /// </summary>
    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// World ID this campaign belongs to (required)
    /// </summary>
    [Required]
    public int WorldId { get; set; }

    /// <summary>
    /// User ID of the campaign creator (Game Master)
    /// </summary>
    [Required]
    public int CreatedBy { get; set; }

    /// <summary>
    /// Navigation property to the world (required)
    /// </summary>
    [ForeignKey(nameof(WorldId))]
    public virtual World World { get; set; } = null!;

    /// <summary>
    /// Navigation property to the campaign creator
    /// </summary>
    [ForeignKey(nameof(CreatedBy))]
    public virtual User CreatedByUser { get; set; } = null!;

    /// <summary>
    /// Campaign creation timestamp (UTC)
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Campaign active status
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    [Required]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the chapters in this campaign.
    /// </summary>
    public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

    /// <summary>
    /// Gets or sets the events specific to this campaign.
    /// </summary>
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    /// <summary>
    /// Gets or sets the achievements specific to this campaign.
    /// </summary>
    public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
}
