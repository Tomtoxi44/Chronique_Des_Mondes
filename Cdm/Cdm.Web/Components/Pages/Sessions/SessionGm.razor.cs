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
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace Cdm.Web.Components.Pages.Sessions;

public partial class SessionGm : IAsyncDisposable
{
    [Parameter] public int SessionId { get; set; }

    [Inject] private SessionApiClient SessionClient { get; set; } = default!;
    [Inject] private ChapterApiClient ChapterClient { get; set; } = default!;
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private NpcApiClient NpcClient { get; set; } = default!;
    [Inject] private CombatApiClient CombatClient { get; set; } = default!;
    [Inject] private LootApiClient LootClient { get; set; } = default!;
    [Inject] private InventoryApiClient InventoryClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ILogger<SessionGm> Logger { get; set; } = default!;

    private SessionDto? Session;
    private List<ChapterDto> Chapters = new();
    private ChapterDto? SelectedChapter;
    private List<WorldCharacterDto> WorldCharacters = new();
    private WorldCharacterDto? SelectedPlayerSheet;
    private List<NpcDto> ChapterNpcs = new();
    private bool IsLoading = true;
    private bool IsEnding = false;
    private string? ErrorMessage;
    private int CurrentUserId;
    private string? CurrentUserName;

    private DotNetObjectReference<SessionGm>? _dotNetRef;
    private bool _needsPreviewClickInit = false;

    // SignalR
    private HubConnection? _hub;
    private bool IsHubConnected => _hub?.State == HubConnectionState.Connected;
    private List<ChatEntry> ChatEntries { get; set; } = new();
    private string ChatInput = "";
    private int? RollingDie;

    // Trades (object exchanges)
    private List<SessionTradeDto> PendingTrades { get; set; } = new();
    private int TradeTargetUserId;
    private string TradeOffer = "";
    private string TradeRequest = "";
    private bool IsProposingTrade;

    // Loot distribution
    private List<CampaignLootDto> CampaignLoot { get; set; } = new();
    private int DistributeLootId;
    private int DistributeTargetWcId;
    private bool IsDistributingLoot;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
            CurrentUserId = uid;
        CurrentUserName = user.Identity?.Name ?? "MJ";

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

        if (Session.StartedById != CurrentUserId)
        {
            Nav.NavigateTo($"/sessions/{SessionId}/player");
            return;
        }

        Chapters = await ChapterClient.GetChaptersByCampaignAsync(Session.CampaignId);
        WorldCharacters = await WorldClient.GetWorldCharactersTypedAsync(Session.WorldId);
        CampaignLoot = await LootClient.GetCampaignLootAsync(Session.CampaignId);

        if (Session.CurrentChapterId.HasValue)
        {
            SelectedChapter = Chapters.FirstOrDefault(c => c.Id == Session.CurrentChapterId.Value);
            if (SelectedChapter != null)
                ChapterNpcs = await NpcClient.GetNpcsByChapterAsync(SelectedChapter.Id);
        }

        NavContext.ClearContext();
        IsLoading = false;

        // Rebuild the timeline from persisted history BEFORE connecting to the hub,
        // so live events only ever append to an already-complete history (no duplicates).
        await LoadHistoryAsync();
        await LoadTradesAsync();

        await ConnectToHubAsync();
    }

    private async Task LoadTradesAsync()
    {
        PendingTrades = await SessionClient.GetPendingTradesAsync(SessionId);
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

        _hub.On<SessionTradeDto>("TradeProposed", trade =>
        {
            PendingTrades.RemoveAll(t => t.Id == trade.Id);
            PendingTrades.Add(trade);
            InvokeAsync(StateHasChanged);
        });

        _hub.On<SessionTradeDto>("TradeResolved", trade =>
        {
            PendingTrades.RemoveAll(t => t.Id == trade.Id);
            InvokeAsync(StateHasChanged);
        });

        _hub.On<LootDistributionResultDto>("LootReceived", loot =>
        {
            var qty = loot.Quantity > 1 ? $" ×{loot.Quantity}" : string.Empty;
            ChatEntries.Add(new ChatEntry("text", "🎁 Butin", $"{loot.RecipientName} reçoit {loot.LootName}{qty}.", null, null, DateTime.UtcNow));
            InvokeAsync(StateHasChanged);
        });

        try
        {
            await _hub.StartAsync();
            await _hub.InvokeAsync("JoinSession", SessionId);
        }
        catch (Exception ex)
        {
            // Ne jamais échouer en silence : sans le hub, le chat et les dés sont muets.
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

    // ── Image poussée aux joueurs ──────────────────────────────────────────
    private string? UploadedImageUrl;   // dernière image envoyée, prête à montrer
    private string? PushedImageUrl;      // image actuellement affichée aux joueurs

    private void OnSessionImageUploaded(string url)
    {
        UploadedImageUrl = url;
    }

    private async Task PushImageToPlayers()
    {
        if (_hub == null || !IsHubConnected || string.IsNullOrEmpty(UploadedImageUrl)) return;
        try
        {
            await _hub.InvokeAsync("ShowImage", SessionId, UploadedImageUrl);
            PushedImageUrl = UploadedImageUrl;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Impossible de montrer l'image (session {SessionId})", SessionId);
            Toast.ShowError("Image non transmise. Vérifiez votre connexion.", "Session");
        }
    }

    private async Task HidePushedImage()
    {
        if (_hub == null || !IsHubConnected) return;
        try
        {
            await _hub.InvokeAsync("HideImage", SessionId);
            PushedImageUrl = null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Impossible de masquer l'image (session {SessionId})", SessionId);
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
            // Repli local : le MJ voit son jet, mais il faut dire qu'il n'a pas été partagé.
            ChatEntries.Add(new ChatEntry("dice", CurrentUserName ?? "MJ", null, $"D{faces}", new[] { result }, DateTime.UtcNow));
            Toast.ShowWarning("Jet effectué localement : il n'a pas été partagé aux joueurs.", "Dés");
        }
        RollingDie = null;
        StateHasChanged();
    }

    /// <summary>
    /// Selectable trade targets: every joined participant (the GM is the current user here).
    /// </summary>
    private List<(int UserId, string Name)> TradeTargets()
    {
        var targets = new List<(int, string)>();
        if (Session == null) return targets;

        foreach (var p in Session.Participants)
        {
            if (p.UserId != CurrentUserId && p.UserId > 0)
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

    /// <summary>Selectable loot recipients: every session participant's character.</summary>
    private List<(int WorldCharacterId, string Name)> LootTargets()
    {
        var targets = new List<(int, string)>();
        if (Session == null) return targets;

        foreach (var p in Session.Participants)
        {
            if (p.WorldCharacterId > 0)
            {
                targets.Add((p.WorldCharacterId, string.IsNullOrWhiteSpace(p.CharacterName) ? p.UserName : p.CharacterName));
            }
        }
        return targets;
    }

    private string LootScopeLabel(int? chapterId)
    {
        if (!chapterId.HasValue) return "Campagne";
        var ch = Chapters.FirstOrDefault(c => c.Id == chapterId.Value);
        return ch != null ? $"Ch. {ch.ChapterNumber}" : "Chapitre";
    }

    private async Task DistributeLoot()
    {
        if (_hub == null || !IsHubConnected || IsDistributingLoot) return;
        if (DistributeLootId <= 0 || DistributeTargetWcId <= 0) return;

        IsDistributingLoot = true;
        try
        {
            await _hub.InvokeAsync("DistributeLoot", SessionId, DistributeLootId, DistributeTargetWcId);
            DistributeLootId = 0;
            DistributeTargetWcId = 0;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Distribution de butin impossible (session {SessionId})", SessionId);
            Toast.ShowError("Butin non distribué. Vérifiez votre connexion.", "Butin");
        }
        IsDistributingLoot = false;
    }

    private async Task SelectChapter(ChapterDto chapter)
    {
        SelectedChapter = chapter;
        ChapterNpcs = await NpcClient.GetNpcsByChapterAsync(chapter.Id);
        _needsPreviewClickInit = true;
        if (Session != null)
        {
            var updated = await SessionClient.UpdateCurrentChapterAsync(Session.Id, chapter.Id);
            if (updated != null) Session = updated;
        }
    }

    private async Task NavigateChapter(int delta)
    {
        if (Session == null || Chapters.Count == 0) return;
        var ordered = Chapters.OrderBy(c => c.ChapterNumber).ToList();
        int currentIndex = SelectedChapter != null
            ? ordered.IndexOf(ordered.FirstOrDefault(c => c.Id == SelectedChapter.Id)!)
            : -1;
        int nextIndex = currentIndex + delta;
        if (nextIndex < 0 || nextIndex >= ordered.Count) return;
        await SelectChapter(ordered[nextIndex]);
    }

    private async Task EndSession()
    {
        if (Session == null) return;
        IsEnding = true;
        var success = await SessionClient.EndSessionAsync(Session.Id);
        if (success)
            Nav.NavigateTo($"/worlds/{Session.WorldId}");
        else
        {
            ErrorMessage = "Impossible de terminer la session.";
            IsEnding = false;
        }
    }

    private async Task TriggerCombat()
    {
        var activeCombat = await CombatClient.GetActiveCombatForSessionAsync(SessionId);
        if (activeCombat != null)
            Nav.NavigateTo($"/sessions/{SessionId}/combat/{activeCombat.Id}/gm");
        else
            Nav.NavigateTo($"/sessions/{SessionId}/combat/setup");
    }

    private List<InventoryItemDto> SelectedPlayerInventory { get; set; } = new();

    private async Task TogglePlayerSheet(SessionParticipantDto participant)
    {
        var sheet = WorldCharacters.FirstOrDefault(wc => wc.Id == participant.WorldCharacterId);
        if (SelectedPlayerSheet?.Id == sheet?.Id)
        {
            SelectedPlayerSheet = null;
            SelectedPlayerInventory = new();
            return;
        }

        SelectedPlayerSheet = sheet;
        SelectedPlayerInventory = sheet != null
            ? await InventoryClient.GetForCharacterAsGmAsync(sheet.Id)
            : new();
    }

    private MarkupString RenderChapterContent(string text)
    {
        if (string.IsNullOrEmpty(text)) return new MarkupString(string.Empty);
        var npcMap = ChapterNpcs.ToDictionary(n => n.Id, n => n);
        var pcMap = WorldCharacters.ToDictionary(c => c.CharacterId, c => c);
        var escaped = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        var rendered = System.Text.RegularExpressions.Regex.Replace(
            escaped,
            @"@\[([^\]]+)\]\(npc:(\d+)\)",
            m =>
            {
                var name = m.Groups[1].Value;
                var id = int.TryParse(m.Groups[2].Value, out var i) ? i : 0;
                var tooltip = id > 0 && npcMap.TryGetValue(id, out var npc)
                    ? (npc.Description ?? npc.PhysicalDescription ?? string.Empty)
                    : string.Empty;
                var tip = string.IsNullOrEmpty(tooltip) ? "" : $" data-tooltip=\"{tooltip.Replace("\"", "&quot;")}\"";
                return $"<span class=\"npc-mention\" data-mention-type=\"npc\" data-mention-id=\"{id}\"{tip}>@{name}</span>";
            });

        rendered = System.Text.RegularExpressions.Regex.Replace(
            rendered,
            @"@\[([^\]]+)\]\(pc:(\d+)\)",
            m =>
            {
                var name = m.Groups[1].Value;
                var id = int.TryParse(m.Groups[2].Value, out var i) ? i : 0;
                var tooltip = id > 0 && pcMap.TryGetValue(id, out var pc)
                    ? $"Personnage joueur{(pc.Level.HasValue ? $" — Niveau {pc.Level}" : string.Empty)}"
                    : "Personnage joueur";
                var tip = $" data-tooltip=\"{tooltip}\"";
                return $"<span class=\"npc-mention pc-mention\" data-mention-type=\"pc\" data-mention-id=\"{id}\"{tip}>@{name}</span>";
            });

        rendered = rendered.Replace("\r\n", "\n").Replace("\n", "<br />");
        return new MarkupString(rendered);
    }

    [JSInvokable]
    public void OpenMentionDetail(string type, int id)
    {
        if (type == "npc")
        {
            var npc = ChapterNpcs.FirstOrDefault(n => n.Id == id);
            if (npc != null)
                _ = JS.InvokeVoidAsync("document.getElementById", $"gm-npc-{id}");
        }
        else if (type == "pc")
        {
            Nav.NavigateTo($"/characters/{id}");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender || _needsPreviewClickInit)
        {
            _needsPreviewClickInit = false;
            _dotNetRef ??= DotNetObjectReference.Create(this);
            try
            {
                var module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/mention.js");
                await module.InvokeVoidAsync("initPreviewClicks", "session-chapter-preview-content", _dotNetRef);
            }
            catch { }
        }
    }

    private Dictionary<string, string> ParseGameSpecificData(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            return dict?.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()) ?? new();
        }
        catch { return new(); }
    }

    private static string GetParticipantStatusClass(SessionParticipantStatus status) => status switch
    {
        SessionParticipantStatus.Joined => "status-active",
        SessionParticipantStatus.Invited => "status-pending",
        _ => "status-draft"
    };

    private static string GetParticipantStatusLabel(SessionParticipantStatus status) => status switch
    {
        SessionParticipantStatus.Joined => "Connecté",
        SessionParticipantStatus.Invited => "Invité",
        _ => "Déconnecté"
    };

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        if (_hub != null)
            await _hub.DisposeAsync();
    }

    private record ChatEntry(string Type, string UserName, string? Text, string? DiceType, int[]? Results, DateTime Timestamp, string? Reason = null, int Modifier = 0);
    private record HubMessageDto(int UserId, string? UserName, string? Message, DateTime Timestamp);
    private record HubDiceDto(int UserId, string? UserName, string? DiceType, int Count, int[]? Results, int Modifier, int Total, string? Reason, DateTime Timestamp);
}
