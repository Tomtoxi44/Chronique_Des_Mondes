// -----------------------------------------------------------------------
// <copyright file="CharacterService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Business.Common.Models;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing character operations.
/// </summary>
/// <param name="dbContext">Database context for character data access.</param>
/// <param name="logger">Logger instance for structured logging.</param>
public class CharacterService(
    AppDbContext dbContext,
    ILogger<CharacterService> logger) : ICharacterService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<CharacterService> logger = logger;

    /// <inheritdoc/>
    public async Task<CharacterDto?> CreateCharacterAsync(CreateCharacterDto dto, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Creating character '{CharacterName}' for user {UserId}",
                dto.Name,
                userId);

            // Create character entity
            var character = new Character
            {
                UserId = userId,
                Name = dto.Name,
                FirstName = dto.FirstName,
                Description = dto.Description,
                Age = dto.Age,
                AvatarUrl = dto.AvatarUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            this.dbContext.Characters.Add(character);
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Character '{CharacterName}' created successfully with ID {CharacterId}",
                character.Name,
                character.Id);

            return MapToDto(character);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error creating character for user {UserId}",
                userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CharacterDto>> GetMyCharactersAsync(int userId)
    {
        try
        {
            this.logger.LogInformation("Fetching characters for user {UserId}", userId);

            var characters = await this.dbContext.Characters
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            this.logger.LogInformation(
                "Found {Count} characters for user {UserId}",
                characters.Count,
                userId);

            return characters.Select(MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error fetching characters for user {UserId}",
                userId);
            return Enumerable.Empty<CharacterDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<CharacterDto?> GetCharacterByIdAsync(int characterId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Fetching character {CharacterId} for user {UserId}",
                characterId,
                userId);

            var character = await this.dbContext.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId && c.IsActive);

            if (character == null)
            {
                this.logger.LogWarning(
                    "Character {CharacterId} not found or user {UserId} not authorized",
                    characterId,
                    userId);
                return null;
            }

            return MapToDto(character);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error fetching character {CharacterId} for user {UserId}",
                characterId,
                userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CharacterDto?> UpdateCharacterAsync(int characterId, UpdateCharacterDto dto, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Updating character {CharacterId} for user {UserId}",
                characterId,
                userId);

            var character = await this.dbContext.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId && c.IsActive);

            if (character == null)
            {
                this.logger.LogWarning(
                    "Character {CharacterId} not found or user {UserId} not authorized",
                    characterId,
                    userId);
                return null;
            }

            // Update fields
            character.Name = dto.Name;
            character.FirstName = dto.FirstName;
            character.Description = dto.Description;
            character.Age = dto.Age;
            character.AvatarUrl = dto.AvatarUrl;
            character.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Character {CharacterId} updated successfully",
                characterId);

            return MapToDto(character);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error updating character {CharacterId} for user {UserId}",
                characterId,
                userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteCharacterAsync(int characterId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Deleting character {CharacterId} for user {UserId}",
                characterId,
                userId);

            var character = await this.dbContext.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId && c.IsActive);

            if (character == null)
            {
                this.logger.LogWarning(
                    "Character {CharacterId} not found or user {UserId} not authorized",
                    characterId,
                    userId);
                return false;
            }

            // Soft delete
            character.IsActive = false;
            character.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Character {CharacterId} deleted successfully",
                characterId);

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error deleting character {CharacterId} for user {UserId}",
                characterId,
                userId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CreateGameProfileAsync(int characterId, int campaignId, object gameProfileData, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Creating game profile for character {CharacterId} in campaign {CampaignId}",
                characterId,
                campaignId);

            // Verify character ownership
            var character = await this.dbContext.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId && c.IsActive);

            if (character == null)
            {
                this.logger.LogWarning(
                    "Character {CharacterId} not found or user {UserId} not authorized",
                    characterId,
                    userId);
                return false;
            }

            // Get campaign to determine game type
            var campaign = await this.dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == campaignId && c.IsActive);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return false;
            }

            // Check if profile already exists
            var existingProfile = await this.dbContext.CharacterGameProfiles
                .FirstOrDefaultAsync(p => p.CharacterId == characterId && p.CampaignId == campaignId);

            if (existingProfile != null)
            {
                this.logger.LogWarning(
                    "Game profile already exists for character {CharacterId} in campaign {CampaignId}",
                    characterId,
                    campaignId);
                return false;
            }

            // Serialize game profile data
            var gameSpecificDataJson = CharacterProfileMapper.SerializeGameProfile(gameProfileData, campaign.GameType);

            if (gameSpecificDataJson == null)
            {
                this.logger.LogWarning(
                    "Failed to serialize game profile data for game type {GameType}",
                    campaign.GameType);
                return false;
            }

            // Extract common fields based on game type
            int? level = null;
            int? currentHealth = null;
            int? maxHealth = null;

            if (gameProfileData is DndCharacterProfile dndProfile)
            {
                level = dndProfile.Level;
                currentHealth = dndProfile.CurrentHitPoints;
                maxHealth = dndProfile.MaxHitPoints;
            }
            else if (gameProfileData is SkyrimCharacterProfile skyrimProfile)
            {
                level = skyrimProfile.Level;
                currentHealth = skyrimProfile.CurrentHealth;
                maxHealth = skyrimProfile.MaxHealth;
            }

            // Create game profile
            var gameProfile = new CharacterGameProfile
            {
                CharacterId = characterId,
                CampaignId = campaignId,
                GameType = campaign.GameType,
                GameSpecificData = gameSpecificDataJson,
                Level = level,
                CurrentHealth = currentHealth,
                MaxHealth = maxHealth,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            this.dbContext.CharacterGameProfiles.Add(gameProfile);
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Game profile created successfully for character {CharacterId} in campaign {CampaignId}",
                characterId,
                campaignId);

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error creating game profile for character {CharacterId} in campaign {CampaignId}",
                characterId,
                campaignId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<object?> GetGameProfileAsync(int characterId, int campaignId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Fetching game profile for character {CharacterId} in campaign {CampaignId}",
                characterId,
                campaignId);

            // Verify character ownership
            var character = await this.dbContext.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId && c.IsActive);

            if (character == null)
            {
                this.logger.LogWarning(
                    "Character {CharacterId} not found or user {UserId} not authorized",
                    characterId,
                    userId);
                return null;
            }

            var gameProfile = await this.dbContext.CharacterGameProfiles
                .FirstOrDefaultAsync(p => p.CharacterId == characterId && p.CampaignId == campaignId && p.IsActive);

            if (gameProfile == null)
            {
                this.logger.LogWarning(
                    "Game profile not found for character {CharacterId} in campaign {CampaignId}",
                    characterId,
                    campaignId);
                return null;
            }

            // Deserialize to typed profile
            var profileData = CharacterProfileMapper.DeserializeGameProfile(
                gameProfile.GameSpecificData,
                gameProfile.GameType);

            return profileData;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error fetching game profile for character {CharacterId} in campaign {CampaignId}",
                characterId,
                campaignId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateGameProfileAsync(int characterId, int campaignId, object gameProfileData, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Updating game profile for character {CharacterId} in campaign {CampaignId}",
                characterId,
                campaignId);

            // Verify character ownership
            var character = await this.dbContext.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId && c.IsActive);

            if (character == null)
            {
                this.logger.LogWarning(
                    "Character {CharacterId} not found or user {UserId} not authorized",
                    characterId,
                    userId);
                return false;
            }

            var gameProfile = await this.dbContext.CharacterGameProfiles
                .FirstOrDefaultAsync(p => p.CharacterId == characterId && p.CampaignId == campaignId && p.IsActive);

            if (gameProfile == null)
            {
                this.logger.LogWarning(
                    "Game profile not found for character {CharacterId} in campaign {CampaignId}",
                    characterId,
                    campaignId);
                return false;
            }

            // Serialize game profile data
            var gameSpecificDataJson = CharacterProfileMapper.SerializeGameProfile(gameProfileData, gameProfile.GameType);

            if (gameSpecificDataJson == null)
            {
                this.logger.LogWarning(
                    "Failed to serialize game profile data for game type {GameType}",
                    gameProfile.GameType);
                return false;
            }

            // Update common fields based on game type
            if (gameProfileData is DndCharacterProfile dndProfile)
            {
                gameProfile.Level = dndProfile.Level;
                gameProfile.CurrentHealth = dndProfile.CurrentHitPoints;
                gameProfile.MaxHealth = dndProfile.MaxHitPoints;
            }
            else if (gameProfileData is SkyrimCharacterProfile skyrimProfile)
            {
                gameProfile.Level = skyrimProfile.Level;
                gameProfile.CurrentHealth = skyrimProfile.CurrentHealth;
                gameProfile.MaxHealth = skyrimProfile.MaxHealth;
            }

            gameProfile.GameSpecificData = gameSpecificDataJson;
            gameProfile.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Game profile updated successfully for character {CharacterId} in campaign {CampaignId}",
                characterId,
                campaignId);

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error updating game profile for character {CharacterId} in campaign {CampaignId}",
                characterId,
                campaignId);
            return false;
        }
    }

    /// <summary>
    /// Maps a Character entity to a CharacterDto
    /// </summary>
    /// <param name="character">The character entity</param>
    /// <returns>Character DTO</returns>
    private static CharacterDto MapToDto(Character character)
    {
        return new CharacterDto
        {
            Id = character.Id,
            UserId = character.UserId,
            Name = character.Name,
            FirstName = character.FirstName,
            Description = character.Description,
            Age = character.Age,
            AvatarUrl = character.AvatarUrl,
            CreatedAt = character.CreatedAt,
            UpdatedAt = character.UpdatedAt
        };
    }
}
