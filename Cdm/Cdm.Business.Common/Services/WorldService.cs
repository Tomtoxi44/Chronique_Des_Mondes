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
public class WorldService(
    AppDbContext dbContext,
    ILogger<WorldService> logger) : IWorldService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<WorldService> logger = logger;

    /// <inheritdoc/>
    public async Task<WorldDto?> CreateWorldAsync(CreateWorldDto dto, int userId)
    {
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

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

            await transaction.CommitAsync();

            this.logger.LogInformation(
                "Successfully created world {WorldId} '{WorldName}'",
                world.Id,
                world.Name);

            return this.MapToDto(world);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

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
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

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

            // Only GM can update
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
            await transaction.CommitAsync();

            this.logger.LogInformation("Successfully updated world {WorldId}", worldId);

            return this.MapToDto(world);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

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
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

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

            // Only GM can delete
            if (world.UserId != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to delete world {WorldId}",
                    userId,
                    worldId);
                return false;
            }

            // Soft delete
            world.IsActive = false;
            world.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            this.logger.LogInformation("Successfully deleted world {WorldId}", worldId);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

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
            CharacterName = wc.Character?.Name ?? string.Empty,
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
}
