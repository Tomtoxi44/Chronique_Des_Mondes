using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Characters;

public partial class Characters
{
    [Inject] private ICharacterApiClient CharacterClient { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private List<CharacterDto> CharacterList { get; set; } = new();
    private bool IsLoading = true;
    private CharacterDto? CharacterToDelete;
    private AppConfirmDialog DeleteDialog { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        var result = await CharacterClient.GetMyCharactersAsync();
        CharacterList = result?.ToList() ?? new List<CharacterDto>();
        IsLoading = false;
    }

    private void ConfirmDelete(CharacterDto character)
    {
        CharacterToDelete = character;
        DeleteDialog.Show();
    }

    private async Task DeleteCharacter()
    {
        if (CharacterToDelete == null) return;
        var success = await CharacterClient.DeleteCharacterAsync(CharacterToDelete.Id);
        if (success) CharacterList.Remove(CharacterToDelete);
        CharacterToDelete = null;
    }

    private static string GetInitials(CharacterDto character)
    {
        var parts = character.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }
}
