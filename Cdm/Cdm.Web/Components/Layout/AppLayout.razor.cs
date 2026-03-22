using Microsoft.AspNetCore.Components;

namespace Cdm.Web.Components.Layout;

/// <summary>
/// Application layout component code-behind.
/// </summary>
public partial class AppLayout
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool isCollapsed = false;
    private bool isMobileOpen = false;
    private bool isDarkTheme = true;

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
