using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Auth;

public partial class ForgotPassword
{
    [Inject] private IAuthApiClient AuthClient { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private ForgotPasswordRequest Model { get; set; } = new();
    private bool IsLoading = false;
    private bool IsSubmitted = false;
    private string ErrorMessage = string.Empty;

    private async Task HandleSubmit()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            await AuthClient.ForgotPasswordAsync(Model);

            // On affiche la même confirmation quoi qu'il arrive : indiquer que
            // l'adresse est inconnue permettrait d'énumérer les comptes.
            IsSubmitted = true;
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = "Une erreur est survenue. Merci de réessayer dans quelques instants.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
