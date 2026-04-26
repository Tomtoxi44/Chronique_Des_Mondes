// -----------------------------------------------------------------------
// <copyright file="CombatPlayer.razor.cs" company="ANGIBAUD Tommy">
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

public partial class CombatPlayer : IAsyncDisposable
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

    private List<CombatParticipantDto> SortedParticipants =>
        Combat?.Participants.Where(p => p.IsActive).OrderBy(p => p.TurnOrder).ToList() ?? new();

    private CombatParticipantDto? ActiveParticipant =>
        SortedParticipants.FirstOrDefault(p => p.TurnOrder == Combat?.CurrentTurnOrder);

    private CombatParticipantDto? MyParticipant =>
        Combat?.Participants.FirstOrDefault(p => p.UserId == CurrentUserId);

    private bool IsMyTurn => ActiveParticipant?.UserId == CurrentUserId;

    private List<CombatActionDto> VisibleActionsForPlayer =>
        Combat?.Actions.Where(a => !a.IsPrivate).OrderBy(a => a.CreatedAt).ToList() ?? new();

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
        if (Combat == null) ErrorMessage = "Combat introuvable.";
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
        _hub.On<object>("CombatEnded", _ => Nav.NavigateTo($"/sessions/{SessionId}/player"));

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
        await InvokeAsync(StateHasChanged);
    }

    private async Task PassTurn()
    {
        if (MyParticipant == null) return;
        await CombatClient.RecordActionAsync(CombatId, new CreateCombatActionDto
        {
            ParticipantName = MyParticipant.Name,
            ActionType = "pass",
            Description = $"{MyParticipant.Name} passe son tour.",
            IsPrivate = false
        });
        await CombatClient.NextTurnAsync(CombatId);
        await ReloadAsync();
    }

    private async Task RecordMyAction(CreateCombatActionDto action)
    {
        await CombatClient.RecordActionAsync(CombatId, action);
        await ReloadAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub != null) await _hub.DisposeAsync();
    }
}
