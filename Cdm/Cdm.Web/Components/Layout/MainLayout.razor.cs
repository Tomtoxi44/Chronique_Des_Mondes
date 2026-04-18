using Cdm.Web.Resources;
using Cdm.Web.Services;
using Cdm.Web.Services.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Cdm.Web.Components.Layout;

public partial class MainLayout : IDisposable
{
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private bool IsCollapsed = false;
    private bool IsMobileOpen = false;
    private bool IsDarkMode = true;
    private bool IsDropdownOpen = false;
    private string UserName = string.Empty;
    private string UserInitials = "?";

    protected override async Task OnInitializedAsync()
    {
        NavContext.OnContextChanged += OnContextChanged;

        var authState = await AuthProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            UserName = user.Claims.FirstOrDefault(c => c.Type == "nickname")?.Value
                    ?? user.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value
                    ?? string.Empty;

            UserInitials = BuildInitials(UserName);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var theme = await JS.InvokeAsync<string?>("localStorage.getItem", "cdm-theme");
                IsDarkMode = theme != "light";
                await InvokeAsync(StateHasChanged);
            }
            catch { /* ignore — prerender ou SSR */ }
        }
    }

    private void OnContextChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void ToggleSidebar()
    {
        IsCollapsed = !IsCollapsed;
        if (IsMobileOpen) IsMobileOpen = false;
    }

    private void ToggleMobile()
    {
        IsMobileOpen = !IsMobileOpen;
    }

    private void CloseMobile()
    {
        IsMobileOpen = false;
    }

    private void ToggleDropdown()
    {
        IsDropdownOpen = !IsDropdownOpen;
    }

    private async Task ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        var theme = IsDarkMode ? "dark" : "light";
        try
        {
            await JS.InvokeVoidAsync("localStorage.setItem", "cdm-theme", theme);
            await JS.InvokeVoidAsync("eval",
                $"document.documentElement.setAttribute('data-theme', '{theme}')");
        }
        catch { /* ignore */ }
    }

    private async Task Logout()
    {
        IsDropdownOpen = false;
        var provider = (CustomAuthStateProvider)AuthProvider;
        await provider.MarkUserAsLoggedOutAsync();
        Nav.NavigateTo("/login");
    }

    private string GetMainClass()
    {
        var classes = new List<string>();
        if (IsCollapsed) classes.Add("sidebar-collapsed");
        else classes.Add("sidebar-normal");
        if (NavContext.HasContext) classes.Add("has-secondary");
        return string.Join(" ", classes);
    }

    private string GetSecondaryLeft() => IsCollapsed ? "sidebar-after-collapsed" : "sidebar-after-normal";

    private string ActiveClass(string href)
    {
        var currentUri = Nav.Uri;
        var baseUri = Nav.BaseUri;
        var path = currentUri.StartsWith(baseUri)
            ? "/" + currentUri[baseUri.Length..].TrimStart('/')
            : "/";

        if (href == "/")
            return path == "/" ? "active" : string.Empty;

        return path.StartsWith(href, StringComparison.OrdinalIgnoreCase) ? "active" : string.Empty;
    }

    private static string BuildInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }

    public void Dispose()
    {
        NavContext.OnContextChanged -= OnContextChanged;
    }
}
