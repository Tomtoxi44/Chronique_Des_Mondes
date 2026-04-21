using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.State;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Auth;

public partial class Login
{
    [Inject] private IAuthApiClient AuthClient { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "returnUrl")]
    private string? ReturnUrl { get; set; }

    private LoginRequest Model { get; set; } = new();
    private bool IsLoading = false;
    private string ErrorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthProvider.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated == true)
            Nav.NavigateTo("/");
    }

    private async Task HandleLogin()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await AuthClient.LoginAsync(Model);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                var provider = (CustomAuthStateProvider)AuthProvider;
                await provider.MarkUserAsAuthenticatedAsync(
                    response.UserId, response.Email, response.Nickname, response.Token,
                    response.RefreshToken, response.RefreshTokenExpiry);

                var target = !string.IsNullOrEmpty(ReturnUrl)
                    ? Uri.UnescapeDataString(ReturnUrl)
                    : "/";
                Nav.NavigateTo(target);
            }
            else
            {
                ErrorMessage = L["Auth_LoginError"];
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = L["Auth_LoginError"];
        }
        finally
        {
            IsLoading = false;
        }
    }
}
