namespace Cdm.ApiService.Endpoints;

using System.Security.Claims;
using Cdm.ApiService.Endpoints.Models;
using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints for managing user roles.
/// </summary>
public static class RoleEndpoints
{
    /// <summary>
    /// Maps role-related endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapRoleEndpoints(this WebApplication app)
    {
        var roleGroup = app.MapGroup("/api/users")
            .WithTags("Roles")
            .RequireAuthorization();

        // GET /api/users/my-roles
        roleGroup.MapGet("/my-roles", GetMyRoles)
            .WithName("GetMyRoles")
            .WithSummary("Get all roles assigned to the current user")
            .WithDescription("Returns a list of all roles (Player, GameMaster, Admin) assigned to the authenticated user")
            .Produces<GetMyRolesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // POST /api/users/request-gamemaster-role
        roleGroup.MapPost("/request-gamemaster-role", RequestGameMasterRole)
            .WithName("RequestGameMasterRole")
            .WithSummary("Request GameMaster role")
            .WithDescription("Automatically assigns the GameMaster role to the authenticated user if they don't already have it")
            .Produces<RequestGameMasterRoleResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// Get all roles assigned to the current user.
    /// </summary>
    /// <param name="user">Current authenticated user.</param>
    /// <param name="roleService">Role service.</param>
    /// <returns>Response with user roles.</returns>
    private static async Task<IResult> GetMyRoles(
        ClaimsPrincipal user,
        [FromServices] IRoleService roleService)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var roles = await roleService.GetUserRolesAsync(userId);

        return Results.Ok(new GetMyRolesResponse
        {
            UserId = userId,
            Roles = roles.Select(r => new RoleInfo
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                AssignedAt = r.AssignedAt
            }).ToList()
        });
    }

    /// <summary>
    /// Request GameMaster role for the current user.
    /// </summary>
    /// <param name="user">Current authenticated user.</param>
    /// <param name="roleService">Role service.</param>
    /// <returns>Response indicating success or conflict.</returns>
    private static async Task<IResult> RequestGameMasterRole(
        ClaimsPrincipal user,
        [FromServices] IRoleService roleService)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var assigned = await roleService.RequestGameMasterRoleAsync(userId);

        if (!assigned)
        {
            return Results.Conflict(new RequestGameMasterRoleResponse
            {
                Success = false,
                Message = "User already has GameMaster role"
            });
        }

        return Results.Ok(new RequestGameMasterRoleResponse
        {
            Success = true,
            Message = "GameMaster role successfully assigned"
        });
    }
}
