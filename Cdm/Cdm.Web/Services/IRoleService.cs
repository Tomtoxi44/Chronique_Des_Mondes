namespace Cdm.Web.Services;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Service for managing user roles on the client side
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Gets all roles assigned to the current authenticated user
    /// </summary>
    /// <returns>List of user roles with details</returns>
    Task<List<UserRoleInfo>?> GetMyRolesAsync();

    /// <summary>
    /// Requests the GameMaster role for the current user
    /// </summary>
    /// <returns>True if successful, false if already has the role or error occurred</returns>
    Task<(bool Success, string Message)> RequestGameMasterRoleAsync();

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    /// <param name="roleName">Role name to check (Player, GameMaster, Admin)</param>
    /// <returns>True if user has the role</returns>
    Task<bool> HasRoleAsync(string roleName);
}

/// <summary>
/// User role information
/// </summary>
public class UserRoleInfo
{
    /// <summary>
    /// Role identifier
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Role name (Player, GameMaster, Admin)
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Date when the role was assigned
    /// </summary>
    public DateTime AssignedAt { get; set; }
}
