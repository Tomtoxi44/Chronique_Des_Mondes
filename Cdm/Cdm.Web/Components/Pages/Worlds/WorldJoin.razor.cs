using Cdm.Common.Enums;
using Cdm.Web.Models;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cdm.Web.Components.Pages.Worlds;

public partial class WorldJoin
{
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private ICharacterApiClient CharacterClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Parameter] public string Token { get; set; } = string.Empty;

    private WorldModel? World;
    private List<CharacterDto> AvailableCharacters = new();
    private int? SelectedCharacterId;
    private bool IsLoading = true;
    private bool IsLoadingCharacters = false;
    private bool IsJoining = false;
    private bool JoinSuccess = false;
    private string? JoinError;
    private string? JoinedCharacterName;

    protected override async Task OnInitializedAsync()
    {
        await LoadWorldAsync();
        if (World != null)
            await LoadAvailableCharactersAsync();
        IsLoading = false;
    }

    private async Task LoadWorldAsync()
    {
        if (string.IsNullOrEmpty(Token)) return;
        var dto = await WorldClient.GetWorldByInviteTokenAsync(Token);
        if (dto != null)
            World = WorldModel.FromDto(dto);
    }

    private async Task LoadAvailableCharactersAsync()
    {
        IsLoadingCharacters = true;
        var characters = await CharacterClient.GetMyCharactersAsync();
        AvailableCharacters = characters?
            .Where(c => !c.IsLocked && !c.SourceCharacterId.HasValue)
            .ToList() ?? new List<CharacterDto>();
        IsLoadingCharacters = false;
    }

    private async Task JoinWorld()
    {
        if (SelectedCharacterId == null || World == null) return;
        IsJoining = true;
        JoinError = null;

        var result = await WorldClient.JoinWorldAsync(Token, SelectedCharacterId.Value);
        if (result != null)
        {
            JoinedCharacterName = AvailableCharacters.FirstOrDefault(c => c.Id == SelectedCharacterId)?.FirstName
                ?? AvailableCharacters.FirstOrDefault(c => c.Id == SelectedCharacterId)?.Name
                ?? "votre personnage";

            // If D&D 5e world → redirect to the D&D wizard
            if (World.GameType == GameType.DnD5e)
            {
                Nav.NavigateTo($"/worlds/dnd-wizard/{result.WorldId}");
                return;
            }

            JoinSuccess = true;
        }
        else
        {
            JoinError = "Impossible de rejoindre ce monde. Le personnage est peut-être déjà lié à un monde ou le lien est invalide.";
        }

        IsJoining = false;
    }
}
