namespace Cdm.Business.Abstraction.DTOs.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Définition d'un nouveau mot de passe à partir d'un jeton de réinitialisation.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Gets or sets the reset token received by email.
    /// </summary>
    [Required(ErrorMessage = "Le jeton de réinitialisation est requis")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// Même politique que l'inscription : 8 caractères minimum,
    /// 1 majuscule, 1 minuscule, 1 chiffre et 1 caractère spécial.
    /// </summary>
    [Required(ErrorMessage = "Le mot de passe est requis")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Le mot de passe doit contenir au moins 1 majuscule, 1 minuscule, 1 chiffre et 1 caractère spécial")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password confirmation.
    /// </summary>
    [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
    [Compare(nameof(NewPassword), ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
