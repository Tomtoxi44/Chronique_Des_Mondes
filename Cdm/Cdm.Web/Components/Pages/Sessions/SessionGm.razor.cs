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
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;

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

        if (Session.CurrentChapterId.HasValue)
        {
            SelectedChapter = Chapters.FirstOrDefault(c => c.Id == Session.CurrentChapterId.Value);
            if (SelectedChapter != null)
                ChapterNpcs = await NpcClient.GetNpcsByChapterAsync(SelectedChapter.Id);
        }

        NavContext.ClearContext();
        IsLoading = false;

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

        try
        {
            await _hub.StartAsync();
            await _hub.InvokeAsync("JoinSession", SessionId);
        }
        catch { }
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
            ChatEntries.Add(new ChatEntry("dice", CurrentUserName ?? "MJ", null, $"D{faces}", new[] { result }, DateTime.UtcNow));
        }
        RollingDie = null;
        StateHasChanged();
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

    private void TogglePlayerSheet(SessionParticipantDto participant)
    {
        var sheet = WorldCharacters.FirstOrDefault(wc => wc.Id == participant.WorldCharacterId);
        SelectedPlayerSheet = SelectedPlayerSheet?.Id == sheet?.Id ? null : sheet;
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

    private record ChatEntry(string Type, string UserName, string? Text, string? DiceType, int[]? Results, DateTime Timestamp);
    private record HubMessageDto(int UserId, string? UserName, string? Message, DateTime Timestamp);
    private record HubDiceDto(int UserId, string? UserName, string? DiceType, int Count, int[]? Results, int Modifier, int Total, string? Reason, DateTime Timestamp);
}
