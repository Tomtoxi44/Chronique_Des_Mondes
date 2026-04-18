using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Characters;

public partial class CharacterCreate
{
    [Inject] private ICharacterApiClient CharacterClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private CreateCharacterDto Model { get; set; } = new();
    private bool IsLoading = false;
    private string ErrorMessage = string.Empty;

    private List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem(L["Characters_Title"], "/characters"),
        new BreadcrumbItem(L["Characters_Create"])
    };

    private async Task HandleCreate()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await CharacterClient.CreateCharacterAsync(Model);
            if (result != null)
                Nav.NavigateTo("/characters");
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
