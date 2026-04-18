using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Auth;

public partial class Register
{
    [Inject] private IAuthApiClient AuthClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

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
