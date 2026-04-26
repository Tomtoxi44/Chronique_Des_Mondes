// -----------------------------------------------------------------------
// <copyright file="CombatGm.razor.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;

namespace Cdm.Web.Components.Pages.Sessions;

public partial class CombatGm : IAsyncDisposable
{
    [Parameter] public int SessionId { get; set; }
    [Parameter] public int CombatId { get; set; }

    [Inject] private CombatApiClient CombatClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;

    private bool IsLoading = true;
    private string? ErrorMessage;
    private int CurrentUserId;
    private CombatDto? Combat;
    private HubConnection? _hub;

    private int HpInput;
    private int InitiativeInput;
    private string FreeActionText = "";
    private bool ShowEndConfirm = false;
    private string? VictorySide;

    private List<CombatParticipantDto> SortedParticipants =>
        Combat?.Participants
            .Where(p => p.IsActive)
            .OrderBy(p => p.TurnOrder)
            .ToList() ?? new();

    private CombatParticipantDto? ActiveParticipant =>
        SortedParticipants.FirstOrDefault(p => p.TurnOrder == Combat?.CurrentTurnOrder);

    private List<CombatActionDto> VisibleActionsForGm =>
        Combat?.Actions.OrderBy(a => a.CreatedAt).ToList() ?? new();

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        if (int.TryParse(authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
            CurrentUserId = uid;

        await LoadAsync();
        await ConnectToHubAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        Combat = await CombatClient.GetCombatAsync(CombatId);
        if (Combat == null)
            ErrorMessage = "Combat introuvable.";
        else
            HpInput = ActiveParticipant?.CurrentHp ?? 0;
        IsLoading = false;
    }

    private async Task ConnectToHubAsync()
    {
        var token = await LocalStorage.GetItemAsync("auth_token");
        if (string.IsNullOrEmpty(token)) return;

        var baseUrl = CombatClient.GetApiBaseUrl();
        _hub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/combat", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _hub.On<object>("TurnChanged", _ => { _ = ReloadAsync(); });
        _hub.On<object>("InitiativeUpdated", _ => { _ = ReloadAsync(); });
        _hub.On<object>("DiceRolled", _ => { _ = ReloadAsync(); });
        _hub.On<object>("CombatEnded", _ => Nav.NavigateTo($"/sessions/{SessionId}/gm"));

        try
        {
            await _hub.StartAsync();
            await _hub.InvokeAsync("JoinCombat", CombatId.ToString());
        }
        catch { }
    }

    private async Task ReloadAsync()
    {
        Combat = await CombatClient.GetCombatAsync(CombatId);
        if (ActiveParticipant != null) HpInput = ActiveParticipant.CurrentHp;
        await InvokeAsync(StateHasChanged);
    }

    private async Task StartCombat()
    {
        var updated = await CombatClient.StartCombatAsync(CombatId);
        if (updated != null) Combat = updated;
    }

    private async Task NextTurn()
    {
        var updated = await CombatClient.NextTurnAsync(CombatId);
        if (updated != null) Combat = updated;
    }

    private async Task UpdateActiveHp()
    {
        if (ActiveParticipant == null) return;
        var updated = await CombatClient.UpdateHpAsync(CombatId, ActiveParticipant.Id, new UpdateHpDto { NewHp = HpInput });
        if (updated != null)
        {
            var p = Combat?.Participants.FirstOrDefault(x => x.Id == updated.Id);
            if (p != null) { p.CurrentHp = updated.CurrentHp; }
        }
    }

    private async Task SetActiveInitiative()
    {
        if (ActiveParticipant == null) return;
        await SetParticipantInitiative(ActiveParticipant, InitiativeInput.ToString());
    }

    private async Task SetParticipantInitiative(CombatParticipantDto participant, string? value)
    {
        if (!int.TryParse(value, out int initiative)) return;
        var updated = await CombatClient.SetInitiativeAsync(CombatId, participant.Id, new SetInitiativeDto { Value = initiative });
        if (updated != null) Combat = updated;
    }

    private async Task RecordActionFromDice(CreateCombatActionDto action)
    {
        await CombatClient.RecordActionAsync(CombatId, action);
        await ReloadAsync();
    }

    private async Task RecordFreeAction()
    {
        if (string.IsNullOrWhiteSpace(FreeActionText) || ActiveParticipant == null) return;
        await CombatClient.RecordActionAsync(CombatId, new CreateCombatActionDto
        {
            ParticipantName = ActiveParticipant.Name,
            ActionType = "text",
            Description = FreeActionText,
            IsPrivate = false
        });
        FreeActionText = "";
        await ReloadAsync();
    }

    private async Task ConfirmEndCombat()
    {
        ShowEndConfirm = false;
        var updated = await CombatClient.EndCombatAsync(CombatId, new EndCombatDto { VictorySide = VictorySide });
        if (updated != null) Nav.NavigateTo($"/sessions/{SessionId}/gm");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub != null) await _hub.DisposeAsync();
    }
}
