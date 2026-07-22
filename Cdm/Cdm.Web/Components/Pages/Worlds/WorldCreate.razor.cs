using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Extensions;
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

    private static IReadOnlyList<(GameType Value, string Label, string Icon)> GameTypeOptions
        => GameTypeExtensions.Picker;

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

}
