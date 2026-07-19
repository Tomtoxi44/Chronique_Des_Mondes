using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Auth;

public partial class ConfirmEmail
{
    [Inject] private IAuthApiClient AuthClient { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "token")]
    private string? Token { get; set; }

    private bool IsLoading = true;
    private bool IsSuccess = false;
    private string ErrorMessage = "Ce lien de confirmation est invalide ou a expiré.";

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "Ce lien de confirmation est incomplet.";
            IsLoading = false;
            return;
        }

        try
        {
            IsSuccess = await AuthClient.ConfirmEmailAsync(Token);

            // Si l'utilisateur est connecté dans ce navigateur, on rafraîchit son
            // état pour faire disparaître le bandeau immédiatement.
            if (IsSuccess && AuthProvider is CustomAuthStateProvider provider)
            {
                await provider.MarkEmailConfirmedAsync();
            }
        }
        catch
        {
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
