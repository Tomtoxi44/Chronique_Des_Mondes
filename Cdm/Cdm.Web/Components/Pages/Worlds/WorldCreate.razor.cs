using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Worlds;

public partial class WorldCreate
{
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private CreateWorldModel Model { get; set; } = new();
    private bool IsLoading = false;
    private string ErrorMessage = string.Empty;

    private List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem(L["Worlds_Title"], "/worlds"),
        new BreadcrumbItem(L["Worlds_Create"])
    };

    private readonly List<GameTypeOption> GameTypeOptions = new()
    {
        new(GameType.Generic,      "Générique",         "bi-globe2"),
        new(GameType.DnD5e,        "D&D 5e",            "bi-shield-fill"),
        new(GameType.Pathfinder,   "Pathfinder",        "bi-shield-fill-check"),
        new(GameType.CallOfCthulhu,"L'Appel de Cthulhu","bi-eye-fill"),
        new(GameType.Warhammer,    "Warhammer",         "bi-hammer"),
        new(GameType.Cyberpunk,    "Cyberpunk",         "bi-cpu-fill"),
        new(GameType.Skyrim,       "Skyrim",            "bi-snow2"),
        new(GameType.Custom,       "Personnalisé",      "bi-stars"),
    };

    private async Task HandleCreate()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var request = new CreateWorldRequest(Model.Name, Model.Description ?? string.Empty, Model.GameType);
            var result = await WorldClient.CreateWorldAsync(request);
            if (result != null)
                Nav.NavigateTo($"/worlds/{result.Id}");
            else
                ErrorMessage = L["Common_ErrorMessage"];
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = L["Common_ErrorMessage"];
        }
        finally
        {
            IsLoading = false;
        }
    }

    private class CreateWorldModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Le nom est requis")]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public GameType GameType { get; set; } = GameType.Generic;
    }

    private record GameTypeOption(GameType Value, string Label, string Icon);
}
