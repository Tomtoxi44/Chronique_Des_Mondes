using Microsoft.AspNetCore.Components;

namespace Cdm.Web.Components.Layout;

/// <summary>
/// Application layout component code-behind.
/// </summary>
public partial class AppLayout
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private bool isCollapsed = false;

    /// <summary>
    /// Toggles the sidebar collapsed state.
    /// </summary>
    private void ToggleSidebar()
    {
        this.isCollapsed = !this.isCollapsed;
    }

    /// <summary>
    /// Determines if the given path matches the current navigation path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is active, false otherwise.</returns>
    private bool IsActive(string path)
    {
        return this.Navigation.Uri.EndsWith(path, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the page title based on the current path.
    /// </summary>
    /// <returns>The page title string.</returns>
    private string GetPageTitle()
    {
        var path = new Uri(this.Navigation.Uri).AbsolutePath;
        return path switch
        {
            "/" => "Accueil",
            "/characters" => "Mes Personnages",
            "/campaigns" => "Mes Campagnes",
            "/dice" => "Lanceur de Dés",
            _ => "Chronique des Mondes"
        };
    }
}
