using Microsoft.AspNetCore.Components;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.ApiClients.Base;
using Cdm.Web.Services.State;
using Cdm.Web.Shared.DTOs.Auth;

namespace Cdm.Web.Components.Pages.Auth;

public partial class Register
{
    [Inject] private IAuthApiClient AuthApiClient { get; set; } = default!;
    [Inject] private CustomAuthStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private ILogger<Register> Logger { get; set; } = default!;
    
    private RegisterRequest RegisterModel { get; set; } = new();
    private string? ErrorMessage { get; set; }
    private bool IsLoading { get; set; }
    private bool ShowPassword { get; set; }
    
    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }
    
    private async Task HandleRegisterAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            
            Logger.LogInformation("Attempting to register user: {Email}", RegisterModel.Email);
            
            var response = await AuthApiClient.RegisterAsync(RegisterModel);
            
            if (response != null)
            {
                Logger.LogInformation("Registration successful for: {Email}", response.Email);
                
                // Marquer l'utilisateur comme authentifié
                await AuthStateProvider.MarkUserAsAuthenticatedAsync(
                    response.UserId,
                    response.Email,
                    response.Token);
                
                // Rediriger vers le dashboard avec forceLoad pour rafraîchir le contexte d'authentification
                NavigationManager.NavigateTo("/characters", forceLoad: true);
            }
        }
        catch (ApiException ex)
        {
            Logger.LogError(ex, "Registration failed for: {Email}", RegisterModel.Email);
            
            if (ex.ValidationErrors != null && ex.ValidationErrors.Any())
            {
                // Afficher la première erreur de validation
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
            ErrorMessage = "Une erreur inattendue s'est produite. Veuillez réessayer.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
