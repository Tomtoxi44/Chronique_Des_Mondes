using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Campaigns;

public partial class Campaigns
{
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private List<CampaignDto> CampaignList { get; set; } = new();
    private bool IsLoading = true;
    private CampaignDto? CampaignToDelete;
    private AppConfirmDialog DeleteDialog { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        CampaignList = await CampaignClient.GetMyCampaignsAsync();
        IsLoading = false;
    }

    private void ConfirmDelete(CampaignDto campaign)
    {
        CampaignToDelete = campaign;
        DeleteDialog.Show();
    }

    private async Task DeleteCampaign()
    {
        if (CampaignToDelete == null) return;
        var success = await CampaignClient.DeleteCampaignAsync(CampaignToDelete.Id);
        if (success) CampaignList.Remove(CampaignToDelete);
        CampaignToDelete = null;
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
}
