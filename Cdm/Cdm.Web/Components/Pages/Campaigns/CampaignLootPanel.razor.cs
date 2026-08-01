using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Extensions;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;

namespace Cdm.Web.Components.Pages.Campaigns;

/// <summary>
/// Panneau réservé au MJ pour préparer le butin distribué aux joueurs pendant les sessions.
/// </summary>
public partial class CampaignLootPanel
{
    [Inject] private LootApiClient LootClient { get; set; } = default!;

    [Parameter] public int CampaignId { get; set; }

    [Parameter] public List<ChapterDto> Chapters { get; set; } = new();

    [Parameter] public GameType WorldGameType { get; set; } = GameType.Generic;

    private List<CampaignLootDto> Loot = new();
    private bool IsLoading = true;
    private bool ShowForm;
    private bool IsSaving;
    private int? EditingId;
    private string? FormError;
    private LootDraft Draft = new();

    /// <summary>
    /// Système proposé par défaut pour un nouvel objet : celui du monde s'il en a un, sinon
    /// générique (un objet générique reste utilisable sur n'importe quel thème).
    /// </summary>
    private GameType DefaultGameType =>
        this.WorldGameType is not (GameType.Generic or GameType.Empty) ? this.WorldGameType : GameType.Generic;

    protected override async Task OnInitializedAsync() => await this.Reload();

    private async Task Reload()
    {
        this.IsLoading = true;
        this.Loot = await this.LootClient.GetCampaignLootAsync(this.CampaignId);
        this.IsLoading = false;
    }

    private string ScopeLabel(int? chapterId)
    {
        if (!chapterId.HasValue) return "Campagne";

        var chapter = this.Chapters.FirstOrDefault(c => c.Id == chapterId.Value);
        return chapter != null ? $"Ch. {chapter.ChapterNumber}" : "Chapitre";
    }

    private static string GameTypeLabel(GameType type) => type.ToShortName();

    private void ToggleForm()
    {
        if (this.ShowForm)
        {
            this.CancelForm();
            return;
        }

        this.EditingId = null;
        this.FormError = null;
        this.Draft = new LootDraft { GameType = this.DefaultGameType };
        this.ShowForm = true;
    }

    private void CancelForm()
    {
        this.ShowForm = false;
        this.EditingId = null;
        this.FormError = null;
        this.Draft = new LootDraft();
    }

    private void EditLoot(CampaignLootDto loot)
    {
        this.EditingId = loot.Id;
        this.Draft = new LootDraft
        {
            Name = loot.Name,
            ItemType = loot.ItemType,
            Description = loot.Description,
            Quantity = loot.Quantity,
            ChapterId = loot.ChapterId?.ToString() ?? string.Empty,
            GameType = loot.GameType,
        };
        this.FormError = null;
        this.ShowForm = true;
    }

    private async Task SaveLoot()
    {
        if (string.IsNullOrWhiteSpace(this.Draft.Name))
        {
            this.FormError = "Le nom est requis.";
            return;
        }

        this.IsSaving = true;
        this.FormError = null;

        var dto = new CreateLootDto
        {
            Name = this.Draft.Name.Trim(),
            ItemType = string.IsNullOrWhiteSpace(this.Draft.ItemType) ? null : this.Draft.ItemType.Trim(),
            Description = string.IsNullOrWhiteSpace(this.Draft.Description) ? null : this.Draft.Description.Trim(),
            Quantity = this.Draft.Quantity < 1 ? 1 : this.Draft.Quantity,
            ChapterId = int.TryParse(this.Draft.ChapterId, out var chapterId) ? chapterId : null,
            GameType = this.Draft.GameType,
        };

        var result = this.EditingId.HasValue
            ? await this.LootClient.UpdateAsync(this.EditingId.Value, dto)
            : await this.LootClient.CreateAsync(this.CampaignId, dto);

        this.IsSaving = false;

        if (result == null)
        {
            this.FormError = "Enregistrement impossible.";
            return;
        }

        this.CancelForm();
        await this.Reload();
    }

    private async Task DeleteLoot(int id)
    {
        if (await this.LootClient.DeleteAsync(id))
        {
            this.Loot.RemoveAll(l => l.Id == id);
        }
    }

    /// <summary>Brouillon du formulaire : les champs de saisie restent des chaînes.</summary>
    private sealed class LootDraft
    {
        public string Name { get; set; } = string.Empty;

        public string? ItemType { get; set; }

        public string? Description { get; set; }

        public int Quantity { get; set; } = 1;

        public string ChapterId { get; set; } = string.Empty;

        public GameType GameType { get; set; } = GameType.Generic;
    }
}
