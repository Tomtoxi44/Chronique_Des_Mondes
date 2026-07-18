using Cdm.Web.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Cdm.Web.Components.Pages.Settings;

public partial class Settings
{
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private bool IsDarkMode = true;
    private string CurrentCulture = "fr";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var theme = await JS.InvokeAsync<string?>("localStorage.getItem", "cdm-theme");
                IsDarkMode = theme != "light";

                var culture = await JS.InvokeAsync<string?>("localStorage.getItem", "cdm-culture");
                CurrentCulture = culture ?? "fr";

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

    private async Task SetLanguage(string culture)
    {
        CurrentCulture = culture;

        // Mémorise le choix pour l'état visuel du bouton.
        try
        {
            await JS.InvokeVoidAsync("localStorage.setItem", "cdm-culture", culture);
        }
        catch { /* non bloquant */ }

        // Délègue au endpoint serveur qui pose le cookie de culture standard
        // (.AspNetCore.Culture) puis redirige : c'est lui qui fait réellement
        // basculer la langue de l'interface.
        this.Nav.NavigateTo(
            $"/set-culture?culture={Uri.EscapeDataString(culture)}&redirectUri={Uri.EscapeDataString("/settings")}",
            forceLoad: true);
    }
}
