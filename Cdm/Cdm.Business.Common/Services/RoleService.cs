namespace Cdm.Business.Common.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;

/// <summary>
/// Service for managing user roles
/// </summary>
public class RoleService : IRoleService
{
    private readonly AppDbContext dbContext;
    private readonly ILogger<RoleService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleService"/> class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="logger">The logger</param>
    public RoleService(AppDbContext dbContext, ILogger<RoleService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all roles assigned to a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <returns>A list of roles assigned to the user</returns>
    public async Task<List<UserRoleDto>> GetUserRolesAsync(int userId)
    {
        this.logger.LogInformation("Getting roles for user {UserId}", userId);

        var userRoles = await this.dbContext.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .Select(ur => new UserRoleDto
            {
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name,
                AssignedAt = ur.AssignedAt
            })
            .ToListAsync();

        this.logger.LogInformation("Found {Count} roles for user {UserId}", userRoles.Count, userId);
        return userRoles;
    }

    /// <summary>
    /// Checks if a user has a specific role
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="roleName">The role name to check</param>
    /// <returns>True if the user has the role, false otherwise</returns>
    public async Task<bool> HasRoleAsync(int userId, string roleName)
    {
        this.logger.LogInformation("Checking if user {UserId} has role {RoleName}", userId, roleName);

        var hasRole = await this.dbContext.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);

        this.logger.LogInformation("User {UserId} has role {RoleName}: {HasRole}", userId, roleName, hasRole);
        return hasRole;
    }

    /// <summary>
    /// Requests the GameMaster role for a user (automatic assignment)
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <returns>True if the role was assigned, false if the user already has it</returns>
    public async Task<bool> RequestGameMasterRoleAsync(int userId)
    {
        this.logger.LogInformation("User {UserId} requesting GameMaster role", userId);

        // Check if user already has GameMaster role
        var hasRole = await this.HasRoleAsync(userId, "GameMaster");
        if (hasRole)
        {
            this.logger.LogWarning("User {UserId} already has GameMaster role", userId);
            return false;
        }

        // Get GameMaster role ID
        var gameMasterRole = await this.dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == "GameMaster");

        if (gameMasterRole == null)
        {
            this.logger.LogError("GameMaster role not found in database");
            throw new InvalidOperationException("GameMaster role not found");
        }

        // Assign GameMaster role
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = gameMasterRole.Id,
            AssignedAt = DateTime.UtcNow
        };

        this.dbContext.UserRoles.Add(userRole);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("User {UserId} successfully assigned GameMaster role", userId);
        return true;
    }

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="roleId">The role identifier</param>
    /// <returns>True if the role was assigned successfully</returns>
    public async Task<bool> AssignRoleAsync(int userId, int roleId)
    {
        this.logger.LogInformation("Assigning role {RoleId} to user {UserId}", roleId, userId);

        // Check if user already has this role
        var existingUserRole = await this.dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (existingUserRole != null)
        {
            this.logger.LogWarning("User {UserId} already has role {RoleId}", userId, roleId);
            return false;
        }

        // Assign role
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };

        this.dbContext.UserRoles.Add(userRole);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("Role {RoleId} successfully assigned to user {UserId}", roleId, userId);
        return true;
    }
}
