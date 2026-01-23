namespace Cdm.Data.Common.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a global role that can be assigned to users (Player, GameMaster, Admin)
/// </summary>
[Table("Roles")]
public class Role
{
    /// <summary>
    /// Gets or sets the unique identifier for the role
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the role
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the role was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of user-role assignments
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
