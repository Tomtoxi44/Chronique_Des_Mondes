namespace Cdm.Web.Shared.DTOs.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Demande d'envoi d'un lien de réinitialisation (couche Web).
/// </summary>
public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "L'adresse e-mail est requise")]
    [EmailAddress(ErrorMessage = "Format d'adresse e-mail invalide")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Définition d'un nouveau mot de passe depuis un jeton (couche Web).
/// </summary>
public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Le mot de passe doit contenir au moins 1 majuscule, 1 minuscule, 1 chiffre et 1 caractère spécial")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
    [Compare(nameof(NewPassword), ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
