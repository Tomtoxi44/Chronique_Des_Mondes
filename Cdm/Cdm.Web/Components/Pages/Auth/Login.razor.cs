using Microsoft.AspNetCore.Components;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.State;
using Cdm.Web.Shared.DTOs.Auth;

namespace Cdm.Web.Components.Pages.Auth;

public partial class Login
{
    [Inject] private IAuthApiClient AuthApiClient { get; set; } = default!;
    [Inject] private CustomAuthStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    
    private LoginRequest LoginModel { get; set; } = new();
    private string? ErrorMessage { get; set; }
    private bool IsLoading { get; set; }
    private bool ShowPassword { get; set; }
    
    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }
    
    private async Task HandleLoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            
            Logger.LogInformation("Attempting to login user: {Email}", LoginModel.Email);
            
            var response = await AuthApiClient.LoginAsync(LoginModel);
            
            if (response != null)
            {
                Logger.LogInformation("Login successful for: {Email}", response.Email);
                
                // Marquer l'utilisateur comme authentifié
                await AuthStateProvider.MarkUserAsAuthenticatedAsync(
                    response.UserId,
                    response.Email,
                    response.Token);
                
                // Petit délai pour s'assurer que le localStorage est synchronisé
                await Task.Delay(100);
                
                // Rediriger vers le dashboard avec forceLoad pour rafraîchir le contexte d'authentification
                NavigationManager.NavigateTo("/characters", forceLoad: true);
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
            ErrorMessage = "Une erreur inattendue s'est produite. Veuillez réessayer.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
