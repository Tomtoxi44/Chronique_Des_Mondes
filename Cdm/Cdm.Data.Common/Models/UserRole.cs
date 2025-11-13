namespace Cdm.Data.Common.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Junction table representing the many-to-many relationship between Users and global Roles
/// </summary>
[Table("UserRoles")]
public class UserRole
{
    /// <summary>
    /// Gets or sets the user identifier
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user navigation property
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the role identifier
    /// </summary>
    [Required]
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the role navigation property
    /// </summary>
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time when the role was assigned to the user
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
