namespace Cdm.Business.Abstraction.DTOs;

using System;

/// <summary>
/// Data Transfer Object for a User-Role assignment
/// </summary>
public class UserRoleDto
{
    /// <summary>
    /// Gets or sets the user identifier
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the role identifier
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the role name
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the role was assigned to the user
    /// </summary>
    public DateTime AssignedAt { get; set; }
}
