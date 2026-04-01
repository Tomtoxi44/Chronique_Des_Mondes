using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
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
    /// Gets or sets whether the delete modal is shown.
    /// </summary>
    private bool ShowDeleteModal { get; set; }

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
            this.ErrorMessage = "Impossible de charger le personnage. Veuillez réessayer.";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Shows the delete confirmation modal.
    /// </summary>
    private void ConfirmDelete()
    {
        this.ShowDeleteModal = true;
    }

    /// <summary>
    /// Cancels the delete operation.
    /// </summary>
    private void CancelDelete()
    {
        this.ShowDeleteModal = false;
    }

    /// <summary>
    /// Deletes the character.
    /// </summary>
    private async Task DeleteCharacterAsync()
    {
        try
        {
            this.IsDeleting = true;

            var success = await this.CharacterApi.DeleteCharacterAsync(this.Id);

            if (success)
            {
                this.Logger.LogInformation("Character {CharacterId} deleted successfully", this.Id);
                this.Navigation.NavigateTo("/characters");
            }
            else
            {
                this.ErrorMessage = "Impossible de supprimer le personnage. Veuillez réessayer.";
                this.ShowDeleteModal = false;
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting character {CharacterId}", this.Id);
            this.ErrorMessage = "Une erreur est survenue lors de la suppression.";
            this.ShowDeleteModal = false;
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

    /// <summary>
    /// Formats the description with line breaks.
    /// </summary>
    /// <param name="description">The description to format.</param>
    /// <returns>The formatted description with HTML line breaks.</returns>
    private static string FormatDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
        {
            return string.Empty;
        }

        // Escape HTML characters first
        var escaped = System.Web.HttpUtility.HtmlEncode(description);
        
        // Replace newlines with <br> tags
        return escaped.Replace("\n", "<br />");
    }
}
