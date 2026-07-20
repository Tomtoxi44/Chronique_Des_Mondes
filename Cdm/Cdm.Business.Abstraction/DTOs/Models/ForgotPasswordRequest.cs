namespace Cdm.Business.Abstraction.DTOs.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Demande d'envoi d'un lien de réinitialisation de mot de passe.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Gets or sets the account email address.
    /// </summary>
    [Required(ErrorMessage = "L'adresse e-mail est requise")]
    [EmailAddress(ErrorMessage = "Format d'adresse e-mail invalide")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
}
