using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Campaigns;

public partial class CampaignDetail : IDisposable
{
    [Inject] private CampaignApiClient CampaignClient { get; set; } = default!;
    [Inject] private ChapterApiClient ChapterClient { get; set; } = default!;
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

    private List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem(L["Campaigns_Title"], "/campaigns"),
        new BreadcrumbItem(Campaign?.Name ?? "...")
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected override void OnParametersSet()
    {
        if (SelectedChapterId.HasValue && Chapters.Count > 0)
            SelectedChapter = Chapters.FirstOrDefault(c => c.Id == SelectedChapterId.Value);
        else
            SelectedChapter = null;

        if (Campaign != null)
            SetSecondaryNav();
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
}
