using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace Cdm.Web.Components.Pages.Characters;

/// <summary>
/// Character detail page code-behind.
/// </summary>
public partial class Detail : ComponentBase
{
    [Parameter]
    public int Id { get; set; }

    [Inject]
    private ICharacterApiClient CharacterApi { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ILogger<Detail> Logger { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the character.
    /// </summary>
    private CharacterDto? Character { get; set; }

    /// <summary>
    /// Gets or sets whether the page is loading.
    /// </summary>
    private bool IsLoading { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    private string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets whether a delete operation is in progress.
    /// </summary>
    private bool IsDeleting { get; set; }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await this.LoadCharacterAsync();
    }

    /// <summary>
    /// Loads the character from the API.
    /// </summary>
    private async Task LoadCharacterAsync()
    {
        try
        {
            this.IsLoading = true;
            this.ErrorMessage = null;

            this.Character = await this.CharacterApi.GetCharacterByIdAsync(this.Id);

            if (this.Character == null)
            {
                this.ErrorMessage = "Personnage introuvable.";
            }
            else
            {
                this.Logger.LogInformation("Loaded character: {CharacterId}", this.Character.Id);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error loading character {CharacterId}", this.Id);
            this.ErrorMessage = "Impossible de charger le personnage. Veuillez reessayer.";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Shows the delete confirmation dialog.
    /// </summary>
    private async Task ConfirmDelete()
    {
        var dialog = await this.DialogService.ShowConfirmationAsync(
            $"Etes-vous sur de vouloir supprimer le personnage {GetCharacterFullName()} ? Cette action est irreversible.",
            "Supprimer",
            "Annuler",
            "Confirmer la suppression");

        var result = await dialog.Result;
        
        if (!result.Cancelled)
        {
            await this.DeleteCharacterAsync();
        }
    }

    /// <summary>
    /// Deletes the character.
    /// </summary>
    private async Task DeleteCharacterAsync()
    {
        try
        {
            this.IsDeleting = true;
            StateHasChanged();

            var success = await this.CharacterApi.DeleteCharacterAsync(this.Id);

            if (success)
            {
                this.Logger.LogInformation("Character {CharacterId} deleted successfully", this.Id);
                this.Navigation.NavigateTo("/characters");
            }
            else
            {
                this.ErrorMessage = "Impossible de supprimer le personnage. Veuillez reessayer.";
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting character {CharacterId}", this.Id);
            this.ErrorMessage = "Une erreur est survenue lors de la suppression.";
        }
        finally
        {
            this.IsDeleting = false;
        }
    }

    /// <summary>
    /// Gets the full name of the character.
    /// </summary>
    /// <returns>The full name.</returns>
    private string GetCharacterFullName()
    {
        if (this.Character == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(this.Character.FirstName))
        {
            return $"{this.Character.FirstName} {this.Character.Name}";
        }

        return this.Character.Name;
    }
}
