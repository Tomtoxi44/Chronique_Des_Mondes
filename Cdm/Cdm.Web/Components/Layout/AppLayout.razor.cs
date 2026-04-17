using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Cdm.Web.Services;

namespace Cdm.Web.Components.Layout;

public partial class AppLayout : IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ThemeStateService ThemeService { get; set; } = default!;

    private bool _sidebarExpanded = true;
    private bool _mobileOpen = false;
    private DesignThemeModes _themeMode = DesignThemeModes.Dark;

    protected override void OnInitialized()
    {
        ThemeService.OnThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void ToggleTheme()
    {
        _themeMode = _themeMode == DesignThemeModes.Dark
            ? DesignThemeModes.Light
            : DesignThemeModes.Dark;
    }

    private void ToggleMobile()
    {
        _mobileOpen = !_mobileOpen;
    }

    private void CloseMobile()
    {
        _mobileOpen = false;
    }

    public void Dispose()
    {
        ThemeService.OnThemeChanged -= OnThemeChanged;
    }
}
