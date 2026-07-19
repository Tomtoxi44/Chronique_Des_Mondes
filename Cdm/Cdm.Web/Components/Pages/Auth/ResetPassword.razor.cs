using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Auth;

public partial class ResetPassword
{
    [Inject] private IAuthApiClient AuthClient { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    /// <summary>Jeton de réinitialisation transmis dans l'URL.</summary>
    [SupplyParameterFromQuery(Name = "token")]
    private string? Token { get; set; }

    private ResetPasswordRequest Model { get; set; } = new();
    private bool IsLoading = false;
    private bool IsSuccess = false;
    private string ErrorMessage = string.Empty;

    private async Task HandleSubmit()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "Ce lien de réinitialisation est incomplet.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        Model.Token = Token;

        try
        {
            var ok = await AuthClient.ResetPasswordAsync(Model);

            if (ok)
            {
                IsSuccess = true;
            }
            else
            {
                ErrorMessage = "Ce lien de réinitialisation est invalide ou a expiré.";
            }
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
