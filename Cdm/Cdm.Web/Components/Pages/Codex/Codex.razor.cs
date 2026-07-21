// -----------------------------------------------------------------------
// <copyright file="Codex.razor.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Cdm.Web.Components.Pages.Codex;

public partial class Codex
{

    private static readonly IReadOnlyList<(GameType Value, string Label)> GameTypes = Cdm.Web.Extensions.GameTypeExtensions.Selectable;

    private List<CodexItemDto> Items = new();
    private bool IsLoading = true;
    private GameType? FilterType;

    private bool IsFormOpen;
    private int? EditingId;
    private CreateCodexItemDto Form = new();
    private bool IsSaving;
    private string? FormError;

    // D&D 5e weapon stats (persisted in Form.GameSpecificData as JSON).
    private string? DndDamageDice;
    private string? DndDamageType;
    private int? DndAttackBonus;

    private static readonly System.Text.Json.JsonSerializerOptions DndJsonOptions =
        new() { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };

    // Ajout à un personnage
    private int? AddTargetItemId;
    private List<WorldCharacterDto> CompatibleCharacters = new();
    private int SelectedCharacterId;
    private bool IsAdding;
    private string? AddError;

    private string FormTitle => EditingId.HasValue ? "Modifier l'item" : "Nouvel item";

    private List<CodexItemDto> Filtered =>
        FilterType.HasValue ? Items.Where(i => i.GameType == FilterType.Value).ToList() : Items;

    protected override async Task OnInitializedAsync()
    {
        Items = await CodexClient.GetMyItemsAsync();
        IsLoading = false;
    }

    private void SetFilter(GameType? type) => FilterType = type;

    private static string GameLabel(GameType type) => Cdm.Web.Extensions.GameTypeExtensions.ToShortName(type);

    /// <summary>Short damage label ("1d8 tranchant (+5)") from a D&amp;D item's stored stats, or empty.</summary>
    private static string DndDamageLabel(string? gameSpecificData)
    {
        if (string.IsNullOrWhiteSpace(gameSpecificData)) return string.Empty;
        try
        {
            var stats = System.Text.Json.JsonSerializer.Deserialize<DndItemStats>(gameSpecificData, DndJsonOptions);
            if (stats == null || string.IsNullOrWhiteSpace(stats.DamageDice)) return string.Empty;
            var label = stats.DamageDice!;
            if (!string.IsNullOrWhiteSpace(stats.DamageType)) label += $" {stats.DamageType}";
            if (stats.AttackBonus.HasValue) label += $" ({(stats.AttackBonus >= 0 ? "+" : "")}{stats.AttackBonus})";
            return label;
        }
        catch { return string.Empty; }
    }

    private void StartCreate()
    {
        EditingId = null;
        Form = new CreateCodexItemDto { GameType = FilterType ?? GameType.Generic };
        DndDamageDice = null;
        DndDamageType = null;
        DndAttackBonus = null;
        FormError = null;
        IsFormOpen = true;
    }

    private void StartEdit(CodexItemDto item)
    {
        EditingId = item.Id;
        Form = new CreateCodexItemDto
        {
            Name = item.Name,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            GameType = item.GameType,
            ItemType = item.ItemType,
            GameSpecificData = item.GameSpecificData,
        };

        // Pre-fill the D&D weapon fields from the stored JSON.
        DndDamageDice = null;
        DndDamageType = null;
        DndAttackBonus = null;
        if (item.GameType == GameType.DnD5e && !string.IsNullOrWhiteSpace(item.GameSpecificData))
        {
            try
            {
                var stats = System.Text.Json.JsonSerializer.Deserialize<DndItemStats>(item.GameSpecificData, DndJsonOptions);
                if (stats != null)
                {
                    DndDamageDice = stats.DamageDice;
                    DndDamageType = stats.DamageType;
                    DndAttackBonus = stats.AttackBonus;
                }
            }
            catch { /* données non conformes : on repart de champs vides */ }
        }

        FormError = null;
        IsFormOpen = true;
    }

    private void CancelForm()
    {
        IsFormOpen = false;
        EditingId = null;
    }

    private async Task SaveForm()
    {
        if (string.IsNullOrWhiteSpace(Form.Name)) return;
        IsSaving = true;
        FormError = null;

        // Persist the D&D weapon stats into GameSpecificData (or clear it for other systems).
        if (Form.GameType == GameType.DnD5e &&
            (!string.IsNullOrWhiteSpace(DndDamageDice) || !string.IsNullOrWhiteSpace(DndDamageType) || DndAttackBonus.HasValue))
        {
            Form.GameSpecificData = System.Text.Json.JsonSerializer.Serialize(
                new DndItemStats
                {
                    DamageDice = string.IsNullOrWhiteSpace(DndDamageDice) ? null : DndDamageDice.Trim(),
                    DamageType = string.IsNullOrWhiteSpace(DndDamageType) ? null : DndDamageType.Trim(),
                    AttackBonus = DndAttackBonus,
                },
                DndJsonOptions);
        }
        else if (Form.GameType != GameType.DnD5e)
        {
            Form.GameSpecificData = null;
        }

        try
        {
            if (EditingId.HasValue)
            {
                var updated = await CodexClient.UpdateAsync(EditingId.Value, Form);
                if (updated != null)
                {
                    var idx = Items.FindIndex(i => i.Id == EditingId.Value);
                    if (idx >= 0) Items[idx] = updated;
                    IsFormOpen = false;
                }
                else { FormError = "Échec de la mise à jour."; }
            }
            else
            {
                var created = await CodexClient.CreateAsync(Form);
                if (created != null)
                {
                    Items.Insert(0, created);
                    IsFormOpen = false;
                }
                else { FormError = "Échec de la création."; }
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task Delete(CodexItemDto item)
    {
        if (await CodexClient.DeleteAsync(item.Id))
        {
            Items.RemoveAll(i => i.Id == item.Id);
        }
    }

    private async Task ToggleShare(CodexItemDto item)
    {
        if (await CodexClient.SetSharedAsync(item.Id, !item.IsShared))
        {
            item.IsShared = !item.IsShared;
        }
    }

    private async Task OpenAdd(CodexItemDto item)
    {
        AddTargetItemId = item.Id;
        SelectedCharacterId = 0;
        AddError = null;
        // A generic item can be added to any character; a themed item only to matching-type characters.
        CompatibleCharacters = item.GameType == GameType.Generic
            ? await CodexClient.GetCompatibleCharactersAsync(null)
            : await CodexClient.GetCompatibleCharactersAsync(item.GameType);
    }

    private async Task ConfirmAdd(CodexItemDto item)
    {
        if (SelectedCharacterId == 0) return;
        IsAdding = true;
        AddError = null;
        try
        {
            var (ok, error) = await CodexClient.AddToCharacterAsync(item.Id, SelectedCharacterId);
            if (ok)
            {
                AddTargetItemId = null;
            }
            else
            {
                AddError = error ?? "Ajout impossible.";
            }
        }
        finally
        {
            IsAdding = false;
        }
    }
}
