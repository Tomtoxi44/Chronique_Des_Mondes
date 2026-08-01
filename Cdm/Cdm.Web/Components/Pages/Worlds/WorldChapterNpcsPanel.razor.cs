using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Components.Shared;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace Cdm.Web.Components.Pages.Worlds;

/// <summary>
/// Panneau PNJ d'un chapitre : création, liste, édition inline et statistiques D&D.
/// Opère sur la liste partagée fournie par le parent (pour rester en phase avec le badge et
/// les @mentions) et le notifie via <see cref="OnChanged"/> après chaque mutation.
/// </summary>
public partial class WorldChapterNpcsPanel
{
    /// <summary>
    /// Options de sérialisation partagées. Une instance neuve à chaque appel annule le cache
    /// de métadonnées interne de <see cref="JsonSerializer"/> et refait toute la réflexion :
    /// les options doivent être statiques et réutilisées.
    /// </summary>
    private static readonly JsonSerializerOptions DndStatsJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Inject] private NpcApiClient NpcClient { get; set; } = default!;

    /// <summary>
    /// Lit les statistiques D&amp;D d'un PNJ. Appelée depuis le rendu : elle réutilise les
    /// options partagées au lieu d'en allouer à chaque PNJ et à chaque re-rendu.
    /// </summary>
    private static DndNpcStats? DeserializeDndStats(string? gameSpecificData)
    {
        if (string.IsNullOrEmpty(gameSpecificData)) return null;

        try
        {
            return JsonSerializer.Deserialize<DndNpcStats>(gameSpecificData, DndStatsJsonOptions);
        }
        catch (JsonException)
        {
            // Données d'un autre système, ou schéma plus ancien : on n'affiche pas de bloc D&D
            // plutôt que de faire tomber tout le rendu du chapitre.
            return null;
        }
    }

    [Parameter] public List<NpcDto> Npcs { get; set; } = new();

    [Parameter] public int ChapterId { get; set; }

    [Parameter] public bool IsDnD { get; set; }

    [Parameter] public EventCallback OnChanged { get; set; }

    /// <summary>PNJ à déplier (ex. depuis un clic sur une @mention). Traité une fois par valeur.</summary>
    [Parameter] public int? ExpandNpcId { get; set; }

    private int? handledExpandId;

    private bool ShowNpcForm;
    private bool IsSavingNpc;
    private CreateNpcDto NewNpc = new();
    private DndNpcStats NewNpcDndStats = new();
    private NpcDto? NpcToDelete;
    private AppConfirmDialog DeleteNpcDialog { get; set; } = default!;

    private HashSet<int> ExpandedNpcIds = new();
    private int? EditingNpcId;
    private CreateNpcDto EditingNpcDraft = new();
    private DndNpcStats EditingNpcDndStats = new();
    private bool IsSavingEditNpc;

    private int lastChapterId;

    protected override void OnParametersSet()
    {
        // Réinitialise l'état transitoire quand on change de chapitre.
        if (this.ChapterId != this.lastChapterId)
        {
            this.lastChapterId = this.ChapterId;
            this.ShowNpcForm = false;
            this.ExpandedNpcIds.Clear();
            this.EditingNpcId = null;
            this.EditingNpcDraft = new();
        }

        // Déplie un PNJ demandé par le parent (clic sur une @mention), une seule fois.
        if (this.ExpandNpcId.HasValue && this.ExpandNpcId != this.handledExpandId)
        {
            this.handledExpandId = this.ExpandNpcId;
            this.ExpandedNpcIds.Add(this.ExpandNpcId.Value);
        }
    }

    private void ShowAddNpc()
    {
        this.NewNpc = new CreateNpcDto { ChapterId = this.ChapterId };
        this.NewNpcDndStats = new DndNpcStats();
        this.ShowNpcForm = true;
    }

    private async Task CreateNpc()
    {
        this.IsSavingNpc = true;
        this.NewNpc.ChapterId = this.ChapterId;

        if (this.IsDnD)
        {
            this.NewNpc.GameSpecificData = JsonSerializer.Serialize(this.NewNpcDndStats, DndStatsJsonOptions);
        }

        var result = await this.NpcClient.CreateNpcAsync(this.NewNpc);
        if (result != null)
        {
            this.Npcs.Add(result);
            this.NewNpc = new CreateNpcDto { ChapterId = this.ChapterId };
            this.NewNpcDndStats = new DndNpcStats();
            this.ShowNpcForm = false;
            await this.OnChanged.InvokeAsync();
        }

        this.IsSavingNpc = false;
    }

    private void ConfirmDeleteNpc(NpcDto npc)
    {
        this.NpcToDelete = npc;
        this.DeleteNpcDialog.Show();
    }

    private async Task DeleteNpc()
    {
        if (this.NpcToDelete == null) return;

        var ok = await this.NpcClient.DeleteNpcAsync(this.NpcToDelete.Id);
        if (ok)
        {
            this.Npcs.Remove(this.NpcToDelete);
            await this.OnChanged.InvokeAsync();
        }

        this.NpcToDelete = null;
    }

    private void ToggleNpcExpand(int npcId)
    {
        if (!this.ExpandedNpcIds.Remove(npcId))
        {
            this.ExpandedNpcIds.Add(npcId);
            return;
        }

        // Replier un PNJ en cours d'édition abandonne l'édition.
        if (this.EditingNpcId == npcId)
        {
            this.EditingNpcId = null;
            this.EditingNpcDraft = new();
        }
    }

    private void StartEditNpc(NpcDto npc)
    {
        this.EditingNpcId = npc.Id;
        this.ExpandedNpcIds.Add(npc.Id);
        this.EditingNpcDraft = new CreateNpcDto
        {
            ChapterId = npc.ChapterId,
            FirstName = npc.FirstName,
            Name = npc.Name,
            Description = npc.Description,
            PhysicalDescription = npc.PhysicalDescription,
            Age = npc.Age,
            ImageUrl = npc.ImageUrl,
            GameSpecificData = npc.GameSpecificData,
        };

        this.EditingNpcDndStats = this.IsDnD && !string.IsNullOrEmpty(npc.GameSpecificData)
            ? JsonSerializer.Deserialize<DndNpcStats>(npc.GameSpecificData, DndStatsJsonOptions) ?? new DndNpcStats()
            : new DndNpcStats();
    }

    private void CancelEditNpc()
    {
        this.EditingNpcId = null;
        this.EditingNpcDraft = new();
        this.EditingNpcDndStats = new();
    }

    private async Task SaveEditNpc()
    {
        if (this.EditingNpcId == null) return;

        this.IsSavingEditNpc = true;

        if (this.IsDnD)
        {
            this.EditingNpcDraft.GameSpecificData = JsonSerializer.Serialize(this.EditingNpcDndStats, DndStatsJsonOptions);
        }

        var result = await this.NpcClient.UpdateNpcAsync(this.EditingNpcId.Value, this.EditingNpcDraft);
        if (result != null)
        {
            var idx = this.Npcs.FindIndex(n => n.Id == this.EditingNpcId.Value);
            if (idx >= 0) this.Npcs[idx] = result;

            this.EditingNpcId = null;
            this.EditingNpcDraft = new();
            this.EditingNpcDndStats = new();
            await this.OnChanged.InvokeAsync();
        }

        this.IsSavingEditNpc = false;
    }
}
