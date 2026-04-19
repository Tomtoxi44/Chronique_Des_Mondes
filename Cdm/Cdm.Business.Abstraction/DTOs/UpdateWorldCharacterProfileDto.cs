namespace Cdm.Business.Abstraction.DTOs;

/// <summary>
/// DTO for updating a world character's game profile.
/// </summary>
public class UpdateWorldCharacterProfileDto
{
    /// <summary>
    /// Gets or sets the character level (if applicable).
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// Gets or sets the current health points.
    /// </summary>
    public int? CurrentHealth { get; set; }

    /// <summary>
    /// Gets or sets the maximum health points.
    /// </summary>
    public int? MaxHealth { get; set; }

    /// <summary>
    /// Gets or sets the game-specific data as a JSON string.
    /// For D&amp;D: {"Race":"Human","Class":"Fighter","Strength":18,...}
    /// For Skyrim: {"Race":"Nord","Health":200,"Magicka":150,...}
    /// </summary>
    public string? GameSpecificData { get; set; }
}
