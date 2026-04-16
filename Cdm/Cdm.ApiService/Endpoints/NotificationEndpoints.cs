// -----------------------------------------------------------------------
// <copyright file="NotificationEndpoints.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Endpoints;

using Cdm.Business.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Notification endpoints for the API.
/// </summary>
public static class NotificationEndpoints
{
    private sealed class NotificationEndpointsLogger { }

    /// <summary>
    /// Maps notification endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        // GET /api/notifications
        group.MapGet("/", GetUserNotificationsAsync)
            .WithName("GetUserNotifications")
            .WithOpenApi();

        // GET /api/notifications/unread-count
        group.MapGet("/unread-count", GetUnreadCountAsync)
            .WithName("GetUnreadNotificationCount")
            .WithOpenApi();

        // PUT /api/notifications/{id}/read
        group.MapPut("/{id:int}/read", MarkAsReadAsync)
            .WithName("MarkNotificationAsRead")
            .WithOpenApi();

        // PUT /api/notifications/read-all
        group.MapPut("/read-all", MarkAllAsReadAsync)
            .WithName("MarkAllNotificationsAsRead")
            .WithOpenApi();

        // DELETE /api/notifications/{id}
        group.MapDelete("/{id:int}", DeleteNotificationAsync)
            .WithName("DeleteNotification")
            .WithOpenApi();
    }

    private static async Task<IResult> GetUserNotificationsAsync(
        [FromServices] INotificationService notificationService,
        ILogger<NotificationEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var notifications = await notificationService.GetUserNotificationsAsync(userId.Value);
            return Results.Ok(notifications);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving notifications");
            return Results.Problem(
                title: "An error occurred while retrieving notifications",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetUnreadCountAsync(
        [FromServices] INotificationService notificationService,
        ILogger<NotificationEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var count = await notificationService.GetUnreadCountAsync(userId.Value);
            return Results.Ok(new { Count = count });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving unread notification count");
            return Results.Problem(
                title: "An error occurred while retrieving unread count",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> MarkAsReadAsync(
        [FromRoute] int id,
        [FromServices] INotificationService notificationService,
        ILogger<NotificationEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var success = await notificationService.MarkAsReadAsync(id, userId.Value);

            if (!success)
            {
                return Results.NotFound(new { Error = "Notification not found or not authorized" });
            }

            return Results.Ok(new { Message = "Notification marked as read" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return Results.Problem(
                title: "An error occurred while marking the notification as read",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> MarkAllAsReadAsync(
        [FromServices] INotificationService notificationService,
        ILogger<NotificationEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var count = await notificationService.MarkAllAsReadAsync(userId.Value);
            return Results.Ok(new { MarkedAsRead = count });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking all notifications as read");
            return Results.Problem(
                title: "An error occurred while marking all notifications as read",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> DeleteNotificationAsync(
        [FromRoute] int id,
        [FromServices] INotificationService notificationService,
        ILogger<NotificationEndpointsLogger> logger,
        HttpContext httpContext)
    {
        try
        {
            var userId = GetUserId(httpContext);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var success = await notificationService.DeleteNotificationAsync(id, userId.Value);

            if (!success)
            {
                return Results.NotFound(new { Error = "Notification not found or not authorized" });
            }

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return Results.Problem(
                title: "An error occurred while deleting the notification",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static int? GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        return userId;
    }
}
