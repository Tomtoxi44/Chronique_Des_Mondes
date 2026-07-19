namespace Cdm.Business.Abstraction.DTOs.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Confirmation d'une adresse email à partir d'un jeton reçu par email.
/// </summary>
public class ConfirmEmailRequest
{
    /// <summary>
    /// Gets or sets the confirmation token received by email.
    /// </summary>
    [Required(ErrorMessage = "Le jeton de confirmation est requis")]
    public string Token { get; set; } = string.Empty;
}
