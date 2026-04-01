namespace Cdm.Web.Shared.DTOs.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Character data transfer object
/// </summary>
public class CharacterDto
{
    /// <summary>
    /// Gets or sets the character ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the character's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the character's description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the character's age
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new character
/// </summary>
public class CreateCharacterDto
{
    /// <summary>
    /// Gets or sets the character's name (required)
    /// </summary>
    [Required(ErrorMessage = "Le nom est requis")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character's first name
    /// </summary>
    [MaxLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the character's description
    /// </summary>
    [MaxLength(2000, ErrorMessage = "La description ne peut pas dépasser 2000 caractères")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the character's age
    /// </summary>
    [Range(0, 10000, ErrorMessage = "L'âge doit être entre 0 et 10000")]
    public int? Age { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL
    /// </summary>
    [Url(ErrorMessage = "L'URL de l'avatar n'est pas valide")]
    [MaxLength(500, ErrorMessage = "L'URL ne peut pas dépasser 500 caractères")]
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// DTO for updating an existing character
/// </summary>
public class UpdateCharacterDto
{
    /// <summary>
    /// Gets or sets the character's name
    /// </summary>
    [Required(ErrorMessage = "Le nom est requis")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character's first name
    /// </summary>
    [MaxLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the character's description
    /// </summary>
    [MaxLength(2000, ErrorMessage = "La description ne peut pas dépasser 2000 caractères")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the character's age
    /// </summary>
    [Range(0, 10000, ErrorMessage = "L'âge doit être entre 0 et 10000")]
    public int? Age { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL
    /// </summary>
    [Url(ErrorMessage = "L'URL de l'avatar n'est pas valide")]
    [MaxLength(500, ErrorMessage = "L'URL ne peut pas dépasser 500 caractères")]
    public string? AvatarUrl { get; set; }
}
