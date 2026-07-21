using Cdm.Web.Extensions;
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
    [Inject] private MarketplaceApiClient MarketClient { get; set; } = default!;
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

    private async Task ToggleShare(CampaignDto campaign)
    {
        var newValue = !campaign.IsShared;
        var ok = await MarketClient.SetCampaignSharedAsync(campaign.Id, newValue);
        if (ok)
        {
            campaign.IsShared = newValue;
        }
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

    public string GetStatusClass(CampaignStatus status) => status.ToCssClass();

    public string GetStatusLabel(CampaignStatus status) => L[status.ToLabelKey()];
}
