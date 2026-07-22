// -----------------------------------------------------------------------
// <copyright file="ClaimsPrincipalExtensions.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService;

using System.Security.Claims;

/// <summary>
/// Helpers to read the authenticated user's identity from claims, shared by all endpoints
/// (replaces the per-endpoint duplicated <c>GetUserId</c> helpers).
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Gets the current user's integer id from the NameIdentifier claim, or null.</summary>
    public static int? GetUserId(this ClaimsPrincipal? user)
    {
        var claim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>Gets the current user's integer id from the request context, or null.</summary>
    public static int? GetUserId(this HttpContext? context) => context?.User.GetUserId();
}
