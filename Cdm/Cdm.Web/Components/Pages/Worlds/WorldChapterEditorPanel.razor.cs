using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace Cdm.Web.Components.Pages.Worlds;

/// <summary>
/// Chapter workspace extracted from <see cref="WorldDetail"/>: content editor with
/// @mention support, NPC panel and image gallery. Loads its own NPCs and owns the
/// JS interop reference; the parent is notified only when the chapter is saved.
/// </summary>
public partial class WorldChapterEditorPanel : IDisposable
{
    [Inject] private ChapterApiClient ChapterClient { get; set; } = default!;
    [Inject] private NpcApiClient NpcClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public ChapterDto Chapter { get; set; } = default!;
    [Parameter] public int CampaignId { get; set; }
    [Parameter] public bool IsDnD { get; set; }

    /// <summary>Player characters of the world, used for @mention (PJ) resolution.</summary>
    [Parameter] public List<WorldCharacterDto> WorldCharacters { get; set; } = new();

    /// <summary>Raised with the saved chapter so the parent can refresh its list.</summary>
    [Parameter] public EventCallback<ChapterDto> OnSaved { get; set; }

    private string ChapterTab = "contenu";
    private bool ChapterPreviewMode;
    private string ChapterContentDraft = string.Empty;
    private bool ChapterContentDirty;
    private bool IsSaving;
    private string? SaveMessage;
    private bool SaveSuccess;

    private List<NpcDto> ChapterNpcs = new();
    private int? _lastChapterId;

    /// <summary>NPC to expand in the panel (driven by a click on an @mention).</summary>
    private int? MentionExpandNpcId;

    // @mention JS interop
    private DotNetObjectReference<WorldChapterEditorPanel>? _dotNetRef;
    private IJSObjectReference? _mentionModule;
    private bool _needsMentionInit;
    private bool _needsPreviewClickInit;

    protected override async Task OnParametersSetAsync()
    {
        if (Chapter == null) return;

        if (_lastChapterId != Chapter.Id)
        {
            _lastChapterId = Chapter.Id;
            ChapterContentDraft = Chapter.Content ?? string.Empty;
            ChapterContentDirty = false;
            SaveMessage = null;
            ChapterTab = "contenu";
            ChapterPreviewMode = false;
            MentionExpandNpcId = null;
            _needsMentionInit = true;
            ChapterNpcs = await NpcClient.GetNpcsByChapterAsync(Chapter.Id);
        }
    }

    private void OnChapterContentInput(ChangeEventArgs e)
    {
        ChapterContentDraft = e.Value?.ToString() ?? string.Empty;
        ChapterContentDirty = true;
        SaveMessage = null;
    }

    private async Task SaveChapterContent()
    {
        if (Chapter == null) return;
        IsSaving = true;
        SaveMessage = null;
        try
        {
            var dto = new CreateChapterDto
            {
                CampaignId = CampaignId,
                Title = Chapter.Title,
                Content = ChapterContentDraft
            };
            var result = await ChapterClient.UpdateChapterAsync(Chapter.Id, dto);
            if (result != null)
            {
                ChapterContentDirty = false;
                SaveSuccess = true;
                SaveMessage = "Chapitre sauvegardé.";
                if (OnSaved.HasDelegate)
                {
                    await OnSaved.InvokeAsync(result);
                }
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

    // ── @mention JS interop ───────────────────────────────────────────────

    [JSInvokable]
    public List<MentionItem> GetMentionables()
    {
        var items = ChapterNpcs
            .Select(n => new MentionItem(n.Id, n.DisplayName, n.Description ?? string.Empty, "npc"))
            .ToList();
        items.AddRange(WorldCharacters
            .Select(c => new MentionItem(c.CharacterId, c.CharacterName, string.Empty, "pc")));
        return items;
    }

    [JSInvokable]
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

    private MarkupString RenderChapterContent(string text)
    {
        if (string.IsNullOrEmpty(text)) return new MarkupString(string.Empty);
        var npcMap = ChapterNpcs.ToDictionary(n => n.Id, n => n);
        var pcMap = WorldCharacters.ToDictionary(c => c.CharacterId, c => c);
        var escaped = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        // Render NPC mentions: @[Name](npc:id)
        var rendered = Regex.Replace(
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
        rendered = Regex.Replace(
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

        if (_needsMentionInit && ChapterTab == "contenu" && !ChapterPreviewMode && _dotNetRef != null)
        {
            _needsMentionInit = false;
            _mentionModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "/js/mention.js");
            await _mentionModule.InvokeVoidAsync("init", "chapter-content-editor", _dotNetRef);
        }

        if (_needsPreviewClickInit && ChapterTab == "contenu" && ChapterPreviewMode && _dotNetRef != null)
        {
            _needsPreviewClickInit = false;
            _mentionModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "/js/mention.js");
            await _mentionModule.InvokeVoidAsync("initPreviewClicks", "chapter-preview-content", _dotNetRef);
        }
    }

    public void Dispose() => _dotNetRef?.Dispose();

    public record MentionItem(int Id, string DisplayName, string Description, string Type);
}
