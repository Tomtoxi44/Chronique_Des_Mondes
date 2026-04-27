using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.DTOs.DnD5e;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Security.Claims;

namespace Cdm.Web.Components.Pages.Campaigns;

public partial class CampaignDetail : IDisposable
{
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private ChapterApiClient ChapterClient { get; set; } = default!;
    [Inject] private NpcApiClient NpcClient { get; set; } = default!;
    [Inject] private DndApiClient DndClient { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    [Parameter] public int CampaignId { get; set; }

    [SupplyParameterFromQuery(Name = "chapterId")]
    private int? SelectedChapterId { get; set; }

    private CampaignDto? Campaign;
    private List<ChapterDto> Chapters = new();
    private ChapterDto? SelectedChapter;
    private bool IsLoading = true;
    private bool ShowChapterForm = false;
    private bool IsSaving = false;
    private ChapterDto? ChapterToDelete;
    private AppConfirmDialog DeleteChapterDialog { get; set; } = default!;
    private CreateChapterDto NewChapter { get; set; } = new();

    // GM detection
    private int CurrentUserId;
    private bool IsGm => Campaign != null && Campaign.CreatedBy == CurrentUserId;
    private bool IsDnd5e => Campaign?.GameType == GameType.DnD5e;

    // NPC management (GM only, per chapter)
    private List<NpcDto> Npcs = new();
    private bool ShowNpcForm = false;
    private bool IsSavingNpc = false;
    private CreateNpcDto NewNpc { get; set; } = new();
    private NpcDto? NpcToDelete;
    private AppConfirmDialog DeleteNpcDialog { get; set; } = default!;
    private int? _lastNpcChapterId;

    // D&D NPC extras
    private CreateDndNpcDto NewDndNpc { get; set; } = new();
    private List<DndMonsterTemplateDto> MonsterTemplates = new();
    private int SelectedMonsterTemplateId = 0;

    private List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem(L["Campaigns_Title"], "/campaigns"),
        new BreadcrumbItem(Campaign?.Name ?? "...")
    };

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
            CurrentUserId = uid;

        await LoadAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (SelectedChapterId.HasValue && Chapters.Count > 0)
            SelectedChapter = Chapters.FirstOrDefault(c => c.Id == SelectedChapterId.Value);
        else
            SelectedChapter = null;

        if (Campaign != null)
            SetSecondaryNav();

        // Load NPCs when chapter changes (GM only)
        if (IsGm && SelectedChapterId != _lastNpcChapterId)
        {
            _lastNpcChapterId = SelectedChapterId;
            ShowNpcForm = false;
            if (SelectedChapterId.HasValue)
                await LoadNpcsForChapterAsync(SelectedChapterId.Value);
            else
                Npcs.Clear();
        }
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            Campaign = await CampaignClient.GetCampaignByIdAsync(CampaignId);
            if (Campaign != null)
            {
                Chapters = await ChapterClient.GetChaptersByCampaignAsync(CampaignId);
                SetSecondaryNav();

                if (Campaign.GameType == GameType.DnD5e && IsGm)
                    MonsterTemplates = await DndClient.GetMonstersAsync();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SetSecondaryNav()
    {
        if (Campaign == null) return;

        var items = new List<SecondaryNavItem>
        {
            new SecondaryNavItem("Vue d'ensemble", $"/campaigns/{CampaignId}", "bi-info-circle",
                IsActive: !SelectedChapterId.HasValue)
        };

        foreach (var ch in Chapters.OrderBy(c => c.ChapterNumber))
        {
            var icon = ch.IsCompleted ? "bi-check-circle-fill" : ch.IsActive ? "bi-circle-fill" : "bi-circle";
            items.Add(new SecondaryNavItem(
                Label: $"Ch. {ch.ChapterNumber} — {ch.Title}",
                Href: $"/campaigns/{CampaignId}?chapterId={ch.Id}",
                Icon: icon,
                IsActive: SelectedChapterId == ch.Id
            ));
        }

        items.Add(new SecondaryNavItem(
            Label: L["Chapters_Create"],
            Href: "#new-chapter",
            Icon: "bi-plus-circle"
        ));

        NavContext.SetContext(
            sectionTitle: Campaign.Name,
            backHref: "/campaigns",
            backLabel: L["Nav_Campaigns"],
            items: items,
            gameType: Campaign.GameType
        );
    }

    private void ShowCreateChapter()
    {
        ShowChapterForm = true;
        NewChapter = new CreateChapterDto { CampaignId = CampaignId };
    }

    private async Task CreateChapter()
    {
        IsSaving = true;
        NewChapter.CampaignId = CampaignId;
        var result = await ChapterClient.CreateChapterAsync(NewChapter);
        if (result != null)
        {
            Chapters.Add(result);
            ShowChapterForm = false;
            NewChapter = new CreateChapterDto();
            SetSecondaryNav();
        }
        IsSaving = false;
    }

    private async Task StartChapter(ChapterDto chapter)
    {
        var result = await ChapterClient.StartChapterAsync(chapter.Id);
        if (result != null)
        {
            var idx = Chapters.FindIndex(c => c.Id == chapter.Id);
            if (idx >= 0) Chapters[idx] = result;
            SelectedChapter = result;
            SetSecondaryNav();
        }
    }

    private async Task CompleteChapter(ChapterDto chapter)
    {
        var result = await ChapterClient.CompleteChapterAsync(chapter.Id);
        if (result != null)
        {
            var idx = Chapters.FindIndex(c => c.Id == chapter.Id);
            if (idx >= 0) Chapters[idx] = result;
            SelectedChapter = result;
            SetSecondaryNav();
        }
    }

    private void ConfirmDeleteChapter(ChapterDto chapter)
    {
        ChapterToDelete = chapter;
        DeleteChapterDialog.Show();
    }

    private async Task DeleteChapter()
    {
        if (ChapterToDelete == null) return;
        var success = await ChapterClient.DeleteChapterAsync(ChapterToDelete.Id);
        if (success)
        {
            Chapters.Remove(ChapterToDelete);
            if (SelectedChapter?.Id == ChapterToDelete.Id)
            {
                SelectedChapter = null;
                Nav.NavigateTo($"/campaigns/{CampaignId}");
            }
            SetSecondaryNav();
        }
        ChapterToDelete = null;
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

    public void Dispose() => NavContext.ClearContext();

    private async Task LoadNpcsForChapterAsync(int chapterId)
    {
        if (IsDnd5e)
            Npcs = (await DndClient.GetDndNpcsAsync(chapterId)).Cast<NpcDto>().ToList();
        else
            Npcs = await NpcClient.GetNpcsByChapterAsync(chapterId);
    }

    private void ShowAddNpc()
    {
        NewNpc = new CreateNpcDto { ChapterId = SelectedChapter!.Id };
        NewDndNpc = new CreateDndNpcDto { ChapterId = SelectedChapter!.Id };
        SelectedMonsterTemplateId = 0;
        ShowNpcForm = true;
    }

    private async Task CreateNpc()
    {
        if (SelectedChapter == null) return;
        IsSavingNpc = true;

        if (IsDnd5e)
        {
            NewDndNpc.ChapterId = SelectedChapter.Id;
            if (SelectedMonsterTemplateId > 0)
                NewDndNpc.MonsterTemplateId = SelectedMonsterTemplateId;

            var dndResult = await DndClient.CreateDndNpcAsync(SelectedChapter.Id, NewDndNpc);
            if (dndResult != null)
            {
                Npcs.Add(dndResult);
                NewDndNpc = new CreateDndNpcDto { ChapterId = SelectedChapter.Id };
                SelectedMonsterTemplateId = 0;
                ShowNpcForm = false;
            }
        }
        else
        {
            NewNpc.ChapterId = SelectedChapter.Id;
            var result = await NpcClient.CreateNpcAsync(NewNpc);
            if (result != null)
            {
                Npcs.Add(result);
                NewNpc = new CreateNpcDto { ChapterId = SelectedChapter.Id };
                ShowNpcForm = false;
            }
        }

        IsSavingNpc = false;
    }

    private void ConfirmDeleteNpc(NpcDto npc)
    {
        NpcToDelete = npc;
        DeleteNpcDialog.Show();
    }

    private async Task DeleteNpc()
    {
        if (NpcToDelete == null) return;
        var success = await NpcClient.DeleteNpcAsync(NpcToDelete.Id);
        if (success)
            Npcs.Remove(NpcToDelete);
        NpcToDelete = null;
    }
}
