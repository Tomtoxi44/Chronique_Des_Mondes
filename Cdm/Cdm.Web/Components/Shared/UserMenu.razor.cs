using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Cdm.Web.Services.State;

namespace Cdm.Web.Components.Shared;

public partial class UserMenu : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private CustomAuthStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProviderBase { get; set; } = default!;

    private bool _isOpen;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(100);
            await InvokeAsync(StateHasChanged);
        }
    }

    private void ToggleDropdown() => _isOpen = !_isOpen;
    private void CloseDropdown() => _isOpen = false;

    private string GetUserEmail(AuthenticationState context)
    {
        return context.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
    }

    private string GetInitials(AuthenticationState context)
    {
        var name = context.User.Identity?.Name ?? "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        return name.Length >= 2 ? name[..2].ToUpper() : name.ToUpper();
    }

    private async Task HandleLogout()
    {
        _isOpen = false;
        await AuthStateProvider.MarkUserAsLoggedOutAsync();
        NavigationManager.NavigateTo("/login", forceLoad: true);
    }
}
