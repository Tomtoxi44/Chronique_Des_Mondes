using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Worlds;

public partial class WorldDetail : IDisposable
{
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    [Parameter] public int WorldId { get; set; }

    private WorldDto? World;
    private List<CampaignDto> Campaigns = new();
    private bool IsLoading = true;
    private AppConfirmDialog DeleteDialog { get; set; } = default!;

    private List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem(L["Worlds_Title"], "/worlds"),
        new BreadcrumbItem(World?.Name ?? "...")
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected override void OnParametersSet()
    {
        if (World != null)
            SetSecondaryNav();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var worldTask = WorldClient.GetWorldByIdAsync(WorldId);
            var campaignsTask = CampaignClient.GetMyCampaignsAsync();
            await Task.WhenAll(worldTask, campaignsTask);

            World = worldTask.Result;
            var allCampaigns = campaignsTask.Result;
            Campaigns = allCampaigns.Where(c => c.WorldId == WorldId).ToList();

            if (World != null)
                SetSecondaryNav();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SetSecondaryNav()
    {
        if (World == null) return;

        var items = Campaigns.Select(c => new SecondaryNavItem(
            Label: c.Name,
            Href: $"/campaigns/{c.Id}",
            Icon: "bi-map",
            IsActive: false
        )).ToList<SecondaryNavItem>();

        items.Insert(0, new SecondaryNavItem(
            Label: "Vue d'ensemble",
            Href: $"/worlds/{WorldId}",
            Icon: "bi-info-circle",
            IsActive: true
        ));

        NavContext.SetContext(
            sectionTitle: World.Name,
            backHref: "/worlds",
            backLabel: L["Nav_Worlds"],
            items: items,
            gameType: World.GameType
        );
    }

    private void ConfirmDeleteWorld() => DeleteDialog.Show();

    private async Task DeleteWorldAsync()
    {
        if (World == null) return;
        var success = await WorldClient.DeleteWorldAsync(World.Id);
        if (success)
        {
            NavContext.ClearContext();
            Nav.NavigateTo("/worlds");
        }
    }

    public string GetStatusClass(CampaignStatus status) => status switch
    {
        CampaignStatus.Active => "status-active",
        CampaignStatus.Planning => "status-planning",
        CampaignStatus.Completed => "status-completed",
        CampaignStatus.Cancelled => "status-cancelled",
        CampaignStatus.OnHold => "status-paused",
        _ => ""
    };

    public string GetStatusLabel(CampaignStatus status) => status switch
    {
        CampaignStatus.Active => L["Campaigns_Status_Active"],
        CampaignStatus.Planning => L["Campaigns_Status_Planning"],
        CampaignStatus.Completed => L["Campaigns_Status_Completed"],
        CampaignStatus.Cancelled => L["Campaigns_Status_Cancelled"],
        CampaignStatus.OnHold => L["Campaigns_Status_OnHold"],
        _ => status.ToString()
    };

    public void Dispose()
    {
        NavContext.ClearContext();
    }
}
