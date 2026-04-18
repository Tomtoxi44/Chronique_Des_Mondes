using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Characters;

public partial class CharacterEdit
{
    [Inject] private ICharacterApiClient CharacterClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    [Parameter] public int CharacterId { get; set; }

    private CharacterDto? Character;
    private UpdateCharacterDto Model { get; set; } = new();
    private bool IsLoading = true;
    private bool IsSaving = false;
    private string ErrorMessage = string.Empty;
    private string SuccessMessage = string.Empty;

    private List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem(L["Characters_Title"], "/characters"),
        new BreadcrumbItem(Character?.Name ?? "...")
    };

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        Character = await CharacterClient.GetCharacterByIdAsync(CharacterId);
        if (Character != null)
        {
            Model = new UpdateCharacterDto
            {
                Name = Character.Name,
                FirstName = Character.FirstName,
                Description = Character.Description,
                Age = Character.Age,
                AvatarUrl = Character.AvatarUrl
            };
        }
        else
        {
            ErrorMessage = L["Common_ErrorMessage"];
        }
        IsLoading = false;
    }

    private async Task HandleSave()
    {
        IsSaving = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        try
        {
            var result = await CharacterClient.UpdateCharacterAsync(CharacterId, Model);
            if (result != null)
            {
                SuccessMessage = L["Profile_SaveSuccess"];
                Character = result;
            }
            else
            {
                ErrorMessage = L["Common_ErrorMessage"];
            }
        }
        catch
        {
            ErrorMessage = L["Common_ErrorMessage"];
        }
        finally
        {
            IsSaving = false;
        }
    }
}
