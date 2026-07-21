using Cdm.Web.Extensions;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Text.Json;

namespace Cdm.Web.Components.Pages.Worlds;

public partial class WorldDetail : IDisposable
{
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private MarketplaceApiClient MarketClient { get; set; } = default!;
    [Inject] private ChapterApiClient ChapterClient { get; set; } = default!;
    [Inject] private EventApiClient EventClient { get; set; } = default!;
    [Inject] private AchievementApiClient AchievementClient { get; set; } = default!;
    [Inject] private SessionApiClient SessionClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Parameter] public int WorldId { get; set; }

    [SupplyParameterFromQuery(Name = "section")]
    private string? Section { get; set; }

    [SupplyParameterFromQuery(Name = "campaignId")]
    private int? SelectedCampaignId { get; set; }

    [SupplyParameterFromQuery(Name = "chapterId")]
    private int? SelectedChapterId { get; set; }

    private WorldDto? World;
    private List<CampaignDto> Campaigns = new();
    private Dictionary<int, bool> ExpandedCampaigns = new();
    private Dictionary<int, List<ChapterDto>> CampaignChapters = new();
    private bool IsLoading = true;
    private bool IsSaving = false;

    private int CurrentUserId = 0;
    private bool IsOwner => World?.UserId == CurrentUserId;

    // Session launch is delegated to WorldSessionLaunchPanel; the parent keeps only a
    // lightweight flag to show the "active session" dot on the Session tab.
    private bool _campaignHasActiveSession;

    // World settings editor (inline in overview)
    private bool IsWorldEditing = false;
    private string WorldEditName = string.Empty;
    private GameType WorldEditGameType = GameType.Generic;

    // Description editing is delegated to WorldDescriptionEditor.

    // Campaign detail
    private CampaignDto? SelectedCampaign;

    private bool IsSharingWorld;
    private bool IsSharingCampaign;

    private async Task ToggleWorldShared()
    {
        if (World == null || IsSharingWorld) return;
        IsSharingWorld = true;
        var newValue = !World.IsShared;
        if (await MarketClient.SetWorldSharedAsync(World.Id, newValue))
        {
            World.IsShared = newValue;
        }
        IsSharingWorld = false;
    }

    private async Task ToggleCampaignShared()
    {
        if (SelectedCampaign == null || IsSharingCampaign) return;
        IsSharingCampaign = true;
        var newValue = !SelectedCampaign.IsShared;
        if (await MarketClient.SetCampaignSharedAsync(SelectedCampaign.Id, newValue))
        {
            SelectedCampaign.IsShared = newValue;
        }
        IsSharingCampaign = false;
    }
    private bool IsCampaignEditing = false;
    private string CampaignEditName = string.Empty;
    private string CampaignEditDescription = string.Empty;

    // Chapter list + drag-and-drop reorder is delegated to WorldChapterListPanel.
    // Campaign creation is delegated to WorldNewCampaignForm.

    // Chapter detail
    private ChapterDto? SelectedChapter;
    // Invite tokens (campaign-level; world-level invite lives in WorldInvitePanel)
    private bool IsGeneratingToken = false;

    // World characters (players)
    private List<WorldCharacterDto> WorldCharacters = new();
    private bool IsLoadingWorldCharacters = false;

    // Events
    // (Événements du monde : gérés par WorldEventsPanel)

    // (Succès du monde : gérés par WorldAchievementsPanel)

    private static readonly List<(GameType Value, string Label, string Icon)> GameTypeOptions = new()
    {
        (GameType.Generic,       "Générique",          "bi-globe2"),
        (GameType.DnD5e,         "D&D 5e",             "bi-shield-fill"),
        (GameType.Pathfinder,    "Pathfinder",         "bi-shield-fill-check"),
        (GameType.CallOfCthulhu, "L'Appel de Cthulhu", "bi-eye-fill"),
        (GameType.Warhammer,     "Warhammer",          "bi-hammer"),
        (GameType.Cyberpunk,     "Cyberpunk",          "bi-cpu-fill"),
        (GameType.Skyrim,        "Skyrim",             "bi-snow2"),
        (GameType.Custom,        "Personnalisé",       "bi-stars"),
    };

    /// <summary>Applies the world returned by <c>WorldDescriptionEditor</c> after a save.</summary>
    private void OnWorldDescriptionSaved(WorldDto updated)
    {
        World = updated;
        SetSecondaryNav();
    }

    private AppConfirmDialog DeleteWorldDialog { get; set; } = default!;
    private AppConfirmDialog DeleteCampaignDialog { get; set; } = default!;
    private CampaignDto? CampaignToDelete;

    // Campaign tabs
    private string CampaignTab = "chapitres";
    private int? _lastCampaignIdForTab;

    // Chapter workspace (tabs, content draft, NPCs, @mention interop) is delegated
    // to WorldChapterEditorPanel.

    private bool IsWorldDnD5e => World?.GameType == GameType.DnD5e;

    private List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem(L["Worlds_Title"], "/worlds"),
        new BreadcrumbItem(World?.Name ?? "...")
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (World == null) return;

        // Update selected campaign/chapter when query params change
        SelectedCampaign = SelectedCampaignId.HasValue
            ? Campaigns.FirstOrDefault(c => c.Id == SelectedCampaignId.Value)
            : null;

        // Reset edit mode when navigating to a different campaign
        IsCampaignEditing = false;
        CampaignEditName = SelectedCampaign?.Name ?? string.Empty;
        CampaignEditDescription = SelectedCampaign?.Description ?? string.Empty;

        if (SelectedCampaignId.HasValue)
        {
            // Ensure the campaign is expanded and chapters are loaded
            if (!CampaignChapters.ContainsKey(SelectedCampaignId.Value))
            {
                var chapters = await ChapterClient.GetChaptersByCampaignAsync(SelectedCampaignId.Value);
                CampaignChapters[SelectedCampaignId.Value] = chapters;
            }
            ExpandedCampaigns[SelectedCampaignId.Value] = true;

            SelectedChapter = SelectedChapterId.HasValue
                ? CampaignChapters[SelectedCampaignId.Value].FirstOrDefault(c => c.Id == SelectedChapterId.Value)
                : null;

            // The chapter workspace needs the world players to resolve @mentions (PJ).
            if (SelectedChapterId.HasValue && WorldCharacters.Count == 0 && !IsLoadingWorldCharacters)
                await LoadWorldCharactersAsync();

            // The session-launch panel self-loads the active session; the parent only needs to
            // know whether one exists to show the tab dot.
            if (IsOwner && Section == null && !SelectedChapterId.HasValue)
            {
                _campaignHasActiveSession = await SessionClient.GetActiveSessionByCampaignAsync(SelectedCampaignId.Value) != null;
            }
        }
        else
        {
            SelectedChapter = null;
            _campaignHasActiveSession = false;
        }

        // Reset tab when switching campaigns
        if (SelectedCampaignId != _lastCampaignIdForTab)
        {
            _lastCampaignIdForTab = SelectedCampaignId;
            CampaignTab = "chapitres";
        }

        if (Section == "invitations" && !IsLoadingWorldCharacters && WorldCharacters.Count == 0)
            await LoadWorldCharactersAsync();

        SetSecondaryNav();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            // Identify current user
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
                CurrentUserId = uid;

            var worldTask = WorldClient.GetWorldByIdAsync(WorldId);
            await worldTask;
            World = worldTask.Result;

            if (World != null)
            {
                // Use world-level campaign endpoint (works for both GM and players)
                Campaigns = await WorldClient.GetWorldCampaignsAsync(WorldId);
                SetSecondaryNav();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ToggleCampaign(string key)
    {
        if (!int.TryParse(key, out var campaignId)) return;

        var currentlyExpanded = ExpandedCampaigns.GetValueOrDefault(campaignId, false);

        if (!currentlyExpanded && !CampaignChapters.ContainsKey(campaignId))
        {
            var chapters = await ChapterClient.GetChaptersByCampaignAsync(campaignId);
            CampaignChapters[campaignId] = chapters;
        }

        ExpandedCampaigns[campaignId] = !currentlyExpanded;
        SetSecondaryNav();
        await InvokeAsync(StateHasChanged);
    }

    private void SetSecondaryNav()
    {
        if (World == null) return;

        NavContext.OnToggleItem = async (key) => await ToggleCampaign(key);

        var items = new List<SecondaryNavItem>
        {
            new("Vue d'ensemble", $"/worlds/{WorldId}", "bi-info-circle",
                IsActive: Section == null && !SelectedCampaignId.HasValue),
        };

        if (IsOwner)
        {
            items.Add(new SecondaryNavItem("Description", $"/worlds/{WorldId}?section=description", "bi-pencil",
                IsActive: Section == "description"));
        }

        // Campaigns section
        items.Add(new SecondaryNavItem("Campagnes", "#", "bi-map", IsSection: true));

        foreach (var campaign in Campaigns)
        {
            var isExpanded = ExpandedCampaigns.GetValueOrDefault(campaign.Id, false);
            var isCampaignActive = SelectedCampaignId == campaign.Id && Section == null;

            var children = new List<SecondaryNavItem>();
            if (isExpanded && CampaignChapters.TryGetValue(campaign.Id, out var chapters))
            {
                if (IsOwner)
                {
                    foreach (var ch in chapters.OrderBy(c => c.ChapterNumber))
                    {
                        var chIcon = ch.IsCompleted ? "bi-check-circle-fill" : ch.IsActive ? "bi-circle-fill" : "bi-circle";
                        children.Add(new SecondaryNavItem(
                            $"Ch. {ch.ChapterNumber} — {ch.Title}",
                            $"/worlds/{WorldId}?campaignId={campaign.Id}&chapterId={ch.Id}",
                            chIcon,
                            IsActive: SelectedChapterId == ch.Id
                        ));
                    }
                    children.Add(new SecondaryNavItem(
                        L["Chapters_Create"],
                        $"/worlds/{WorldId}?campaignId={campaign.Id}&section=new-chapter",
                        "bi-plus-circle",
                        IsActive: Section == "new-chapter" && SelectedCampaignId == campaign.Id
                    ));
                }
            }

            items.Add(new SecondaryNavItem(
                campaign.Name,
                $"/worlds/{WorldId}?campaignId={campaign.Id}",
                "bi-map",
                IsActive: isCampaignActive,
                Children: isExpanded ? children : null,
                IsExpanded: isExpanded,
                ToggleKey: IsOwner ? campaign.Id.ToString() : null
            ));
        }

        if (IsOwner)
        {
            items.Add(new SecondaryNavItem(
                L["Campaigns_Create"],
                $"/worlds/{WorldId}?section=new-campaign",
                "bi-plus-circle",
                IsActive: Section == "new-campaign"
            ));

            // Management section
            items.Add(new SecondaryNavItem("Gestion", "#", "bi-gear", IsSection: true));
            items.Add(new SecondaryNavItem(
                "Événements",
                $"/worlds/{WorldId}?section=events",
                "bi-lightning",
                IsActive: Section == "events"
            ));
            items.Add(new SecondaryNavItem(
                "Succès",
                $"/worlds/{WorldId}?section=achievements",
                "bi-trophy",
                IsActive: Section == "achievements"
            ));
            items.Add(new SecondaryNavItem(
                "Invitations",
                $"/worlds/{WorldId}?section=invitations",
                "bi-person-plus",
                IsActive: Section == "invitations"
            ));
        }
        else
        {
            // Player section: link to their world character
            items.Add(new SecondaryNavItem("Mon personnage", $"/worlds/{WorldId}/my-character", "bi-person-badge",
                IsActive: false));
        }

        NavContext.SetContext(
            sectionTitle: World.Name,
            backHref: "/worlds",
            backLabel: L["Nav_Worlds"],
            items: items,
            gameType: World.GameType
        );
    }

    private async Task SaveWorldSettings()
    {
        if (World == null || string.IsNullOrWhiteSpace(WorldEditName)) return;
        IsSaving = true;
        try
        {
            var result = await WorldClient.UpdateWorldAsync(WorldId, new UpdateWorldRequest(WorldEditName, World.Description ?? string.Empty, World.IsActive, WorldEditGameType));
            if (result != null)
            {
                World = result;
                IsWorldEditing = false;
                SetSecondaryNav();
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void BeginWorldEdit()
    {
        if (World == null) return;
        WorldEditName = World.Name;
        WorldEditGameType = World.GameType;
        IsWorldEditing = true;
    }

    /// <summary>Applies the chapter saved by <c>WorldChapterEditorPanel</c>.</summary>
    private void OnChapterSaved(ChapterDto saved)
    {
        if (!SelectedCampaignId.HasValue) return;
        var chapters = CampaignChapters[SelectedCampaignId.Value];
        var idx = chapters.FindIndex(c => c.Id == saved.Id);
        if (idx >= 0) chapters[idx] = saved;
        SelectedChapter = saved;
        SetSecondaryNav();
    }

    /// <summary>Adds the chapter created by <c>WorldNewChapterForm</c> and opens it.</summary>
    private void OnChapterCreated(ChapterDto created)
    {
        if (!SelectedCampaignId.HasValue) return;
        if (!CampaignChapters.ContainsKey(SelectedCampaignId.Value))
            CampaignChapters[SelectedCampaignId.Value] = new();
        CampaignChapters[SelectedCampaignId.Value].Add(created);
        SetSecondaryNav();
        Nav.NavigateTo($"/worlds/{WorldId}?campaignId={SelectedCampaignId.Value}&chapterId={created.Id}");
    }

    // ── Drag-and-drop chapter reordering ──────────────────────────────────
    /// <summary>Applies the chapter order produced by <c>WorldChapterListPanel</c>.</summary>
    private void OnChaptersReordered(List<ChapterDto> ordered)
    {
        if (!SelectedCampaignId.HasValue) return;
        CampaignChapters[SelectedCampaignId.Value] = ordered;
        SetSecondaryNav();
    }

    private async Task SaveCampaignEdit()
    {
        if (SelectedCampaign == null) return;
        IsSaving = true;
        var request = new UpdateCampaignRequest(
            CampaignEditName.Trim(),
            CampaignEditDescription,
            SelectedCampaign.Visibility,
            SelectedCampaign.MaxPlayers,
            SelectedCampaign.Status);
        var result = await CampaignClient.UpdateCampaignAsync(SelectedCampaign.Id, request);
        if (result != null)
        {
            SelectedCampaign = result;
            var idx = Campaigns.FindIndex(c => c.Id == result.Id);
            if (idx >= 0) Campaigns[idx] = result;
            IsCampaignEditing = false;
            SetSecondaryNav();
        }
        IsSaving = false;
    }

    /// <summary>Adds the campaign created by <c>WorldNewCampaignForm</c> and opens it.</summary>
    private void OnCampaignCreated(CampaignDto created)
    {
        Campaigns.Add(created);
        SetSecondaryNav();
        Nav.NavigateTo($"/worlds/{WorldId}?campaignId={created.Id}");
    }

    private async Task LoadWorldCharactersAsync()
    {
        IsLoadingWorldCharacters = true;
        WorldCharacters = await WorldClient.GetWorldCharactersTypedAsync(WorldId);
        IsLoadingWorldCharacters = false;
    }


    private async Task RemoveWorldCharacter(int characterId)
    {
        var ok = await WorldClient.RemoveCharacterFromWorldAsync(WorldId, characterId);
        if (ok)
            WorldCharacters.RemoveAll(wc => wc.CharacterId == characterId);
    }

    private async Task GenerateInviteToken(int campaignId)
    {
        IsGeneratingToken = true;
        var token = await CampaignClient.GenerateInviteTokenAsync(campaignId);
        if (token != null)
        {
            var campaign = Campaigns.FirstOrDefault(c => c.Id == campaignId);
            if (campaign != null)
                campaign.InviteToken = token;
        }
        IsGeneratingToken = false;
    }

    private void ConfirmDeleteWorld() => DeleteWorldDialog.Show();

    private async Task DeleteWorldAsync()
    {
        if (World == null) return;
        var success = await WorldClient.DeleteWorldAsync(World.Id);
        if (success)
        {
            NavContext.ClearContext();
            Nav.NavigateTo("/worlds");
        }
    }

    private void ConfirmDeleteCampaign(CampaignDto campaign)
    {
        CampaignToDelete = campaign;
        DeleteCampaignDialog.Show();
    }

    private async Task DeleteCampaignAsync()
    {
        if (CampaignToDelete == null) return;
        var success = await CampaignClient.DeleteCampaignAsync(CampaignToDelete.Id);
        if (success)
        {
            Campaigns.Remove(CampaignToDelete);
            ExpandedCampaigns.Remove(CampaignToDelete.Id);
            CampaignChapters.Remove(CampaignToDelete.Id);
            CampaignToDelete = null;
            Nav.NavigateTo($"/worlds/{WorldId}");
        }
        else
        {
            CampaignToDelete = null;
        }
    }

    public string GetStatusClass(CampaignStatus status) => status.ToCssClass();

    public string GetStatusLabel(CampaignStatus status) => L[status.ToLabelKey()];


    public void Dispose()
    {
        NavContext.ClearContext();
    }
}
