using Cdm.Web.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Globalization;

namespace Cdm.Web.Components.Pages.Settings;

public partial class Settings
{
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private bool IsDarkMode = true;

    // Source de vérité = la culture réellement appliquée par ASP.NET Core (cookie).
    // Auparavant l'état du bouton venait de localStorage, ce qui pouvait diverger
    // de la langue affichée (UI en anglais mais bouton « Français » surligné).
    private string CurrentCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

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
            catch { /* ignore */ }
        }
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

    private void SetLanguage(string culture)
    {
        if (string.Equals(culture, CurrentCulture, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Délègue au endpoint serveur qui pose le cookie de culture standard
        // (.AspNetCore.Culture) puis redirige : c'est lui qui fait réellement
        // basculer la langue de l'interface.
        this.Nav.NavigateTo(
            $"/set-culture?culture={Uri.EscapeDataString(culture)}&redirectUri={Uri.EscapeDataString("/settings")}",
            forceLoad: true);
    }
}
