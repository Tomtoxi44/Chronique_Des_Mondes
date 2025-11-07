// <copyright file="RoleInfo.cs" company="Chronique Des Mondes">
// Copyright (c) Chronique Des Mondes. All rights reserved.
// </copyright>

namespace Cdm.ApiService.Endpoints.Models;

/// <summary>
/// Role information.
/// </summary>
public class RoleInfo
{
    /// <summary>
    /// Gets or sets role identifier.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets role name (Player, GameMaster, Admin).
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets date and time when the role was assigned.
    /// </summary>
    public DateTime AssignedAt { get; set; }
}
