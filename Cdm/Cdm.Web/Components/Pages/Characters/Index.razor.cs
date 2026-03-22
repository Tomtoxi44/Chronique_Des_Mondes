using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;

namespace Cdm.Web.Components.Pages.Characters;

/// <summary>
/// Characters list page code-behind.
/// </summary>
public partial class Index : ComponentBase
{
    [Inject]
    private ICharacterApiClient CharacterApi { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ILogger<Index> Logger { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of characters.
    /// </summary>
    private IEnumerable<CharacterDto>? Characters { get; set; }

    /// <summary>
    /// Gets or sets whether the page is loading.
    /// </summary>
    private bool IsLoading { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    private string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets whether the delete modal is shown.
    /// </summary>
    private bool ShowDeleteModal { get; set; }

    /// <summary>
    /// Gets or sets the character to delete.
    /// </summary>
    private CharacterDto? CharacterToDelete { get; set; }

    /// <summary>
    /// Gets or sets whether a delete operation is in progress.
    /// </summary>
    private bool IsDeleting { get; set; }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await this.LoadCharactersAsync();
    }

    /// <summary>
    /// Loads the characters from the API.
    /// </summary>
    private async Task LoadCharactersAsync()
    {
        try
        {
            this.IsLoading = true;
            this.ErrorMessage = null;

            this.Characters = await this.CharacterApi.GetMyCharactersAsync();

            this.Logger.LogInformation("Loaded {Count} characters", this.Characters?.Count() ?? 0);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error loading characters");
            this.ErrorMessage = "Impossible de charger vos personnages. Veuillez réessayer.";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Navigates to the character detail page.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    private void ViewCharacter(int characterId)
    {
        this.Navigation.NavigateTo($"/characters/{characterId}");
    }

    /// <summary>
    /// Navigates to the character edit page.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    private void EditCharacter(int characterId)
    {
        this.Navigation.NavigateTo($"/characters/{characterId}/edit");
    }

    /// <summary>
    /// Shows the delete confirmation modal.
    /// </summary>
    /// <param name="character">The character to delete.</param>
    private void ConfirmDelete(CharacterDto character)
    {
        this.CharacterToDelete = character;
        this.ShowDeleteModal = true;
    }

    /// <summary>
    /// Cancels the delete operation.
    /// </summary>
    private void CancelDelete()
    {
        this.CharacterToDelete = null;
        this.ShowDeleteModal = false;
    }

    /// <summary>
    /// Deletes the character.
    /// </summary>
    private async Task DeleteCharacterAsync()
    {
        if (this.CharacterToDelete == null)
        {
            return;
        }

        try
        {
            this.IsDeleting = true;

            var success = await this.CharacterApi.DeleteCharacterAsync(this.CharacterToDelete.Id);

            if (success)
            {
                this.Logger.LogInformation("Character {CharacterId} deleted successfully", this.CharacterToDelete.Id);
                
                // Reload the list
                await this.LoadCharactersAsync();
            }
            else
            {
                this.ErrorMessage = "Impossible de supprimer le personnage. Veuillez réessayer.";
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting character {CharacterId}", this.CharacterToDelete.Id);
            this.ErrorMessage = "Une erreur est survenue lors de la suppression.";
        }
        finally
        {
            this.IsDeleting = false;
            this.ShowDeleteModal = false;
            this.CharacterToDelete = null;
        }
    }

    /// <summary>
    /// Truncates a description to a maximum length.
    /// </summary>
    /// <param name="description">The description to truncate.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns>The truncated description.</returns>
    private static string TruncateDescription(string description, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(description) || description.Length <= maxLength)
        {
            return description;
        }

        return description[..maxLength] + "...";
    }

    /// <summary>
    /// Gets the full name of a character.
    /// </summary>
    /// <param name="character">The character.</param>
    /// <returns>The full name.</returns>
    private static string GetCharacterFullName(CharacterDto character)
    {
        if (!string.IsNullOrEmpty(character.FirstName))
        {
            return $"{character.FirstName} {character.Name}";
        }

        return character.Name;
    }
}
