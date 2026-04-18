using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Home;

public partial class Home
{
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private ICharacterApiClient CharacterClient { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private bool IsLoading = true;
    private int WorldCount = 0;
    private int CharacterCount = 0;
    private int CampaignCount = 0;
    private WorldDto? LastWorld;
    private string WelcomeTitle = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        var nickname = authState.User.Claims
            .FirstOrDefault(c => c.Type == "nickname")?.Value
            ?? authState.User.Claims
            .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;

        WelcomeTitle = string.IsNullOrEmpty(nickname)
            ? L["Home_WelcomeDefault"]
            : string.Format(L["Home_Welcome"], nickname);

        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        IsLoading = true;

        try
        {
            var worldsTask = WorldClient.GetMyWorldsAsync();
            var campaignsTask = CampaignClient.GetMyCampaignsAsync();
            var charactersTask = CharacterClient.GetMyCharactersAsync();

            await Task.WhenAll(worldsTask, campaignsTask, charactersTask);

            var worlds = worldsTask.Result ?? new List<WorldDto>();
            var campaigns = campaignsTask.Result ?? new List<CampaignDto>();
            var characters = charactersTask.Result ?? Enumerable.Empty<Cdm.Web.Shared.DTOs.Models.CharacterDto>();

            WorldCount = worlds.Count;
            CampaignCount = campaigns.Count;
            CharacterCount = characters.Count();
            LastWorld = worlds.OrderByDescending(w => w.UpdatedAt ?? w.CreatedAt).FirstOrDefault();
        }
        catch
        {
            /* Erreurs loguées dans les API clients */
        }
        finally
        {
            IsLoading = false;
        }
    }
}
