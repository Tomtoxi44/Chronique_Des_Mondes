using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.DTOs.DnD5e;
using Cdm.Common;
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
    [Inject] private DndApiClient DndClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ILogger<SessionPlayer> Logger { get; set; } = default!;

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

    // Image forcée par le MJ (overlay non fermable tant que le MJ ne la retire pas).
    private string? ForcedImageUrl;
    private string ChatInput = "";
    private int? RollingDie;

    // D&D 5e "roll from sheet" support
    private DndCharacterStatsDto? DndStats;
    private List<DndInventoryItemDto> Weapons { get; set; } = new();
    private List<DndCharacterSpellDto> DamageSpells { get; set; } = new();
    private bool IsDndCharacter => MyCharacter?.GameType == Cdm.Common.Enums.GameType.DnD5e;

    // Editable custom roll (pre-filled when a weapon/spell is selected)
    private int CustomCount = 1;
    private int CustomFaces = 20;
    private int CustomModifier;
    private string? CustomReason;
    private bool IsRollingCustom;

    // Trades (object exchanges)
    private List<SessionTradeDto> PendingTrades { get; set; } = new();
    private int TradeTargetUserId;
    private string TradeOffer = "";
    private string TradeRequest = "";
    private bool IsProposingTrade;

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

        // Rebuild the timeline from persisted history BEFORE connecting to the hub,
        // so live events only ever append to an already-complete history (no duplicates).
        await LoadHistoryAsync();
        await LoadTradesAsync();
        await LoadDndSheetAsync();

        await ConnectToHubAsync();
    }

    private async Task LoadTradesAsync()
    {
        PendingTrades = await SessionClient.GetPendingTradesAsync(SessionId);
    }

    /// <summary>
    /// Loads the D&D weapons and damage spells of the player's character so rolls can be
    /// pre-filled from the sheet. No-op for non-D&D characters.
    /// </summary>
    private async Task LoadDndSheetAsync()
    {
        if (!IsDndCharacter || MyCharacter == null)
        {
            return;
        }

        DndStats = await DndClient.GetCharacterStatsAsync(MyCharacter.Id);

        var inventory = await DndClient.GetInventoryAsync(MyCharacter.Id);
        Weapons = inventory.Where(i => !string.IsNullOrWhiteSpace(i.DamageDice)).ToList();

        var spells = await DndClient.GetCharacterSpellsAsync(MyCharacter.Id);
        DamageSpells = spells.Where(s => !string.IsNullOrWhiteSpace(s.DamageDice)).ToList();
    }

    private async Task LoadHistoryAsync()
    {
        var history = await SessionClient.GetSessionHistoryAsync(SessionId);
        if (history == null) return;

        var entries = new List<ChatEntry>(history.Messages.Count + history.DiceRolls.Count);
        foreach (var m in history.Messages)
            entries.Add(new ChatEntry("text", m.UserName, m.Message, null, null, m.SentAt));
        foreach (var d in history.DiceRolls)
            entries.Add(new ChatEntry("dice", d.UserName, null, d.DiceType, d.Results, d.RolledAt, d.Reason, d.Modifier));

        ChatEntries = entries.OrderBy(e => e.Timestamp).ToList();
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
            ChatEntries.Add(new ChatEntry("dice", dto.UserName ?? "?", null, dto.DiceType, dto.Results, dto.Timestamp, dto.Reason, dto.Modifier));
            InvokeAsync(StateHasChanged);
        });

        _hub.On<string>("ShowImage", url =>
        {
            ForcedImageUrl = url;
            InvokeAsync(StateHasChanged);
        });

        _hub.On("HideImage", () =>
        {
            ForcedImageUrl = null;
            InvokeAsync(StateHasChanged);
        });

        _hub.On<SessionTradeDto>("TradeProposed", trade =>
        {
            PendingTrades.RemoveAll(t => t.Id == trade.Id);
            PendingTrades.Add(trade);
            InvokeAsync(StateHasChanged);
        });

        _hub.On<SessionTradeDto>("TradeResolved", trade =>
        {
            // Once accepted, declined or cancelled, the trade leaves the pending list.
            PendingTrades.RemoveAll(t => t.Id == trade.Id);
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
        catch (Exception ex)
        {
            // Ne jamais échouer en silence : sans le hub, le chat et les dés partagés sont muets.
            Logger.LogWarning(ex, "Connexion au hub de session impossible (session {SessionId})", SessionId);
            Toast.ShowError(
                "Temps réel indisponible : le chat et les dés partagés ne fonctionneront pas. Rechargez la page pour réessayer.",
                "Connexion perdue");
        }
    }

    private async Task SendMessage()
    {
        if (_hub == null || !IsHubConnected || string.IsNullOrWhiteSpace(ChatInput)) return;
        var msg = ChatInput.Trim();
        ChatInput = "";
        try
        {
            await _hub.InvokeAsync("SendMessage", SessionId, msg);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Envoi du message impossible (session {SessionId})", SessionId);
            ChatInput = msg; // on rend son texte à l'utilisateur plutôt que de le perdre
            Toast.ShowError("Message non envoyé. Vérifiez votre connexion.", "Chat");
        }
    }

    private async Task RollDice(int faces)
    {
        if (_hub == null || !IsHubConnected || RollingDie.HasValue) return;
        RollingDie = faces;
        StateHasChanged();
        await Task.Delay(400);
        var result = Random.Shared.Next(1, faces + 1);
        try
        {
            await _hub.InvokeAsync("RollDice", SessionId, $"D{faces}", 1, new[] { result }, 0, (string?)null);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Partage du jet de dé impossible (session {SessionId})", SessionId);
            // Repli local : le joueur voit son jet, mais il faut dire qu'il n'a pas été partagé.
            ChatEntries.Add(new ChatEntry("dice", CurrentUserName ?? "Joueur", null, $"D{faces}", new[] { result }, DateTime.UtcNow));
            Toast.ShowWarning("Jet effectué localement : il n'a pas été partagé à la table.", "Dés");
        }
        RollingDie = null;
        StateHasChanged();
    }

    /// <summary>
    /// Pre-fills the custom roll from a weapon's damage dice + Strength modifier (melee default).
    /// </summary>
    private void OnWeaponSelected(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var id) || id <= 0)
        {
            return;
        }

        var weapon = Weapons.FirstOrDefault(w => w.Id == id);
        if (weapon == null || !DiceNotation.TryParse(weapon.DamageDice, out var expr))
        {
            return;
        }

        CustomCount = expr.Count;
        CustomFaces = expr.Faces;
        // Damage bonus = flat bonus in the notation + the character's Strength modifier.
        CustomModifier = expr.FlatBonus + (DndStats?.StrengthModifier ?? 0);
        CustomReason = $"{weapon.Name} (dégâts)";
    }

    /// <summary>
    /// Pre-fills the custom roll from a spell's damage dice.
    /// </summary>
    private void OnSpellSelected(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var id) || id <= 0)
        {
            return;
        }

        var spell = DamageSpells.FirstOrDefault(s => s.Id == id);
        if (spell == null || !DiceNotation.TryParse(spell.DamageDice, out var expr))
        {
            return;
        }

        CustomCount = expr.Count;
        CustomFaces = expr.Faces;
        CustomModifier = expr.FlatBonus;
        CustomReason = $"{spell.Name} (dégâts)";
    }

    /// <summary>
    /// Rolls the editable custom expression (count dice of the chosen faces + modifier)
    /// and shares it with its reason.
    /// </summary>
    private async Task RollCustom()
    {
        if (_hub == null || !IsHubConnected || IsRollingCustom)
        {
            return;
        }

        var count = Math.Clamp(CustomCount, 1, 20);
        var faces = Math.Clamp(CustomFaces, 2, 100);

        IsRollingCustom = true;
        StateHasChanged();

        var results = new int[count];
        for (var i = 0; i < count; i++)
        {
            results[i] = Random.Shared.Next(1, faces + 1);
        }

        try
        {
            await _hub.InvokeAsync("RollDice", SessionId, $"D{faces}", count, results, CustomModifier, CustomReason);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Partage du jet personnalisé impossible (session {SessionId})", SessionId);
            ChatEntries.Add(new ChatEntry("dice", CurrentUserName ?? "Joueur", null, $"D{faces}", results, DateTime.UtcNow, CustomReason, CustomModifier));
            Toast.ShowWarning("Jet effectué localement : il n'a pas été partagé à la table.", "Dés");
        }

        IsRollingCustom = false;
        StateHasChanged();
    }

    /// <summary>
    /// Selectable trade targets: the GM and every joined participant, excluding oneself.
    /// </summary>
    private List<(int UserId, string Name)> TradeTargets()
    {
        var targets = new List<(int, string)>();
        if (Session == null) return targets;

        if (Session.StartedById != CurrentUserId)
            targets.Add((Session.StartedById, $"{Session.StartedByName} (MJ)"));

        foreach (var p in Session.Participants)
        {
            if (p.UserId != CurrentUserId && p.UserId != Session.StartedById && p.UserId > 0)
                targets.Add((p.UserId, p.UserName));
        }
        return targets;
    }

    private async Task ProposeTrade()
    {
        if (_hub == null || !IsHubConnected || IsProposingTrade) return;
        if (TradeTargetUserId <= 0 || string.IsNullOrWhiteSpace(TradeOffer) || string.IsNullOrWhiteSpace(TradeRequest))
        {
            Toast.ShowWarning("Choisissez un destinataire et décrivez l'offre et la demande.", "Échange");
            return;
        }

        IsProposingTrade = true;
        try
        {
            await _hub.InvokeAsync("ProposeTrade", SessionId, TradeTargetUserId, TradeOffer.Trim(), TradeRequest.Trim());
            TradeOffer = "";
            TradeRequest = "";
            TradeTargetUserId = 0;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Proposition d'échange impossible (session {SessionId})", SessionId);
            Toast.ShowError("Proposition d'échange non envoyée.", "Échange");
        }
        IsProposingTrade = false;
    }

    private async Task RespondToTrade(int tradeId, bool accept)
    {
        if (_hub == null || !IsHubConnected) return;
        try
        {
            await _hub.InvokeAsync("RespondToTrade", SessionId, tradeId, accept);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Réponse à l'échange impossible (session {SessionId})", SessionId);
            Toast.ShowError("Réponse non envoyée.", "Échange");
        }
    }

    private async Task CancelTrade(int tradeId)
    {
        if (_hub == null || !IsHubConnected) return;
        try
        {
            await _hub.InvokeAsync("CancelTrade", SessionId, tradeId);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Annulation de l'échange impossible (session {SessionId})", SessionId);
            Toast.ShowError("Annulation non envoyée.", "Échange");
        }
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

    private record ChatEntry(string Type, string UserName, string? Text, string? DiceType, int[]? Results, DateTime Timestamp, string? Reason = null, int Modifier = 0);
    private record HubMessageDto(int UserId, string? UserName, string? Message, DateTime Timestamp);
    private record HubDiceDto(int UserId, string? UserName, string? DiceType, int Count, int[]? Results, int Modifier, int Total, string? Reason, DateTime Timestamp);
}

