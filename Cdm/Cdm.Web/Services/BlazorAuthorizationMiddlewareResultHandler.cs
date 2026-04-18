using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Cdm.Web.Services;

/// <summary>
/// Custom authorization result handler for Blazor Server.
/// Instead of returning HTTP 401/403 (which prevents Blazor from rendering),
/// this handler always allows the request through so Blazor's AuthorizeRouteView
/// can handle authorization at the component level and redirect properly.
/// </summary>
public class BlazorAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    public Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        return next(context);
    }
}
