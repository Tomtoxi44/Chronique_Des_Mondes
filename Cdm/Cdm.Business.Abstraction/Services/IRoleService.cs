namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing user roles
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Gets all roles assigned to a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <returns>A list of roles assigned to the user</returns>
    Task<List<UserRoleDto>> GetUserRolesAsync(int userId);

    /// <summary>
    /// Checks if a user has a specific role
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="roleName">The role name to check</param>
    /// <returns>True if the user has the role, false otherwise</returns>
    Task<bool> HasRoleAsync(int userId, string roleName);

    /// <summary>
    /// Requests the GameMaster role for a user (automatic assignment)
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <returns>True if the role was assigned, false if the user already has it</returns>
    Task<bool> RequestGameMasterRoleAsync(int userId);

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="roleId">The role identifier</param>
    /// <returns>True if the role was assigned successfully</returns>
    Task<bool> AssignRoleAsync(int userId, int roleId);
}
