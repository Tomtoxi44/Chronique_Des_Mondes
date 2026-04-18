using Cdm.Common.Enums;

namespace Cdm.Web.Services;

/// <summary>
/// Provides contextual navigation state to the MainLayout so it knows
/// when to activate the secondary sidebar (e.g., inside a World or Campaign detail).
/// </summary>
public class NavigationContextService
{
    public string? SectionTitle { get; private set; }
    public string? BackHref { get; private set; }
    public string? BackLabel { get; private set; }
    public GameType? GameType { get; private set; }
    public List<SecondaryNavItem> Items { get; private set; } = new();

    public bool HasContext => Items.Count > 0 || !string.IsNullOrEmpty(SectionTitle);

    public event Action? OnContextChanged;

    /// <summary>
    /// Called when the user clicks an expand/collapse toggle on a nav tree item.
    /// The string parameter is the item's ToggleKey.
    /// </summary>
    public Func<string, Task>? OnToggleItem { get; set; }

    public void SetContext(string sectionTitle, string backHref, string backLabel, List<SecondaryNavItem> items, GameType? gameType = null)
    {
        SectionTitle = sectionTitle;
        BackHref = backHref;
        BackLabel = backLabel;
        Items = items;
        GameType = gameType;
        OnContextChanged?.Invoke();
    }

    public void ClearContext()
    {
        SectionTitle = null;
        BackHref = null;
        BackLabel = null;
        Items = new();
        GameType = null;
        OnToggleItem = null;
        OnContextChanged?.Invoke();
    }
}

public record SecondaryNavItem(
    string Label,
    string Href,
    string Icon,
    bool IsActive = false,
    List<SecondaryNavItem>? Children = null,
    bool IsExpanded = false,
    bool IsSection = false,
    string? ToggleKey = null);
