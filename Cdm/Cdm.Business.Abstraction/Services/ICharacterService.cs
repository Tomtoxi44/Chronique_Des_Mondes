namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Service for managing characters
/// </summary>
public interface ICharacterService
{
    /// <summary>
    /// Creates a new generic character
    /// </summary>
    /// <param name="request">The character creation request</param>
    /// <param name="userId">The user identifier of the character owner</param>
    /// <returns>The created character</returns>
    Task<CharacterDto?> CreateCharacterAsync(CreateCharacterDto request, int userId);

    /// <summary>
    /// Gets all characters owned by a specific user
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <returns>A list of characters owned by the user</returns>
    Task<IEnumerable<CharacterDto>> GetMyCharactersAsync(int userId);

    /// <summary>
    /// Gets a character by its identifier
    /// </summary>
    /// <param name="characterId">The character identifier</param>
    /// <param name="userId">The user identifier requesting the character</param>
    /// <returns>The character if found and authorized, null otherwise</returns>
    Task<CharacterDto?> GetCharacterByIdAsync(int characterId, int userId);

    /// <summary>
    /// Updates an existing character
    /// </summary>
    /// <param name="characterId">The character identifier</param>
    /// <param name="request">The update request</param>
    /// <param name="userId">The user identifier</param>
    /// <returns>The updated character if successful, null otherwise</returns>
    Task<CharacterDto?> UpdateCharacterAsync(int characterId, UpdateCharacterDto request, int userId);

    /// <summary>
    /// Deletes a character (soft delete by setting IsActive to false)
    /// </summary>
    /// <param name="characterId">The character identifier</param>
    /// <param name="userId">The user identifier</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteCharacterAsync(int characterId, int userId);

    /// <summary>
    /// Creates a game-specific profile for a character joining a campaign
    /// </summary>
    /// <param name="characterId">The character identifier</param>
    /// <param name="campaignId">The campaign identifier</param>
    /// <param name="gameProfileData">The game-specific profile data (D&D, Skyrim, etc.)</param>
    /// <param name="userId">The user identifier</param>
    /// <returns>True if profile created successfully, false otherwise</returns>
    Task<bool> CreateGameProfileAsync(int characterId, int campaignId, object gameProfileData, int userId);

    /// <summary>
    /// Gets the game-specific profile for a character in a campaign
    /// </summary>
    /// <param name="characterId">The character identifier</param>
    /// <param name="campaignId">The campaign identifier</param>
    /// <param name="userId">The user identifier</param>
    /// <returns>The game profile data if found, null otherwise</returns>
    Task<object?> GetGameProfileAsync(int characterId, int campaignId, int userId);

    /// <summary>
    /// Updates the game-specific profile for a character in a campaign
    /// </summary>
    /// <param name="characterId">The character identifier</param>
    /// <param name="campaignId">The campaign identifier</param>
    /// <param name="gameProfileData">The updated game-specific profile data</param>
    /// <param name="userId">The user identifier</param>
    /// <returns>True if updated successfully, false otherwise</returns>
    Task<bool> UpdateGameProfileAsync(int characterId, int campaignId, object gameProfileData, int userId);
}
