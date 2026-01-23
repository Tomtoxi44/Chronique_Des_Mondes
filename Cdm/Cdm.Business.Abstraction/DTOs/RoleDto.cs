namespace Cdm.Business.Abstraction.DTOs;

using System;

/// <summary>
/// Data Transfer Object for a Role
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Gets or sets the role identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the role name (Player, GameMaster, Admin)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the role was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
