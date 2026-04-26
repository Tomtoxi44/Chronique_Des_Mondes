using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.State;
using Cdm.Web.Services;
using Cdm.Web.Services.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Text.Json;

namespace Cdm.Web.Components.Pages.Sessions;

public partial class SessionPlayer : IAsyncDisposable
{
    [Parameter] public int SessionId { get; set; }

    [Inject] private SessionApiClient SessionClient { get; set; } = default!;
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private CombatApiClient CombatClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;

    private SessionDto? Session;
    private WorldCharacterDto? MyCharacter;
    private SessionParticipantDto? MyParticipant;
    private CombatDto? ActiveCombat;
    private bool IsLoading = true;
    private bool IsLeaving = false;
    private string? ErrorMessage;
    private int CurrentUserId;
    private string? CurrentUserName;

    private string ActiveTab = "character";
    private Dictionary<string, string> GameSpecificFields { get; set; } = new();
    private List<InventoryItem> Inventory { get; set; } = new();
    private string NewItemName = "";

    // SignalR
    private HubConnection? _hub;
    private bool IsHubConnected => _hub?.State == HubConnectionState.Connected;
    private List<ChatEntry> ChatEntries { get; set; } = new();
    private string ChatInput = "";
    private int? RollingDie;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
            CurrentUserId = uid;
        CurrentUserName = user.Identity?.Name ?? "Joueur";

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        Session = await SessionClient.GetSessionAsync(SessionId);

        if (Session == null)
        {
            ErrorMessage = "Session introuvable ou accès refusé.";
            IsLoading = false;
            return;
        }

        if (Session.StartedById == CurrentUserId)
        {
            Nav.NavigateTo($"/sessions/{SessionId}/gm");
            return;
        }

        MyParticipant = Session.Participants.FirstOrDefault(p => p.UserId == CurrentUserId);
        MyCharacter = await WorldClient.GetMyWorldCharacterAsync(Session.WorldId);
        ParseGameSpecificData();

        // Auto-join si status == Invited ou Left (rejoin après avoir quitté)
        if ((MyParticipant?.Status == SessionParticipantStatus.Invited
             || MyParticipant?.Status == SessionParticipantStatus.Left) && MyCharacter != null)
        {
            var joined = await SessionClient.JoinSessionAsync(Session.Id, MyCharacter.Id);
            if (joined != null)
            {
                Session = joined;
                MyParticipant = Session.Participants.FirstOrDefault(p => p.UserId == CurrentUserId);
            }
        }

        NavContext.ClearContext();
        IsLoading = false;

        ActiveCombat = await CombatClient.GetActiveCombatForSessionAsync(SessionId);

        await ConnectToHubAsync();
    }

    private async Task ConnectToHubAsync()
    {
        if (Session == null) return;
        var token = await LocalStorage.GetItemAsync("auth_token");
        if (string.IsNullOrEmpty(token)) return;

        var baseUrl = SessionClient.GetApiBaseUrl();
        _hub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/session", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _hub.On<HubMessageDto>("ReceiveMessage", msg =>
        {
            ChatEntries.Add(new ChatEntry("text", msg.UserName ?? "?", msg.Message, null, null, msg.Timestamp));
            InvokeAsync(StateHasChanged);
        });

        _hub.On<HubDiceDto>("DiceRolled", dto =>
        {
            ChatEntries.Add(new ChatEntry("dice", dto.UserName ?? "?", null, dto.DiceType, dto.Results, dto.Timestamp));
            InvokeAsync(StateHasChanged);
        });

        _hub.On<object>("SessionEnded", _ =>
        {
            InvokeAsync(() =>
            {
                Nav.NavigateTo($"/worlds/{Session!.WorldId}");
                return Task.CompletedTask;
            });
        });

        try
        {
            await _hub.StartAsync();
            await _hub.InvokeAsync("JoinSession", SessionId);
        }
        catch { /* hub optionnel : la session fonctionne sans */ }
    }

    private async Task SendMessage()
    {
        if (_hub == null || !IsHubConnected || string.IsNullOrWhiteSpace(ChatInput)) return;
        var msg = ChatInput.Trim();
        ChatInput = "";
        try { await _hub.InvokeAsync("SendMessage", SessionId, msg); }
        catch { }
    }

    private async Task RollDice(int faces)
    {
        if (_hub == null || !IsHubConnected || RollingDie.HasValue) return;
        RollingDie = faces;
        StateHasChanged();
        await Task.Delay(400);
        var result = Random.Shared.Next(1, faces + 1);
        try { await _hub.InvokeAsync("RollDice", SessionId, $"D{faces}", 1, new[] { result }, 0, (string?)null); }
        catch
        {
            ChatEntries.Add(new ChatEntry("dice", CurrentUserName ?? "Joueur", null, $"D{faces}", new[] { result }, DateTime.UtcNow));
        }
        RollingDie = null;
        StateHasChanged();
    }

    private async Task LeaveSession()
    {
        if (Session == null) return;
        IsLeaving = true;
        var ok = await SessionClient.LeaveSessionAsync(Session.Id);
        IsLeaving = false;
        if (ok) Nav.NavigateTo($"/worlds/{Session.WorldId}");
    }

    private void ParseGameSpecificData()
    {
        GameSpecificFields = new();
        Inventory = new();
        if (string.IsNullOrWhiteSpace(MyCharacter?.GameSpecificData)) return;
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(MyCharacter.GameSpecificData);
            if (dict == null) return;
            foreach (var kv in dict)
            {
                if (kv.Key == "inventory" && kv.Value.ValueKind == JsonValueKind.Array)
                    Inventory = kv.Value.Deserialize<List<InventoryItem>>() ?? new();
                else
                    GameSpecificFields[kv.Key] = kv.Value.ToString();
            }
        }
        catch { }
    }

    private async Task AddInventoryItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemName) || MyCharacter == null) return;
        Inventory.Add(new InventoryItem { Name = NewItemName.Trim(), Qty = 1 });
        NewItemName = "";
        await SaveInventory();
    }

    private async Task RemoveInventoryItem(InventoryItem item)
    {
        Inventory.Remove(item);
        await SaveInventory();
    }

    private async Task SaveInventory()
    {
        if (MyCharacter == null) return;
        var dict = new Dictionary<string, object>(GameSpecificFields.ToDictionary(kv => kv.Key, kv => (object)kv.Value));
        dict["inventory"] = Inventory;
        var json = JsonSerializer.Serialize(dict);
        var request = new UpdateWorldCharacterProfileRequest(MyCharacter.Level, MyCharacter.CurrentHealth, MyCharacter.MaxHealth, json);
        var result = await WorldClient.UpdateMyWorldCharacterAsync(Session!.WorldId, request);
        if (result != null) MyCharacter = result;
    }

    private static string GetStatusLabel(SessionStatus status) => status switch
    {
        SessionStatus.Active => "En cours",
        SessionStatus.Paused => "En pause",
        SessionStatus.Ended => "Terminée",
        _ => "Planifiée"
    };

    private static string GetStatusClass(SessionStatus status) => status switch
    {
        SessionStatus.Active => "status-active",
        SessionStatus.Paused => "status-pending",
        SessionStatus.Ended => "status-completed",
        _ => "status-draft"
    };

    public async ValueTask DisposeAsync()
    {
        if (_hub != null)
        {
            await _hub.DisposeAsync();
        }
    }

    private record InventoryItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("qty")]
        public int Qty { get; set; } = 1;
    }

    private record ChatEntry(string Type, string UserName, string? Text, string? DiceType, int[]? Results, DateTime Timestamp);
    private record HubMessageDto(int UserId, string? UserName, string? Message, DateTime Timestamp);
    private record HubDiceDto(int UserId, string? UserName, string? DiceType, int Count, int[]? Results, int Modifier, int Total, string? Reason, DateTime Timestamp);
}

