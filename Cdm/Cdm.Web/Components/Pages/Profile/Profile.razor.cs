using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
using Cdm.Web.Resources;
using Cdm.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Profile;

public partial class Profile
{
    [Inject] private ProfileApiClient ProfileClient { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private ProfileResponse? UserProfile;
    private UpdateProfileRequest UpdateModel { get; set; } = new();
    private bool IsLoading = true;
    private bool IsSaving = false;
    private string ErrorMessage = string.Empty;
    private string SuccessMessage = string.Empty;
    private string UserName = string.Empty;
    private string UserInitials = "?";

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        UserName = authState.User.Claims
            .FirstOrDefault(c => c.Type == "nickname")?.Value
            ?? authState.User.Identity?.Name
            ?? string.Empty;
        UserInitials = BuildInitials(UserName);

        try
        {
            UserProfile = await ProfileClient.GetProfileAsync();
            if (UserProfile != null)
            {
                UpdateModel = new UpdateProfileRequest { Username = UserProfile.Username };
            }
        }
        catch { /* ignore */ }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleSave()
    {
        IsSaving = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        try
        {
            var result = await ProfileClient.UpdateProfileAsync(UpdateModel);
            if (result != null)
            {
                UserProfile = result;
                SuccessMessage = L["Profile_SaveSuccess"];
            }
            else
            {
                ErrorMessage = L["Profile_SaveError"];
            }
        }
        catch
        {
            ErrorMessage = L["Profile_SaveError"];
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static string BuildInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }
}
