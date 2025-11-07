// <copyright file="GetMyRolesResponse.cs" company="Chronique Des Mondes">
// Copyright (c) Chronique Des Mondes. All rights reserved.
// </copyright>

namespace Cdm.ApiService.Endpoints.Models;

/// <summary>
/// Response for GetMyRoles endpoint.
/// </summary>
public class GetMyRolesResponse
{
    /// <summary>
    /// Gets or sets user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets list of roles assigned to the user.
    /// </summary>
    public List<RoleInfo> Roles { get; set; } = new();
}
