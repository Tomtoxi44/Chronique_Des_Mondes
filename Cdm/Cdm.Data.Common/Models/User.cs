namespace Cdm.Data.Common.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// User entity representing registered users in the system
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// User unique identifier
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// User email address (unique)
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User display nickname
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// User unique username (3-30 characters, optional)
    /// </summary>
    [MaxLength(30)]
    public string? Username { get; set; }

    /// <summary>
    /// Avatar image URL or path
    /// </summary>
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User preferences stored as JSON (theme, notifications, etc.)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Preferences { get; set; }

    /// <summary>
    /// BCrypt hashed password (work factor 12)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Account creation timestamp (UTC)
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login timestamp (UTC)
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Account active status
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last update timestamp (UTC)
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
