using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Cdm.Web.Components.Pages.Characters;

/// <summary>
/// Character edit page code-behind.
/// </summary>
public partial class Edit : ComponentBase
{
    [Inject]
    private ICharacterApiClient CharacterApi { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ILogger<Edit> Logger { get; set; } = default!;

    /// <summary>
    /// Gets or sets the character ID from the route.
    /// </summary>
    [Parameter]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the loaded character.
    /// </summary>
    private CharacterDto? Character { get; set; }

    /// <summary>
    /// Gets or sets the form model.
    /// </summary>
    private CharacterFormModel FormModel { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the page is loading.
    /// </summary>
    private bool IsLoading { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the form is being submitted.
    /// </summary>
    private bool IsSubmitting { get; set; }

    /// <summary>
    /// Gets or sets the error message for form submission.
    /// </summary>
    private string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the error message for initial load.
    /// </summary>
    private string? LoadErrorMessage { get; set; }

    /// <summary>
    /// Initializes the component and loads the character.
    /// </summary>
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
            this.LoadErrorMessage = null;

            this.Logger.LogInformation("Loading character for edit: {CharacterId}", this.Id);

            this.Character = await this.CharacterApi.GetCharacterByIdAsync(this.Id);

            if (this.Character != null)
            {
                // Populate the form model with existing data
                this.FormModel = new CharacterFormModel
                {
                    Name = this.Character.Name,
                    FirstName = this.Character.FirstName,
                    Description = this.Character.Description,
                    Age = this.Character.Age,
                    AvatarUrl = this.Character.AvatarUrl
                };

                this.Logger.LogInformation("Character loaded for edit: {CharacterName}", this.Character.Name);
            }
            else
            {
                this.LoadErrorMessage = "Personnage introuvable.";
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error loading character {CharacterId} for edit", this.Id);
            this.LoadErrorMessage = "Une erreur est survenue lors du chargement du personnage.";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Handles the form submission.
    /// </summary>
    private async Task HandleSubmitAsync()
    {
        try
        {
            this.IsSubmitting = true;
            this.ErrorMessage = null;

            var request = new UpdateCharacterDto
            {
                Name = this.FormModel.Name,
                FirstName = this.FormModel.FirstName,
                Description = this.FormModel.Description,
                Age = this.FormModel.Age,
                AvatarUrl = this.FormModel.AvatarUrl
            };

            this.Logger.LogInformation("Updating character {CharacterId}: {Name}", this.Id, request.Name);

            var result = await this.CharacterApi.UpdateCharacterAsync(this.Id, request);

            if (result != null)
            {
                this.Logger.LogInformation("Character updated successfully: {CharacterId}", result.Id);
                this.Navigation.NavigateTo($"/characters/{result.Id}");
            }
            else
            {
                this.ErrorMessage = "Impossible de mettre à jour le personnage. Veuillez réessayer.";
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating character {CharacterId}", this.Id);
            this.ErrorMessage = "Une erreur est survenue lors de la mise à jour du personnage.";
        }
        finally
        {
            this.IsSubmitting = false;
        }
    }

    /// <summary>
    /// Handles image loading errors.
    /// </summary>
    private void HandleImageError()
    {
        // Clear the avatar URL if the image fails to load
        this.FormModel.AvatarUrl = null;
        this.StateHasChanged();
    }

    /// <summary>
    /// Form model for character editing.
    /// </summary>
    public class CharacterFormModel
    {
        /// <summary>
        /// Gets or sets the character's name (required).
        /// </summary>
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the character's first name.
        /// </summary>
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the character's description.
        /// </summary>
        [StringLength(2000, ErrorMessage = "La description ne peut pas dépasser 2000 caractères")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the character's age.
        /// </summary>
        [Range(0, 10000, ErrorMessage = "L'âge doit être compris entre 0 et 10000")]
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the avatar URL.
        /// </summary>
        [Url(ErrorMessage = "L'URL de l'avatar n'est pas valide")]
        public string? AvatarUrl { get; set; }
    }
}
