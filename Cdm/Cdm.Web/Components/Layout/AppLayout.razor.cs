using Cdm.Common.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Cdm.Web.Components.Layout;

/// <summary>
/// Application layout component code-behind.
/// </summary>
public partial class AppLayout
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private bool isCollapsed = false;
    private bool isMobileOpen = false;
    private bool isDarkTheme = true;
    private string _currentTheme = "theme-generic";

    private string AppLayoutClass => $"app-layout {_currentTheme} {(isDarkTheme ? "" : "light-theme")}".Trim();

    /// <summary>
    /// Toggles the sidebar collapsed state.
    /// </summary>
    private void ToggleSidebar()
    {
        this.isCollapsed = !this.isCollapsed;
    }

    /// <summary>
    /// Toggles the mobile sidebar visibility.
    /// </summary>
    private void ToggleMobileSidebar()
    {
        this.isMobileOpen = !this.isMobileOpen;
    }

    /// <summary>
    /// Closes the mobile sidebar.
    /// </summary>
    private void CloseMobileSidebar()
    {
        this.isMobileOpen = false;
    }

    /// <summary>
    /// Toggles the theme between dark and light.
    /// </summary>
    private void ToggleTheme()
    {
        this.isDarkTheme = !this.isDarkTheme;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var savedTheme = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "cdm-theme-override");
            if (!string.IsNullOrEmpty(savedTheme) && IsValidTheme(savedTheme))
            {
                _currentTheme = savedTheme;
                StateHasChanged();
            }
        }
    }

    /// <summary>
    /// Returns the CSS theme class corresponding to the given <see cref="GameType"/>.
    /// </summary>
    public static string GetThemeClass(GameType gameType) => gameType switch
    {
        GameType.DnD5e => "theme-dnd",
        GameType.Pathfinder => "theme-pathfinder",
        GameType.CallOfCthulhu => "theme-cthulhu",
        GameType.Warhammer => "theme-warhammer",
        GameType.Cyberpunk => "theme-cyberpunk",
        GameType.Skyrim => "theme-skyrim",
        _ => "theme-generic"
    };

    /// <summary>
    /// Applies a theme and persists it to localStorage.
    /// </summary>
    public async Task SetThemeAsync(string themeClass)
    {
        if (IsValidTheme(themeClass))
        {
            _currentTheme = themeClass;
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "cdm-theme-override", themeClass);
            StateHasChanged();
        }
    }

    private static bool IsValidTheme(string theme) => theme is
        "theme-generic" or "theme-dnd" or "theme-pathfinder" or
        "theme-cthulhu" or "theme-warhammer" or "theme-cyberpunk" or
        "theme-skyrim" or "light-theme";

    /// <summary>
    /// Determines if the given path matches the current navigation path.
    /// </summary>
    /// <param name="href">The path to check.</param>
    /// <returns>True if the path is active, false otherwise.</returns>
    private bool IsActive(string href)
    {
        var currentUri = this.NavigationManager.Uri;
        var baseUri = this.NavigationManager.BaseUri;
        var relativePath = currentUri.Replace(baseUri, "/").TrimEnd('/');
        
        if (string.IsNullOrEmpty(relativePath)) relativePath = "/";
        
        if (href == "/")
        {
            return relativePath == "/";
        }
        
        return relativePath.StartsWith(href, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the page title based on the current path.
    /// </summary>
    /// <returns>The page title string.</returns>
    private string GetPageTitle()
    {
        var currentUri = this.NavigationManager.Uri;
        var baseUri = this.NavigationManager.BaseUri;
        var relativePath = currentUri.Replace(baseUri, "/").TrimEnd('/').ToLower();

        return relativePath switch
        {
            "/" or "" => "Tableau de bord",
            "/characters" => "Mes Personnages",
            "/campaigns" => "Mes Campagnes",
            "/campaigns/create" => "Nouvelle Campagne",
            "/dice" => "Lanceur de Dés",
            "/profile" => "Mon Profil",
            "/settings" => "Paramètres",
            _ => "Chronique des Mondes"
        };
    }
}
