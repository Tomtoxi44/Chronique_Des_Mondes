using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Cdm.Web.Services.State;

namespace Cdm.Web.Components.Shared;

/// <summary>
/// User menu component for displaying authentication state and user actions.
/// </summary>
public partial class UserMenu : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private CustomAuthStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProviderBase { get; set; } = default!;
    
    private bool isDropdownOpen = false;

    /// <summary>
    /// Initializes the component and refreshes authentication state.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Force a refresh of the authentication state after the first render
            // This ensures the UI updates correctly when navigating after login
            await Task.Delay(100);
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Toggles the dropdown menu visibility.
    /// </summary>
    private void ToggleDropdown()
    {
        isDropdownOpen = !isDropdownOpen;
    }

    /// <summary>
    /// Closes the dropdown menu.
    /// </summary>
    private void CloseDropdown()
    {
        isDropdownOpen = false;
    }

    /// <summary>
    /// Gets the user's email from the authentication context.
    /// </summary>
    private string GetUserEmail(AuthenticationState context)
    {
        return context.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
    }

    /// <summary>
    /// Handles user logout.
    /// </summary>
    private async Task HandleLogout()
    {
        CloseDropdown();
        await AuthStateProvider.MarkUserAsLoggedOutAsync();
        NavigationManager.NavigateTo("/login", forceLoad: false);
    }
}
