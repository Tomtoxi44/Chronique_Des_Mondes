using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Cdm.Web.Components.Pages.Characters;

/// <summary>
/// Character creation page code-behind.
/// </summary>
public partial class Create : ComponentBase
{
    [Inject]
    private ICharacterApiClient CharacterApi { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ILogger<Create> Logger { get; set; } = default!;

    /// <summary>
    /// Gets or sets the form model.
    /// </summary>
    private CharacterFormModel FormModel { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the form is being submitted.
    /// </summary>
    private bool IsSubmitting { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    private string? ErrorMessage { get; set; }

    /// <summary>
    /// Handles the form submission.
    /// </summary>
    private async Task HandleSubmitAsync()
    {
        try
        {
            this.IsSubmitting = true;
            this.ErrorMessage = null;

            var request = new CreateCharacterDto
            {
                Name = this.FormModel.Name,
                FirstName = this.FormModel.FirstName,
                Description = this.FormModel.Description,
                Age = this.FormModel.Age,
                AvatarUrl = this.FormModel.AvatarUrl
            };

            this.Logger.LogInformation("Creating character: {Name}", request.Name);

            var result = await this.CharacterApi.CreateCharacterAsync(request);

            if (result != null)
            {
                this.Logger.LogInformation("Character created successfully: {CharacterId}", result.Id);
                this.Navigation.NavigateTo($"/characters/{result.Id}");
            }
            else
            {
                this.ErrorMessage = "Impossible de créer le personnage. Veuillez réessayer.";
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error creating character");
            this.ErrorMessage = "Une erreur est survenue lors de la création du personnage.";
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
    /// Form model for character creation.
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
