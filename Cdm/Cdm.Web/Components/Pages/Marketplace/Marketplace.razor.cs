// -----------------------------------------------------------------------
// <copyright file="Marketplace.razor.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Cdm.Web.Components.Pages.Marketplace;

public partial class Marketplace
{

    private enum Tab { Items, Worlds, Campaigns, Characters }

    private static readonly IReadOnlyList<(GameType Value, string Label)> GameTypes = Cdm.Web.Extensions.GameTypeExtensions.Selectable;

    private Tab ActiveTab = Tab.Items;
    private List<CodexItemDto> Items = new();
    private List<MarketplaceEntryDto> Entries = new();
    private List<WorldDto> MyWorlds = new();
    private readonly Dictionary<int, int> TargetWorlds = new();
    private bool IsLoading = true;
    private GameType? FilterType;
    private string SearchTerm = string.Empty;
    private int? ImportingId;
    private readonly HashSet<int> ImportedIds = new();
    private string? ErrorMessage;

    private string SearchPlaceholder => ActiveTab switch
    {
        Tab.Worlds => "Rechercher un monde…",
        Tab.Campaigns => "Rechercher une campagne…",
        Tab.Characters => "Rechercher un personnage…",
        _ => "Rechercher un item…",
    };

    private string EmptyMessage => ActiveTab switch
    {
        Tab.Worlds => "Aucun monde partagé ne correspond.",
        Tab.Campaigns => "Aucune campagne partagée ne correspond.",
        Tab.Characters => "Aucun personnage partagé ne correspond.",
        _ => "Aucun item partagé ne correspond.",
    };

    protected override async Task OnInitializedAsync()
    {
        MyWorlds = await WorldClient.GetMyWorldsAsync();
        await Reload();
    }

    private async Task SwitchTab(Tab tab)
    {
        if (ActiveTab == tab) return;
        ActiveTab = tab;
        FilterType = null;
        SearchTerm = string.Empty;
        ImportedIds.Clear();
        ErrorMessage = null;
        await Reload();
    }

    private async Task Reload()
    {
        IsLoading = true;
        ErrorMessage = null;
        var search = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm;
        switch (ActiveTab)
        {
            case Tab.Items:
                Items = await CodexClient.GetMarketplaceItemsAsync(FilterType, search);
                break;
            case Tab.Worlds:
                Entries = await MarketClient.GetSharedWorldsAsync(FilterType, search);
                break;
            case Tab.Campaigns:
                Entries = await MarketClient.GetSharedCampaignsAsync(FilterType, search);
                break;
            case Tab.Characters:
                Entries = await MarketClient.GetSharedCharactersAsync(search);
                break;
        }
        IsLoading = false;
    }

    private async Task SetFilter(GameType? type)
    {
        FilterType = type;
        await Reload();
    }

    private async Task OnSearchKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await Reload();
        }
    }

    private int GetTargetWorld(int campaignId, List<WorldDto> compatible)
        => TargetWorlds.TryGetValue(campaignId, out var id) ? id : compatible[0].Id;

    private void SetTargetWorld(int campaignId, string? value)
    {
        if (int.TryParse(value, out var id))
        {
            TargetWorlds[campaignId] = id;
        }
    }

    private async Task ImportItem(int id)
    {
        ImportingId = id;
        try
        {
            var imported = await CodexClient.ImportItemAsync(id);
            if (imported != null)
            {
                ImportedIds.Add(id);
            }
            else
            {
                ErrorMessage = "Import impossible.";
            }
        }
        finally
        {
            ImportingId = null;
        }
    }

    private async Task ImportEntry(int id)
    {
        ImportingId = id;
        ErrorMessage = null;
        try
        {
            var (success, error) = ActiveTab == Tab.Worlds
                ? await MarketClient.ImportWorldAsync(id)
                : await MarketClient.ImportCharacterAsync(id);
            if (success)
            {
                ImportedIds.Add(id);
                if (ActiveTab == Tab.Worlds)
                {
                    MyWorlds = await WorldClient.GetMyWorldsAsync();
                }
            }
            else
            {
                ErrorMessage = error;
            }
        }
        finally
        {
            ImportingId = null;
        }
    }

    private async Task ImportCampaign(int id, List<WorldDto> compatible)
    {
        ImportingId = id;
        ErrorMessage = null;
        try
        {
            var targetWorldId = GetTargetWorld(id, compatible);
            var (success, error) = await MarketClient.ImportCampaignAsync(id, targetWorldId);
            if (success)
            {
                ImportedIds.Add(id);
            }
            else
            {
                ErrorMessage = error;
            }
        }
        finally
        {
            ImportingId = null;
        }
    }
}
