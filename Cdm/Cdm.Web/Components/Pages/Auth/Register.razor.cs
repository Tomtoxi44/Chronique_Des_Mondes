using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.State;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Auth;

public partial class Register
{
    [Inject] private IAuthApiClient AuthClient { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;
    [Inject] private Cdm.Web.Services.ToastService Toast { get; set; } = default!;

    private RegisterRequest Model { get; set; } = new();
    private bool IsLoading = false;
    private bool RegisterSuccess = false;
    private string ErrorMessage = string.Empty;
    private string SuccessMessage = string.Empty;

    private async Task HandleRegister()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await AuthClient.RegisterAsync(Model);
            if (response != null)
            {
                SuccessMessage = L["Auth_RegisterSuccess"];
                RegisterSuccess = true;

                // L'API renvoie déjà un jeton : on connecte directement l'utilisateur
                // au lieu de le renvoyer vers l'écran de connexion.
                if (!string.IsNullOrEmpty(response.Token) &&
                    AuthProvider is CustomAuthStateProvider provider)
                {
                    await provider.MarkUserAsAuthenticatedAsync(
                        response.UserId, response.Email, response.Nickname, response.Token,
                        response.RefreshToken, response.RefreshTokenExpiry);

                    // Le ToastService est scopé au circuit : le toast reste affiché
                    // après la navigation vers le tableau de bord.
                    Toast.ShowSuccess(
                        $"Bienvenue, {response.Nickname} ! Votre compte a bien été créé.",
                        "Inscription réussie");

                    Nav.NavigateTo("/");
                    return;
                }

                // Repli : ancien comportement si le jeton est absent.
                await Task.Delay(2000);
                Nav.NavigateTo("/login");
            }
            else
            {
                ErrorMessage = L["Auth_RegisterError"];
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = L["Auth_RegisterError"];
        }
        finally
        {
            IsLoading = false;
        }
    }
}
