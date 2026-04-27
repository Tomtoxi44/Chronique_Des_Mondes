// -----------------------------------------------------------------------
// <copyright file="NpcService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing Non-Player Characters (NPCs) within chapters.
/// </summary>
/// <param name="dbContext">Database context.</param>
/// <param name="logger">Logger instance.</param>
public class NpcService(
    AppDbContext dbContext,
    ILogger<NpcService> logger) : INpcService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<NpcService> logger = logger;

    /// <inheritdoc/>
    public async Task<NpcDto?> CreateNpcAsync(CreateNpcDto request, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Creating NPC for chapter {ChapterId} by user {UserId}",
                request.ChapterId,
                userId);

            if (!await this.IsGmOfChapterAsync(request.ChapterId, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to create NPCs in chapter {ChapterId}",
                    userId,
                    request.ChapterId);
                return null;
            }

            var npc = new NonPlayerCharacter
            {
                ChapterId = request.ChapterId,
                Name = request.Name,
                FirstName = request.FirstName,
                Description = request.Description,
                PhysicalDescription = request.PhysicalDescription,
                Age = request.Age,
                GameSpecificData = request.GameSpecificData,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            this.dbContext.NonPlayerCharacters.Add(npc);
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Created NPC {NpcId} in chapter {ChapterId}", npc.Id, npc.ChapterId);

            return MapToDto(npc);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating NPC for chapter {ChapterId}", request.ChapterId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<NpcDto>> GetNpcsByChapterAsync(int chapterId, int userId)
    {
        try
        {
            if (!await this.IsGmOfChapterAsync(chapterId, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} is not authorized to view NPCs in chapter {ChapterId}",
                    userId,
                    chapterId);
                return Enumerable.Empty<NpcDto>();
            }

            var npcs = await this.dbContext.NonPlayerCharacters
                .Where(npc => npc.ChapterId == chapterId && npc.IsActive)
                .OrderBy(npc => npc.CreatedAt)
                .ToListAsync();

            return npcs.Select(MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving NPCs for chapter {ChapterId}", chapterId);
            return Enumerable.Empty<NpcDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<NpcDto?> GetNpcByIdAsync(int npcId, int userId)
    {
        try
        {
            var npc = await this.dbContext.NonPlayerCharacters
                .FirstOrDefaultAsync(n => n.Id == npcId && n.IsActive);

            if (npc == null)
            {
                this.logger.LogWarning("NPC {NpcId} not found", npcId);
                return null;
            }

            if (!await this.IsGmOfChapterAsync(npc.ChapterId, userId))
            {
                this.logger.LogWarning("User {UserId} not authorized to view NPC {NpcId}", userId, npcId);
                return null;
            }

            return MapToDto(npc);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving NPC {NpcId}", npcId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<NpcDto?> UpdateNpcAsync(int npcId, CreateNpcDto request, int userId)
    {
        try
        {
            var npc = await this.dbContext.NonPlayerCharacters
                .FirstOrDefaultAsync(n => n.Id == npcId && n.IsActive);

            if (npc == null)
            {
                this.logger.LogWarning("NPC {NpcId} not found", npcId);
                return null;
            }

            if (!await this.IsGmOfChapterAsync(npc.ChapterId, userId))
            {
                this.logger.LogWarning("User {UserId} not authorized to update NPC {NpcId}", userId, npcId);
                return null;
            }

            npc.Name = request.Name;
            npc.FirstName = request.FirstName;
            npc.Description = request.Description;
            npc.PhysicalDescription = request.PhysicalDescription;
            npc.Age = request.Age;
            npc.GameSpecificData = request.GameSpecificData;
            npc.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Updated NPC {NpcId}", npcId);
            return MapToDto(npc);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating NPC {NpcId}", npcId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteNpcAsync(int npcId, int userId)
    {
        try
        {
            var npc = await this.dbContext.NonPlayerCharacters
                .FirstOrDefaultAsync(n => n.Id == npcId && n.IsActive);

            if (npc == null)
            {
                this.logger.LogWarning("NPC {NpcId} not found", npcId);
                return false;
            }

            if (!await this.IsGmOfChapterAsync(npc.ChapterId, userId))
            {
                this.logger.LogWarning("User {UserId} not authorized to delete NPC {NpcId}", userId, npcId);
                return false;
            }

            npc.IsActive = false;
            npc.UpdatedAt = DateTime.UtcNow;
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Soft-deleted NPC {NpcId}", npcId);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting NPC {NpcId}", npcId);
            return false;
        }
    }

    /// <summary>
    /// Checks if the given user is the GM of the world/campaign that owns the given chapter.
    /// </summary>
    private async Task<bool> IsGmOfChapterAsync(int chapterId, int userId)
    {
        var chapter = await this.dbContext.Chapters
            .Include(ch => ch.Campaign)
                .ThenInclude(c => c.World)
            .FirstOrDefaultAsync(ch => ch.Id == chapterId && ch.IsActive);

        if (chapter == null) return false;

        return chapter.Campaign.World?.UserId == userId
            || chapter.Campaign.CreatedBy == userId;
    }

    private static NpcDto MapToDto(NonPlayerCharacter npc)
    {
        return new NpcDto
        {
            Id = npc.Id,
            ChapterId = npc.ChapterId,
            Name = npc.Name,
            FirstName = npc.FirstName,
            Description = npc.Description,
            PhysicalDescription = npc.PhysicalDescription,
            Age = npc.Age,
            GameSpecificData = npc.GameSpecificData,
            CreatedAt = npc.CreatedAt,
            UpdatedAt = npc.UpdatedAt
        };
    }
}
