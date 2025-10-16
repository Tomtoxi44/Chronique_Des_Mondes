using Microsoft.AspNetCore.Components;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Shared.DTOs.Auth;

namespace Cdm.Web.Components.Pages.Auth;

/// <summary>
/// Registration component for new user accounts.
/// </summary>
public partial class Register
{
    [Inject] private RegisterHandler Handler { get; set; } = default!;
    [Inject] private ILogger<Register> Logger { get; set; } = default!;
    
    private RegisterRequest RegisterModel { get; set; } = new();
    private string? ErrorMessage { get; set; }
    private bool IsLoading { get; set; }
    private bool ShowPassword { get; set; }
    
    /// <summary>
    /// Toggles password visibility.
    /// </summary>
    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }
    
    /// <summary>
    /// Handles the registration form submission.
    /// </summary>
    private async Task HandleRegisterAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            
            var response = await Handler.HandleRegisterAsync(RegisterModel);
            
            if (response != null)
            {
                // Wait a bit longer to ensure auth state is fully propagated
                await Task.Delay(300);
                NavigationManager.NavigateTo("/characters", forceLoad: false);
            }
        }
        catch (ApiException ex)
        {
            Logger.LogError(ex, "Registration failed for: {Email}", RegisterModel.Email);
            
            if (ex.ValidationErrors != null && ex.ValidationErrors.Any())
            {
                var firstError = ex.ValidationErrors.First();
                ErrorMessage = $"{firstError.Key}: {string.Join(", ", firstError.Value)}";
            }
            else
            {
                ErrorMessage = ex.Message;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during registration");
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
