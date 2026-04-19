using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.State;
using Cdm.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Text.Json;

namespace Cdm.Web.Components.Pages.Sessions;

public partial class SessionGm
{
    [Parameter] public int SessionId { get; set; }

    [Inject] private SessionApiClient SessionClient { get; set; } = default!;
    [Inject] private ChapterApiClient ChapterClient { get; set; } = default!;
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private NpcApiClient NpcClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

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

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
            CurrentUserId = uid;

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

        // Redirect if not the GM
        if (Session.StartedById != CurrentUserId)
        {
            Nav.NavigateTo($"/sessions/{SessionId}/player");
            return;
        }

        // Load chapters for the campaign
        Chapters = await ChapterClient.GetChaptersByCampaignAsync(Session.CampaignId);

        // Load world character sheets for the GM
        WorldCharacters = await WorldClient.GetWorldCharactersTypedAsync(Session.WorldId);

        // Pre-select current chapter if set
        if (Session.CurrentChapterId.HasValue)
        {
            SelectedChapter = Chapters.FirstOrDefault(c => c.Id == Session.CurrentChapterId.Value);
            if (SelectedChapter != null)
                ChapterNpcs = await NpcClient.GetNpcsByChapterAsync(SelectedChapter.Id);
        }

        NavContext.ClearContext();
        IsLoading = false;
    }

    private async Task SelectChapter(ChapterDto chapter)
    {
        SelectedChapter = chapter;
        ChapterNpcs = await NpcClient.GetNpcsByChapterAsync(chapter.Id);
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

    private void TogglePlayerSheet(SessionParticipantDto participant)
    {
        var sheet = WorldCharacters.FirstOrDefault(wc => wc.Id == participant.WorldCharacterId);
        SelectedPlayerSheet = SelectedPlayerSheet?.Id == sheet?.Id ? null : sheet;
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
}
