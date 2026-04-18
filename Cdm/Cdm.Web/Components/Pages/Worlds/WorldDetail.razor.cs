using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Cdm.Web.Components.Pages.Worlds;

public partial class WorldDetail : IDisposable
{
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private ChapterApiClient ChapterClient { get; set; } = default!;
    [Inject] private EventApiClient EventClient { get; set; } = default!;
    [Inject] private AchievementApiClient AchievementClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

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

    // Description editor
    private string DescriptionDraft = string.Empty;
    private bool DescriptionDirty = false;

    // Campaign detail
    private CampaignDto? SelectedCampaign;
    private bool ShowNewChapterForm = false;
    private CreateChapterDto NewChapter = new();
    private bool IsCampaignEditing = false;
    private string CampaignEditName = string.Empty;
    private string CampaignEditDescription = string.Empty;

    // New campaign (inline)
    private bool ShowNewCampaignForm = false;
    private CreateCampaignDto NewCampaign = new();
    private string? CampaignCreateError;

    // Chapter detail
    private ChapterDto? SelectedChapter;
    private string ChapterContentDraft = string.Empty;
    private bool ChapterContentDirty = false;

    // Invite tokens
    private bool IsGeneratingToken = false;

    // Events
    private List<EventDto> WorldEvents = new();
    private bool IsLoadingEvents = false;
    private bool ShowNewEventForm = false;
    private CreateEventDto NewEvent = new() { Level = EventLevel.World, EffectType = EventEffectType.Narrative, IsPermanent = true };

    // Achievements
    private List<AchievementDto> WorldAchievements = new();
    private bool IsLoadingAchievements = false;
    private bool ShowNewAchievementForm = false;
    private CreateAchievementDto NewAchievement = new() { Level = AchievementLevel.World, Rarity = AchievementRarity.Common };

    private void OnDescriptionInput(ChangeEventArgs e)
    {
        DescriptionDraft = e.Value?.ToString() ?? string.Empty;
        DescriptionDirty = true;
        SaveMessage = null;
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
        }
        else
        {
            SelectedChapter = null;
        }

        if (Section == "description")
            DescriptionDraft = World.Description ?? string.Empty;

        ShowNewCampaignForm = Section == "new-campaign";

        if (Section == "events" && !IsLoadingEvents && WorldEvents.Count == 0)
            await LoadEventsAsync();

        if (Section == "achievements" && !IsLoadingAchievements && WorldAchievements.Count == 0)
            await LoadAchievementsAsync();

        SetSecondaryNav();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var worldTask = WorldClient.GetWorldByIdAsync(WorldId);
            var campaignsTask = CampaignClient.GetMyCampaignsAsync();
            await Task.WhenAll(worldTask, campaignsTask);

            World = worldTask.Result;
            var allCampaigns = campaignsTask.Result;
            Campaigns = allCampaigns.Where(c => c.WorldId == WorldId).ToList();

            if (World != null)
            {
                DescriptionDraft = World.Description ?? string.Empty;
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
            new("Description", $"/worlds/{WorldId}?section=description", "bi-pencil",
                IsActive: Section == "description"),
        };

        // Campaigns section
        items.Add(new SecondaryNavItem("Campagnes", "#", "bi-map", IsSection: true));

        foreach (var campaign in Campaigns)
        {
            var isExpanded = ExpandedCampaigns.GetValueOrDefault(campaign.Id, false);
            var isCampaignActive = SelectedCampaignId == campaign.Id && Section == null;

            var children = new List<SecondaryNavItem>();
            if (isExpanded && CampaignChapters.TryGetValue(campaign.Id, out var chapters))
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
                    $"/worlds/{WorldId}?campaignId={campaign.Id}#new-chapter",
                    "bi-plus-circle"
                ));
            }

            items.Add(new SecondaryNavItem(
                campaign.Name,
                $"/worlds/{WorldId}?campaignId={campaign.Id}",
                "bi-map",
                IsActive: isCampaignActive,
                Children: isExpanded ? children : null,
                IsExpanded: isExpanded,
                ToggleKey: campaign.Id.ToString()
            ));
        }

        items.Add(new SecondaryNavItem(
            L["Campaigns_Create"],
            $"/worlds/{WorldId}?section=new-campaign",
            "bi-plus-circle"
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

        NavContext.SetContext(
            sectionTitle: World.Name,
            backHref: "/worlds",
            backLabel: L["Nav_Worlds"],
            items: items,
            gameType: World.GameType
        );
    }

    private async Task SaveDescription()
    {
        if (World == null) return;
        IsSaving = true;
        SaveMessage = null;
        try
        {
            var result = await WorldClient.UpdateWorldAsync(WorldId, new UpdateWorldRequest(World.Name, DescriptionDraft, World.IsActive, World.GameType));
            if (result != null)
            {
                World = result;
                DescriptionDirty = false;
                SaveSuccess = true;
                SaveMessage = "Description sauvegardée.";
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
            ShowNewChapterForm = false;
            NewChapter = new CreateChapterDto();
            SetSecondaryNav();
        }
        IsSaving = false;
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

    private async Task CreateCampaignInline()
    {
        if (World == null) return;
        IsSaving = true;
        CampaignCreateError = null;
        NewCampaign.WorldId = WorldId;
        var result = await CampaignClient.CreateCampaignAsync(NewCampaign);
        if (result != null)
        {
            Campaigns.Add(result);
            ShowNewCampaignForm = false;
            NewCampaign = new CreateCampaignDto();
            SetSecondaryNav();
            Nav.NavigateTo($"/worlds/{WorldId}?campaignId={result.Id}");
        }
        else
        {
            CampaignCreateError = "Erreur lors de la création de la campagne.";
        }
        IsSaving = false;
    }

    private async Task LoadEventsAsync()
    {
        IsLoadingEvents = true;
        WorldEvents = await EventClient.GetEventsByWorldAsync(WorldId);
        IsLoadingEvents = false;
    }

    private async Task CreateEvent()
    {
        if (World == null) return;
        IsSaving = true;
        NewEvent.WorldId = WorldId;
        NewEvent.Level = EventLevel.World;
        var result = await EventClient.CreateEventAsync(NewEvent);
        if (result != null)
        {
            WorldEvents.Insert(0, result);
            ShowNewEventForm = false;
            NewEvent = new() { Level = EventLevel.World, EffectType = EventEffectType.Narrative, IsPermanent = true };
        }
        IsSaving = false;
    }

    private async Task DeleteEvent(int eventId)
    {
        var ok = await EventClient.DeleteEventAsync(eventId);
        if (ok)
            WorldEvents.RemoveAll(e => e.Id == eventId);
    }

    private async Task ToggleEventActive(EventDto ev)
    {
        var result = await EventClient.SetEventActiveAsync(ev.Id, !ev.IsActive);
        if (result != null)
        {
            var idx = WorldEvents.FindIndex(e => e.Id == result.Id);
            if (idx >= 0) WorldEvents[idx] = result;
        }
    }

    private async Task LoadAchievementsAsync()
    {
        IsLoadingAchievements = true;
        WorldAchievements = await AchievementClient.GetAchievementsByWorldAsync(WorldId);
        IsLoadingAchievements = false;
    }

    private async Task CreateAchievement()
    {
        if (World == null) return;
        IsSaving = true;
        NewAchievement.WorldId = WorldId;
        NewAchievement.Level = AchievementLevel.World;
        var result = await AchievementClient.CreateAchievementAsync(NewAchievement);
        if (result != null)
        {
            WorldAchievements.Insert(0, result);
            ShowNewAchievementForm = false;
            NewAchievement = new() { Level = AchievementLevel.World, Rarity = AchievementRarity.Common };
        }
        IsSaving = false;
    }

    private async Task DeleteAchievement(int achievementId)
    {
        var ok = await AchievementClient.DeleteAchievementAsync(achievementId);
        if (ok)
            WorldAchievements.RemoveAll(a => a.Id == achievementId);
    }

    private static string GetRarityColor(AchievementRarity rarity) => rarity switch
    {
        AchievementRarity.Common => "var(--color-text-muted)",
        AchievementRarity.Rare => "#3b82f6",
        AchievementRarity.Epic => "#8b5cf6",
        AchievementRarity.Legendary => "#f59e0b",
        _ => "var(--color-border)"
    };

    private static string GetRarityBorderStyle(AchievementRarity rarity) =>
        $"border-top: 3px solid {GetRarityColor(rarity)};";

    private static string GetRarityClass(AchievementRarity rarity) => rarity switch
    {
        AchievementRarity.Common => "rarity-common",
        AchievementRarity.Rare => "rarity-rare",
        AchievementRarity.Epic => "rarity-epic",
        AchievementRarity.Legendary => "rarity-legendary",
        _ => ""
    };

    private static string GetRarityLabel(AchievementRarity rarity) => rarity switch
    {
        AchievementRarity.Common => "Commun",
        AchievementRarity.Rare => "Rare",
        AchievementRarity.Epic => "Épique",
        AchievementRarity.Legendary => "Légendaire",
        _ => rarity.ToString()
    };

    private static string GetEffectLabel(EventEffectType type) => type switch
    {
        EventEffectType.StatModifier => "Modificateur de stat",
        EventEffectType.HealthModifier => "Modificateur de PV",
        EventEffectType.DiceModifier => "Modificateur de dé",
        EventEffectType.Narrative => "Narratif",
        _ => type.ToString()
    };

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

    private async Task CopyToClipboard(string text)
    {
        try
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
        catch { /* silently ignore if clipboard API not available */ }
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

    public string GetStatusClass(CampaignStatus status) => status switch
    {
        CampaignStatus.Active => "status-active",
        CampaignStatus.Planning => "status-planning",
        CampaignStatus.Completed => "status-completed",
        CampaignStatus.Cancelled => "status-cancelled",
        CampaignStatus.OnHold => "status-paused",
        _ => ""
    };

    public string GetStatusLabel(CampaignStatus status) => status switch
    {
        CampaignStatus.Active => L["Campaigns_Status_Active"],
        CampaignStatus.Planning => L["Campaigns_Status_Planning"],
        CampaignStatus.Completed => L["Campaigns_Status_Completed"],
        CampaignStatus.Cancelled => L["Campaigns_Status_Cancelled"],
        CampaignStatus.OnHold => L["Campaigns_Status_OnHold"],
        _ => status.ToString()
    };

    public void Dispose()
    {
        NavContext.ClearContext();
    }
}
