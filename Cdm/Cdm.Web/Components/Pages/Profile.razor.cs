using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Cdm.Web.Components.Pages;

/// <summary>
/// User profile page code-behind.
/// </summary>
public partial class Profile
{
    /// <summary>
    /// Extracts the user email from the authentication state.
    /// </summary>
    /// <param name="context">The authentication state context.</param>
    /// <returns>The user's email address or a default value.</returns>
    private string GetUserEmail(AuthenticationState context)
    {
        return context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value 
            ?? "email@example.com";
    }
}
