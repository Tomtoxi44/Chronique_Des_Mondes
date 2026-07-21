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
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace Cdm.Web.Components.Pages.Worlds;

public partial class WorldDetail : IDisposable
{
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private MarketplaceApiClient MarketClient { get; set; } = default!;
    [Inject] private ChapterApiClient ChapterClient { get; set; } = default!;
    [Inject] private NpcApiClient NpcClient { get; set; } = default!;
    [Inject] private EventApiClient EventClient { get; set; } = default!;
    [Inject] private AchievementApiClient AchievementClient { get; set; } = default!;
    [Inject] private SessionApiClient SessionClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
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
    private string? SaveMessage;
    private bool SaveSuccess = false;

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
    private CreateChapterDto NewChapter = new();
    private bool IsCampaignEditing = false;
    private string CampaignEditName = string.Empty;
    private string CampaignEditDescription = string.Empty;

    // Chapter drag-and-drop reorder
    private ChapterDto? _draggedChapter;
    private int? _dragOverChapterId;
    private bool IsReordering = false;

    // Campaign creation is delegated to WorldNewCampaignForm.

    // Chapter detail
    private ChapterDto? SelectedChapter;
    private string ChapterContentDraft = string.Empty;
    private bool ChapterContentDirty = false;

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

    private void OnChapterContentInput(ChangeEventArgs e)
    {
        ChapterContentDraft = e.Value?.ToString() ?? string.Empty;
        ChapterContentDirty = true;
        SaveMessage = null;
    }

    private AppConfirmDialog DeleteWorldDialog { get; set; } = default!;
    private AppConfirmDialog DeleteCampaignDialog { get; set; } = default!;
    private CampaignDto? CampaignToDelete;

    // Campaign tabs
    private string CampaignTab = "chapitres";
    private int? _lastCampaignIdForTab;

    // Chapter tabs (Contenu | PNJ)
    private string ChapterTab = "contenu";
    private int? _lastChapterIdForTab;
    private bool ChapterPreviewMode = false;

    // NPC list for the selected chapter — chargée ici (badge + @mentions),
    // l'UI/CRUD est déléguée au composant WorldChapterNpcsPanel.
    private List<NpcDto> ChapterNpcs = new();
    private int? _lastNpcChapterId;

    // PNJ à déplier dans le panneau (piloté par un clic sur une @mention).
    private int? MentionExpandNpcId;

    private bool IsWorldDnD5e => World?.GameType == GameType.DnD5e;

    // @mention JS interop
    private DotNetObjectReference<WorldDetail>? _dotNetRef;
    private IJSObjectReference? _mentionModule;
    private bool _needsMentionInit = false;
    private bool _needsPreviewClickInit = false;

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

            if (SelectedChapter != null)
                ChapterContentDraft = SelectedChapter.Content ?? string.Empty;

            // Load NPCs when chapter changes (le badge et les @mentions lisent ChapterNpcs ;
            // l'affichage/CRUD est délégué à WorldChapterNpcsPanel).
            if (SelectedChapterId != _lastNpcChapterId)
            {
                _lastNpcChapterId = SelectedChapterId;
                MentionExpandNpcId = null;
                if (SelectedChapterId.HasValue && SelectedChapter != null)
                    ChapterNpcs = await NpcClient.GetNpcsByChapterAsync(SelectedChapterId.Value);
                else
                    ChapterNpcs.Clear();
            }

            // Ensure WorldCharacters are loaded for @mention PJ support
            if (SelectedChapterId.HasValue && WorldCharacters.Count == 0 && !IsLoadingWorldCharacters)
                await LoadWorldCharactersAsync();

            // Reset chapter tab when switching chapters
            if (SelectedChapterId != _lastChapterIdForTab)
            {
                _lastChapterIdForTab = SelectedChapterId;
                ChapterTab = "contenu";
                ChapterPreviewMode = false;
                _needsMentionInit = true;
            }

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

        if (Section == "new-chapter" && SelectedCampaignId.HasValue)
        {
            var existingChapters = CampaignChapters.TryGetValue(SelectedCampaignId.Value, out var chs) ? chs : new();
            NewChapter = new CreateChapterDto
            {
                ChapterNumber = existingChapters.Count > 0 ? existingChapters.Max(c => c.ChapterNumber) + 1 : 1
            };
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

    private async Task SaveChapterContent()
    {
        if (SelectedChapter == null || !SelectedCampaignId.HasValue) return;
        IsSaving = true;
        SaveMessage = null;
        try
        {
            var dto = new CreateChapterDto
            {
                CampaignId = SelectedCampaignId.Value,
                Title = SelectedChapter.Title,
                Content = ChapterContentDraft
            };
            var result = await ChapterClient.UpdateChapterAsync(SelectedChapter.Id, dto);
            if (result != null)
            {
                var chapters = CampaignChapters[SelectedCampaignId.Value];
                var idx = chapters.FindIndex(c => c.Id == result.Id);
                if (idx >= 0) chapters[idx] = result;
                SelectedChapter = result;
                ChapterContentDirty = false;
                SaveSuccess = true;
                SaveMessage = "Chapitre sauvegardé.";
            }
            else
            {
                SaveSuccess = false;
                SaveMessage = "Erreur lors de la sauvegarde.";
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task CreateChapter()
    {
        if (!SelectedCampaignId.HasValue) return;
        IsSaving = true;
        NewChapter.CampaignId = SelectedCampaignId.Value;
        var result = await ChapterClient.CreateChapterAsync(NewChapter);
        if (result != null)
        {
            if (!CampaignChapters.ContainsKey(SelectedCampaignId.Value))
                CampaignChapters[SelectedCampaignId.Value] = new();
            CampaignChapters[SelectedCampaignId.Value].Add(result);
            NewChapter = new CreateChapterDto();
            SetSecondaryNav();
            Nav.NavigateTo($"/worlds/{WorldId}?campaignId={SelectedCampaignId.Value}&chapterId={result.Id}");
        }
        IsSaving = false;
    }

    // ── Drag-and-drop chapter reordering ──────────────────────────────────
    private void OnChapterDragStart(ChapterDto chapter)
    {
        _draggedChapter = chapter;
    }

    private void OnChapterDragOver(ChapterDto target)
    {
        _dragOverChapterId = target.Id;
    }

    private void OnChapterDragLeave()
    {
        _dragOverChapterId = null;
    }

    private async Task OnChapterDrop(ChapterDto target)
    {
        if (_draggedChapter == null || _draggedChapter.Id == target.Id || !SelectedCampaignId.HasValue) return;
        if (!CampaignChapters.TryGetValue(SelectedCampaignId.Value, out var chapters)) return;

        // Reorder list in memory
        var ordered = chapters.OrderBy(c => c.ChapterNumber).ToList();
        var from = ordered.IndexOf(_draggedChapter);
        var to = ordered.IndexOf(target);
        if (from < 0 || to < 0) return;

        ordered.RemoveAt(from);
        ordered.Insert(to, _draggedChapter);

        // Renumber starting at 1
        IsReordering = true;
        _draggedChapter = null;
        _dragOverChapterId = null;

        var tasks = new List<Task>();
        for (int i = 0; i < ordered.Count; i++)
        {
            var ch = ordered[i];
            var newNumber = i + 1;
            if (ch.ChapterNumber != newNumber)
            {
                ch.ChapterNumber = newNumber;
                var dto = new CreateChapterDto
                {
                    CampaignId = ch.CampaignId,
                    ChapterNumber = newNumber,
                    Title = ch.Title,
                    Content = ch.Content
                };
                tasks.Add(ChapterClient.UpdateChapterAsync(ch.Id, dto).ContinueWith(_ => { }));
            }
        }

        CampaignChapters[SelectedCampaignId.Value] = ordered;
        StateHasChanged();

        await Task.WhenAll(tasks);
        IsReordering = false;
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


    // ── @mention JS interop ───────────────────────────────────────────────

    [Microsoft.JSInterop.JSInvokable]
    public List<MentionItem> GetMentionables()
    {
        var items = ChapterNpcs
            .Select(n => new MentionItem(n.Id, n.DisplayName, n.Description ?? string.Empty, "npc"))
            .ToList();
        items.AddRange(WorldCharacters
            .Select(c => new MentionItem(c.CharacterId, c.CharacterName, string.Empty, "pc")));
        return items;
    }

    [Microsoft.JSInterop.JSInvokable]
    public async Task OpenMentionDetail(string type, int id)
    {
        if (type == "npc")
        {
            ChapterTab = "pnj";
            MentionExpandNpcId = id;
            await InvokeAsync(StateHasChanged);
        }
        else if (type == "pc")
        {
            await InvokeAsync(() => Nav.NavigateTo($"/characters/{id}"));
        }
    }

    private void SwitchToChapterTab(string tab)
    {
        ChapterTab = tab;
        if (tab == "contenu" && !ChapterPreviewMode)
            _needsMentionInit = true;
    }

    private void ToggleChapterPreview()
    {
        ChapterPreviewMode = !ChapterPreviewMode;
        if (!ChapterPreviewMode)
            _needsMentionInit = true;
        else
            _needsPreviewClickInit = true;
    }

    private MarkupString RenderChapterContent(string text)
    {
        if (string.IsNullOrEmpty(text)) return new MarkupString(string.Empty);
        var npcMap = ChapterNpcs.ToDictionary(n => n.Id, n => n);
        var pcMap = WorldCharacters.ToDictionary(c => c.CharacterId, c => c);
        var escaped = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        // Render NPC mentions: @[Name](npc:id)
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

        // Render PJ mentions: @[Name](pc:id)
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            _dotNetRef = DotNetObjectReference.Create(this);

        if (_needsMentionInit && ChapterTab == "contenu" && !ChapterPreviewMode
            && SelectedChapter != null && _dotNetRef != null)
        {
            _needsMentionInit = false;
            _mentionModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "/js/mention.js");
            await _mentionModule.InvokeVoidAsync("init", "chapter-content-editor", _dotNetRef);
        }

        if (_needsPreviewClickInit && ChapterTab == "contenu" && ChapterPreviewMode
            && SelectedChapter != null && _dotNetRef != null)
        {
            _needsPreviewClickInit = false;
            _mentionModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "/js/mention.js");
            await _mentionModule.InvokeVoidAsync("initPreviewClicks", "chapter-preview-content", _dotNetRef);
        }
    }

    public void Dispose()
    {
        NavContext.ClearContext();
        _dotNetRef?.Dispose();
    }

    public record MentionItem(int Id, string DisplayName, string Description, string Type);
}
