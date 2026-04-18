using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Worlds;

public partial class Worlds
{
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private List<WorldDto> WorldsList { get; set; } = new();
    private bool IsLoading = true;
    private string ErrorMessage = string.Empty;
    private WorldDto? WorldToDelete;
    private AppConfirmDialog DeleteDialog { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadWorldsAsync();
    }

    private async Task LoadWorldsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            WorldsList = await WorldClient.GetMyWorldsAsync();
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

    private void NavigateToWorld(int worldId) => Nav.NavigateTo($"/worlds/{worldId}");

    private void ConfirmDelete(WorldDto world)
    {
        WorldToDelete = world;
        DeleteDialog.Show();
    }

    private async Task DeleteWorld()
    {
        if (WorldToDelete == null) return;
        var success = await WorldClient.DeleteWorldAsync(WorldToDelete.Id);
        if (success)
        {
            WorldsList.Remove(WorldToDelete);
        }
        WorldToDelete = null;
    }
}
