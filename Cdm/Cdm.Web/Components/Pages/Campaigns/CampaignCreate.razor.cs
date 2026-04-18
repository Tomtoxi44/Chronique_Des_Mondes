using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Campaigns;

public partial class CampaignCreate
{
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "worldId")]
    private int? PreSelectedWorldId { get; set; }

    private CreateCampaignDto Model { get; set; } = new();
    private List<WorldDto> Worlds { get; set; } = new();
    private bool IsLoading = false;
    private string ErrorMessage = string.Empty;

    private List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem(L["Campaigns_Title"], "/campaigns"),
        new BreadcrumbItem(L["Campaigns_Create"])
    };

    protected override async Task OnInitializedAsync()
    {
        Worlds = await WorldClient.GetMyWorldsAsync();
        if (PreSelectedWorldId.HasValue && PreSelectedWorldId > 0)
            Model.WorldId = PreSelectedWorldId.Value;
        else if (Worlds.Count == 1)
            Model.WorldId = Worlds[0].Id;
    }

    private async Task HandleCreate()
    {
        if (Model.WorldId == 0)
        {
            ErrorMessage = "Veuillez sélectionner un monde.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await CampaignClient.CreateCampaignAsync(Model);
            if (result != null)
                Nav.NavigateTo($"/campaigns/{result.Id}");
            else
                ErrorMessage = L["Common_ErrorMessage"];
        }
        catch
        {
            ErrorMessage = L["Common_ErrorMessage"];
        }
        finally
        {
            IsLoading = false;
        }
    }
}
