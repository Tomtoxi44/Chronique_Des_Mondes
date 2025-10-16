using Microsoft.AspNetCore.Components;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Shared.DTOs.Auth;

namespace Cdm.Web.Components.Pages.Auth;

/// <summary>
/// Login component for user authentication.
/// </summary>
public partial class Login
{
    [Inject] private LoginHandler Handler { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    
    private LoginRequest LoginModel { get; set; } = new();
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
    /// Handles the login form submission.
    /// </summary>
    private async Task HandleLoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            
            var response = await Handler.HandleLoginAsync(LoginModel);
            
            if (response != null)
            {
                // Wait a bit longer to ensure auth state is fully propagated
                await Task.Delay(300);
                NavigationManager.NavigateTo("/characters", forceLoad: false);
            }
        }
        catch (ApiException ex)
        {
            Logger.LogError(ex, "Login failed for: {Email}", LoginModel.Email);
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during login");
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
