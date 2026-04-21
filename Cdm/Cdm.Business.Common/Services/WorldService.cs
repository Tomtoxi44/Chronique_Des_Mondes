// -----------------------------------------------------------------------
// <copyright file="WorldService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing world operations.
/// </summary>
/// <param name="dbContext">Database context for world data access.</param>
/// <param name="logger">Logger instance for structured logging.</param>
/// <param name="notificationService">Notification service for creating notifications.</param>
public class WorldService(
    AppDbContext dbContext,
    ILogger<WorldService> logger,
    INotificationService notificationService) : IWorldService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<WorldService> logger = logger;
    private readonly INotificationService notificationService = notificationService;

    /// <inheritdoc/>
    public async Task<WorldDto?> CreateWorldAsync(CreateWorldDto dto, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Creating world '{WorldName}' with GameType {GameType} for user {UserId}",
                dto.Name,
                dto.GameType,
                userId);

            var world = new World
            {
                Name = dto.Name,
                Description = dto.Description,
                GameType = dto.GameType,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            this.dbContext.Worlds.Add(world);
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Successfully created world {WorldId} '{WorldName}'",
                world.Id,
                world.Name);

            return this.MapToDto(world);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error creating world '{WorldName}' for user {UserId}",
                dto.Name,
                userId);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorldDto>> GetMyWorldsAsync(int userId)
    {
        try
        {
            this.logger.LogInformation("Retrieving worlds for user {UserId}", userId);

            var worlds = await this.dbContext.Worlds
                .Include(w => w.Campaigns)
                .Include(w => w.WorldCharacters)
                .Where(w => w.UserId == userId && w.IsActive)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            this.logger.LogInformation(
                "Retrieved {Count} worlds for user {UserId}",
                worlds.Count,
                userId);

            return worlds.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving worlds for user {UserId}", userId);
            return Enumerable.Empty<WorldDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<WorldDto?> GetWorldByIdAsync(int worldId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving world {WorldId} for user {UserId}",
                worldId,
                userId);

            var world = await this.dbContext.Worlds
                .Include(w => w.Campaigns)
                .Include(w => w.WorldCharacters)
                .FirstOrDefaultAsync(w => w.Id == worldId && w.IsActive);

            if (world == null)
            {
                this.logger.LogWarning("World {WorldId} not found", worldId);
                return null;
            }

            // Authorization check: user must be the creator (GM) or a participant
            var isParticipant = await this.dbContext.WorldCharacters
                .AnyAsync(wc => wc.WorldId == worldId && wc.Character.UserId == userId && wc.IsActive);

            if (world.UserId != userId && !isParticipant)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to access world {WorldId}",
                    userId,
                    worldId);
                return null;
            }

            this.logger.LogInformation("Successfully retrieved world {WorldId}", worldId);

            return this.MapToDto(world);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error retrieving world {WorldId} for user {UserId}",
                worldId,
                userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorldDto>> GetWorldsForUserAsync(int userId)
    {
        try
        {
            this.logger.LogInformation("Retrieving all worlds for user {UserId} (as GM or player)", userId);

            // Worlds where user is GM
            var gmWorlds = await this.dbContext.Worlds
                .Include(w => w.Campaigns)
                .Include(w => w.WorldCharacters)
                .Where(w => w.UserId == userId && w.IsActive)
                .ToListAsync();

            // Worlds where user is a player
            var playerWorldIds = await this.dbContext.WorldCharacters
                .Where(wc => wc.Character.UserId == userId && wc.IsActive)
                .Select(wc => wc.WorldId)
                .Distinct()
                .ToListAsync();

            var playerWorlds = await this.dbContext.Worlds
                .Include(w => w.Campaigns)
                .Include(w => w.WorldCharacters)
                .Where(w => playerWorldIds.Contains(w.Id) && w.UserId != userId && w.IsActive)
                .ToListAsync();

            var allWorlds = gmWorlds.Concat(playerWorlds)
                .OrderByDescending(w => w.CreatedAt)
                .ToList();

            this.logger.LogInformation(
                "Retrieved {Count} worlds for user {UserId}",
                allWorlds.Count,
                userId);

            return allWorlds.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving worlds for user {UserId}", userId);
            return Enumerable.Empty<WorldDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<WorldDto?> UpdateWorldAsync(int worldId, CreateWorldDto request, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Updating world {WorldId} for user {UserId}",
                worldId,
                userId);

            var world = await this.dbContext.Worlds.FindAsync(worldId);

            if (world == null || !world.IsActive)
            {
                this.logger.LogWarning("World {WorldId} not found", worldId);
                return null;
            }

            if (world.UserId != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to update world {WorldId}",
                    userId,
                    worldId);
                return null;
            }

            world.Name = request.Name;
            world.Description = request.Description;
            world.GameType = request.GameType;
            world.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Successfully updated world {WorldId}", worldId);

            return this.MapToDto(world);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error updating world {WorldId} for user {UserId}",
                worldId,
                userId);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteWorldAsync(int worldId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Deleting world {WorldId} for user {UserId}",
                worldId,
                userId);

            var world = await this.dbContext.Worlds.FindAsync(worldId);

            if (world == null)
            {
                this.logger.LogWarning("World {WorldId} not found", worldId);
                return false;
            }

            if (world.UserId != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to delete world {WorldId}",
                    userId,
                    worldId);
                return false;
            }

            world.IsActive = false;
            world.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Successfully deleted world {WorldId}", worldId);

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error deleting world {WorldId} for user {UserId}",
                worldId,
                userId);

            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorldCharacterDto>> GetWorldCharactersAsync(int worldId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving characters for world {WorldId} by user {UserId}",
                worldId,
                userId);

            var world = await this.dbContext.Worlds.FindAsync(worldId);

            if (world == null || !world.IsActive)
            {
                this.logger.LogWarning("World {WorldId} not found", worldId);
                return Enumerable.Empty<WorldCharacterDto>();
            }

            // Check authorization
            var isParticipant = await this.dbContext.WorldCharacters
                .AnyAsync(wc => wc.WorldId == worldId && wc.Character.UserId == userId && wc.IsActive);

            if (world.UserId != userId && !isParticipant)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to view characters in world {WorldId}",
                    userId,
                    worldId);
                return Enumerable.Empty<WorldCharacterDto>();
            }

            var worldCharacters = await this.dbContext.WorldCharacters
                .Include(wc => wc.Character)
                .Include(wc => wc.World)
                .Where(wc => wc.WorldId == worldId && wc.IsActive)
                .OrderBy(wc => wc.JoinedAt)
                .ToListAsync();

            this.logger.LogInformation(
                "Retrieved {Count} characters for world {WorldId}",
                worldCharacters.Count,
                worldId);

            return worldCharacters.Select(wc => this.MapToWorldCharacterDto(wc));
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error retrieving characters for world {WorldId}",
                worldId);
            return Enumerable.Empty<WorldCharacterDto>();
        }
    }

    /// <summary>
    /// Maps a World entity to a WorldDto.
    /// </summary>
    /// <param name="world">World entity to map.</param>
    /// <returns>Mapped WorldDto.</returns>
    private WorldDto MapToDto(World world)
    {
        return new WorldDto
        {
            Id = world.Id,
            UserId = world.UserId,
            Name = world.Name,
            Description = world.Description,
            GameType = world.GameType,
            IsActive = world.IsActive,
            CreatedAt = world.CreatedAt,
            UpdatedAt = world.UpdatedAt,
            CampaignCount = world.Campaigns?.Count(c => c.IsActive && !c.IsDeleted) ?? 0,
            CharacterCount = world.WorldCharacters?.Count(wc => wc.IsActive) ?? 0
        };
    }

    /// <summary>
    /// Maps a WorldCharacter entity to a WorldCharacterDto.
    /// </summary>
    /// <param name="wc">WorldCharacter entity to map.</param>
    /// <returns>Mapped WorldCharacterDto.</returns>
    private WorldCharacterDto MapToWorldCharacterDto(WorldCharacter wc)
    {
        return new WorldCharacterDto
        {
            Id = wc.Id,
            CharacterId = wc.CharacterId,
            CharacterName = wc.Character?.FirstName ?? wc.Character?.Name ?? string.Empty,
            WorldId = wc.WorldId,
            WorldName = wc.World?.Name ?? string.Empty,
            GameType = wc.World?.GameType ?? GameType.Custom,
            GameSpecificData = wc.GameSpecificData,
            Level = wc.Level,
            CurrentHealth = wc.CurrentHealth,
            MaxHealth = wc.MaxHealth,
            IsActive = wc.IsActive,
            JoinedAt = wc.JoinedAt,
            UpdatedAt = wc.UpdatedAt,
            UserId = wc.Character?.UserId ?? 0,
            AvatarUrl = wc.Character?.AvatarUrl
        };
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveCharacterFromWorldAsync(int worldId, int characterId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Removing character {CharacterId} from world {WorldId} by user {UserId}",
                characterId, worldId, userId);

            var world = await this.dbContext.Worlds
                .FirstOrDefaultAsync(w => w.Id == worldId && w.IsActive);

            if (world == null)
            {
                this.logger.LogWarning("World {WorldId} not found", worldId);
                return false;
            }

            var worldCharacter = await this.dbContext.WorldCharacters
                .Include(wc => wc.Character)
                .FirstOrDefaultAsync(wc => wc.WorldId == worldId && wc.CharacterId == characterId && wc.IsActive);

            if (worldCharacter == null)
            {
                this.logger.LogWarning(
                    "WorldCharacter not found for character {CharacterId} in world {WorldId}",
                    characterId, worldId);
                return false;
            }

            // Only the world owner (GM) or the character owner can remove the character
            bool isWorldOwner = world.UserId == userId;
            bool isCharacterOwner = worldCharacter.Character?.UserId == userId;

            if (!isWorldOwner && !isCharacterOwner)
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to remove character {CharacterId} from world {WorldId}",
                    userId, characterId, worldId);
                return false;
            }

            // Soft delete the world character
            worldCharacter.IsActive = false;
            worldCharacter.UpdatedAt = DateTime.UtcNow;

            // Unlock the character so it can join another world
            if (worldCharacter.Character != null)
            {
                worldCharacter.Character.IsLocked = false;
            }

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Successfully removed character {CharacterId} from world {WorldId}",
                characterId, worldId);

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error removing character {CharacterId} from world {WorldId}",
                characterId, worldId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GenerateWorldInviteTokenAsync(int worldId, int userId)
    {
        try
        {
            var world = await this.dbContext.Worlds.FindAsync(worldId);
            if (world == null || !world.IsActive || world.UserId != userId)
            {
                this.logger.LogWarning(
                    "World {WorldId} not found or user {UserId} not authorized to generate invite token",
                    worldId, userId);
                return null;
            }

            world.InviteToken = Guid.NewGuid().ToString("N");
            world.InviteTokenExpiry = DateTime.UtcNow.AddDays(30);
            world.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Generated invite token for world {WorldId}", worldId);
            return world.InviteToken;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error generating invite token for world {WorldId}", worldId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<WorldDto?> GetWorldByInviteTokenAsync(string inviteToken)
    {
        try
        {
            var world = await this.dbContext.Worlds
                .Include(w => w.Campaigns)
                .Include(w => w.WorldCharacters)
                .FirstOrDefaultAsync(w =>
                    w.InviteToken == inviteToken &&
                    w.InviteTokenExpiry > DateTime.UtcNow &&
                    w.IsActive);

            if (world == null)
            {
                this.logger.LogWarning("World not found for invite token (invalid or expired)");
                return null;
            }

            return this.MapToDto(world);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving world by invite token");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<WorldCharacterDto?> JoinWorldAsync(string inviteToken, int characterId, int userId)
    {
        try
        {
            var world = await this.dbContext.Worlds
                .FirstOrDefaultAsync(w =>
                    w.InviteToken == inviteToken &&
                    w.InviteTokenExpiry > DateTime.UtcNow &&
                    w.IsActive);

            if (world == null)
            {
                this.logger.LogWarning("Invalid or expired invite token");
                return null;
            }

            var character = await this.dbContext.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId && c.IsActive);

            if (character == null)
            {
                this.logger.LogWarning(
                    "Character {CharacterId} not found or not owned by user {UserId}",
                    characterId, userId);
                return null;
            }

            if (character.IsLocked)
            {
                this.logger.LogWarning("Character {CharacterId} is already locked in another world", characterId);
                return null;
            }

            // Check if already in this world
            var existing = await this.dbContext.WorldCharacters
                .FirstOrDefaultAsync(wc => wc.WorldId == world.Id && wc.CharacterId == characterId);
            if (existing != null)
            {
                if (existing.IsActive)
                {
                    this.logger.LogWarning("Character {CharacterId} is already in world {WorldId}", characterId, world.Id);
                    return null;
                }

                // Reactivate if previously removed
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
                character.IsLocked = true;
                await this.dbContext.SaveChangesAsync();
                return this.MapToWorldCharacterDto(existing);
            }

            var worldCharacter = new WorldCharacter
            {
                WorldId = world.Id,
                CharacterId = characterId,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };

            character.IsLocked = true;

            this.dbContext.WorldCharacters.Add(worldCharacter);
            await this.dbContext.SaveChangesAsync();

            // Notify the GM
            await this.notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = world.UserId,
                Type = NotificationType.WorldInvite,
                Title = "Un joueur a rejoint votre monde",
                Message = $"Le personnage '{character.Name}' a rejoint le monde '{world.Name}'.",
                RelatedEntityId = world.Id,
                RelatedEntityType = "World",
                ActionUrl = $"/worlds/{world.Id}",
                SentBy = userId
            });

            // Reload with nav props
            await this.dbContext.Entry(worldCharacter).Reference(wc => wc.Character).LoadAsync();
            await this.dbContext.Entry(worldCharacter).Reference(wc => wc.World).LoadAsync();

            this.logger.LogInformation(
                "Character {CharacterId} joined world {WorldId}",
                characterId, world.Id);

            return this.MapToWorldCharacterDto(worldCharacter);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error joining world with invite token");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<WorldCharacterDto?> GetMyWorldCharacterAsync(int worldId, int userId)
    {
        try
        {
            var worldCharacter = await this.dbContext.WorldCharacters
                .Include(wc => wc.Character)
                .Include(wc => wc.World)
                .FirstOrDefaultAsync(wc =>
                    wc.WorldId == worldId &&
                    wc.Character.UserId == userId &&
                    wc.IsActive);

            if (worldCharacter == null)
            {
                this.logger.LogWarning(
                    "No active world character found for user {UserId} in world {WorldId}",
                    userId, worldId);
                return null;
            }

            return this.MapToWorldCharacterDto(worldCharacter);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving world character for user {UserId} in world {WorldId}", userId, worldId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<WorldCharacterDto?> UpdateMyWorldCharacterAsync(int worldId, UpdateWorldCharacterProfileDto dto, int userId)
    {
        try
        {
            var worldCharacter = await this.dbContext.WorldCharacters
                .Include(wc => wc.Character)
                .Include(wc => wc.World)
                .FirstOrDefaultAsync(wc =>
                    wc.WorldId == worldId &&
                    wc.Character.UserId == userId &&
                    wc.IsActive);

            if (worldCharacter == null)
            {
                this.logger.LogWarning(
                    "No active world character found for user {UserId} in world {WorldId} (update)",
                    userId, worldId);
                return null;
            }

            worldCharacter.Level = dto.Level;
            worldCharacter.CurrentHealth = dto.CurrentHealth;
            worldCharacter.MaxHealth = dto.MaxHealth;
            worldCharacter.GameSpecificData = dto.GameSpecificData;
            worldCharacter.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Updated world character for user {UserId} in world {WorldId}",
                userId, worldId);

            return this.MapToWorldCharacterDto(worldCharacter);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating world character for user {UserId} in world {WorldId}", userId, worldId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CampaignDto>> GetWorldCampaignsForMemberAsync(int worldId, int userId)
    {
        try
        {
            var world = await this.dbContext.Worlds.FindAsync(worldId);
            if (world == null || !world.IsActive)
            {
                this.logger.LogWarning("World {WorldId} not found", worldId);
                return Enumerable.Empty<CampaignDto>();
            }

            // Check authorization: must be GM or active participant
            var isGm = world.UserId == userId;
            var isParticipant = await this.dbContext.WorldCharacters
                .AnyAsync(wc => wc.WorldId == worldId && wc.Character.UserId == userId && wc.IsActive);

            if (!isGm && !isParticipant)
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to view campaigns for world {WorldId}",
                    userId, worldId);
                return Enumerable.Empty<CampaignDto>();
            }

            var campaigns = await this.dbContext.Campaigns
                .Where(c => c.WorldId == worldId && c.IsActive && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return campaigns.Select(c => new CampaignDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                WorldId = c.WorldId,
                GameType = world.GameType,
                Visibility = c.Visibility,
                MaxPlayers = c.MaxPlayers,
                CoverImageUrl = c.CoverImageUrl,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Status = c.Status,
                IsActive = c.IsActive
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving campaigns for world {WorldId}", worldId);
            return Enumerable.Empty<CampaignDto>();
        }
    }
}
