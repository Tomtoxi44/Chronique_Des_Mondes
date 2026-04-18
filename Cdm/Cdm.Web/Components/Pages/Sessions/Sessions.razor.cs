using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Sessions;

public partial class Sessions
{
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private List<CampaignDto> ActiveCampaigns { get; set; } = new();
    private bool IsLoading = true;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        var all = await CampaignClient.GetMyCampaignsAsync();
        ActiveCampaigns = all.Where(c => c.Status == CampaignStatus.Active).ToList();
        IsLoading = false;
    }
}
