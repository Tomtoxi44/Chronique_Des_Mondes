// -----------------------------------------------------------------------
// <copyright file="EventService.cs" company="ANGIBAUD Tommy">
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
/// Service for managing events that affect characters.
/// </summary>
/// <param name="dbContext">Database context for event data access.</param>
/// <param name="logger">Logger instance for structured logging.</param>
public class EventService(
    AppDbContext dbContext,
    ILogger<EventService> logger) : IEventService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly ILogger<EventService> logger = logger;

    /// <inheritdoc/>
    public async Task<EventDto?> CreateEventAsync(CreateEventDto dto, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Creating event '{Name}' at level {Level} by user {UserId}",
                dto.Name,
                dto.Level,
                userId);

            // Validate level and corresponding ID
            var validationResult = await this.ValidateEventLevelAsync(dto, userId);
            if (!validationResult.IsValid)
            {
                this.logger.LogWarning(validationResult.ErrorMessage);
                return null;
            }

            var eventEntity = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                Level = dto.Level,
                WorldId = dto.Level == EventLevel.World ? dto.WorldId : null,
                CampaignId = dto.Level == EventLevel.Campaign ? dto.CampaignId : null,
                ChapterId = dto.Level == EventLevel.Chapter ? dto.ChapterId : null,
                EffectType = dto.EffectType,
                TargetStat = dto.TargetStat,
                ModifierValue = dto.ModifierValue,
                IsActive = true,
                IsPermanent = dto.IsPermanent,
                ExpiresAt = dto.ExpiresAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            this.dbContext.Events.Add(eventEntity);
            await this.dbContext.SaveChangesAsync();


            this.logger.LogInformation(
                "Successfully created event {EventId} '{Name}'",
                eventEntity.Id,
                eventEntity.Name);

            return this.MapToDto(eventEntity);
        }
        catch (Exception ex)
        {

            this.logger.LogError(
                ex,
                "Error creating event '{Name}'",
                dto.Name);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetEventsByWorldAsync(int worldId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving events for world {WorldId} by user {UserId}",
                worldId,
                userId);

            // Check authorization
            var world = await this.dbContext.Worlds.FindAsync(worldId);
            if (world == null || !world.IsActive)
            {
                return Enumerable.Empty<EventDto>();
            }

            var isAuthorized = await this.IsUserAuthorizedForWorldAsync(worldId, userId);
            if (!isAuthorized)
            {
                return Enumerable.Empty<EventDto>();
            }

            var events = await this.dbContext.Events
                .Where(e => e.WorldId == worldId && e.Level == EventLevel.World)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return events.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving events for world {WorldId}", worldId);
            return Enumerable.Empty<EventDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetEventsByCampaignAsync(int campaignId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving events for campaign {CampaignId} by user {UserId}",
                campaignId,
                userId);

            var campaign = await this.dbContext.Campaigns
                .Include(c => c.World)
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

            if (campaign == null)
            {
                return Enumerable.Empty<EventDto>();
            }

            var isAuthorized = await this.IsUserAuthorizedForWorldAsync(campaign.WorldId, userId);
            if (!isAuthorized)
            {
                return Enumerable.Empty<EventDto>();
            }

            var events = await this.dbContext.Events
                .Where(e => e.CampaignId == campaignId && e.Level == EventLevel.Campaign)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return events.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving events for campaign {CampaignId}", campaignId);
            return Enumerable.Empty<EventDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetEventsByChapterAsync(int chapterId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving events for chapter {ChapterId} by user {UserId}",
                chapterId,
                userId);

            var chapter = await this.dbContext.Chapters
                .Include(ch => ch.Campaign)
                    .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(ch => ch.Id == chapterId && ch.IsActive);

            if (chapter == null)
            {
                return Enumerable.Empty<EventDto>();
            }

            var isAuthorized = await this.IsUserAuthorizedForWorldAsync(chapter.Campaign.WorldId, userId);
            if (!isAuthorized)
            {
                return Enumerable.Empty<EventDto>();
            }

            var events = await this.dbContext.Events
                .Where(e => e.ChapterId == chapterId && e.Level == EventLevel.Chapter)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return events.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving events for chapter {ChapterId}", chapterId);
            return Enumerable.Empty<EventDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<EventDto?> GetEventByIdAsync(int eventId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving event {EventId} for user {UserId}",
                eventId,
                userId);

            var eventEntity = await this.dbContext.Events
                .Include(e => e.World)
                .Include(e => e.Campaign)
                    .ThenInclude(c => c!.World)
                .Include(e => e.Chapter)
                    .ThenInclude(ch => ch!.Campaign)
                        .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity == null)
            {
                return null;
            }

            // Get the world ID for authorization
            int? worldId = eventEntity.Level switch
            {
                EventLevel.World => eventEntity.WorldId,
                EventLevel.Campaign => eventEntity.Campaign?.WorldId,
                EventLevel.Chapter => eventEntity.Chapter?.Campaign?.WorldId,
                _ => null
            };

            if (worldId == null || !await this.IsUserAuthorizedForWorldAsync(worldId.Value, userId))
            {
                return null;
            }

            return this.MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving event {EventId}", eventId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<EventDto?> UpdateEventAsync(int eventId, CreateEventDto request, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Updating event {EventId} by user {UserId}",
                eventId,
                userId);

            var eventEntity = await this.dbContext.Events
                .Include(e => e.World)
                .Include(e => e.Campaign)
                    .ThenInclude(c => c!.World)
                .Include(e => e.Chapter)
                    .ThenInclude(ch => ch!.Campaign)
                        .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity == null)
            {
                return null;
            }

            // Only GM can update
            if (!await this.IsUserGmForEventAsync(eventEntity, userId))
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to update event {EventId}",
                    userId,
                    eventId);
                return null;
            }

            eventEntity.Name = request.Name;
            eventEntity.Description = request.Description;
            eventEntity.EffectType = request.EffectType;
            eventEntity.TargetStat = request.TargetStat;
            eventEntity.ModifierValue = request.ModifierValue;
            eventEntity.IsPermanent = request.IsPermanent;
            eventEntity.ExpiresAt = request.ExpiresAt;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            return this.MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating event {EventId}", eventId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteEventAsync(int eventId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Deleting event {EventId} by user {UserId}",
                eventId,
                userId);

            var eventEntity = await this.dbContext.Events
                .Include(e => e.World)
                .Include(e => e.Campaign)
                    .ThenInclude(c => c!.World)
                .Include(e => e.Chapter)
                    .ThenInclude(ch => ch!.Campaign)
                        .ThenInclude(c => c.World)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity == null)
            {
                return false;
            }

            if (!await this.IsUserGmForEventAsync(eventEntity, userId))
            {
                return false;
            }

            // Soft delete
            eventEntity.IsActive = false;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting event {EventId}", eventId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<EventDto?> SetEventActiveAsync(int eventId, bool isActive, int userId)
    {
        try
        {
            var eventEntity = await this.dbContext.Events
                .Include(e => e.World)
                .Include(e => e.Campaign)
                    .ThenInclude(c => c!.World)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity == null)
            {
                return null;
            }

            if (!await this.IsUserGmForEventAsync(eventEntity, userId))
            {
                return null;
            }

            eventEntity.IsActive = isActive;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            return this.MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error setting event {EventId} active status", eventId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetActiveEventsForCharacterAsync(
        int characterId,
        int worldId,
        int? campaignId = null,
        int? chapterId = null)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving active events for character {CharacterId} in world {WorldId}",
                characterId,
                worldId);

            // Get all active events that could affect this character
            var query = this.dbContext.Events
                .Where(e => e.IsActive);

            // World-level events
            var worldEvents = await query
                .Where(e => e.Level == EventLevel.World && e.WorldId == worldId)
                .ToListAsync();

            // Campaign-level events (if campaign specified)
            var campaignEvents = campaignId.HasValue
                ? await query
                    .Where(e => e.Level == EventLevel.Campaign && e.CampaignId == campaignId)
                    .ToListAsync()
                : new List<Event>();

            // Chapter-level events (if chapter specified)
            var chapterEvents = chapterId.HasValue
                ? await query
                    .Where(e => e.Level == EventLevel.Chapter && e.ChapterId == chapterId)
                    .ToListAsync()
                : new List<Event>();

            // Filter out expired temporary events
            var now = DateTime.UtcNow;
            var allEvents = worldEvents
                .Concat(campaignEvents)
                .Concat(chapterEvents)
                .Where(e => e.IsPermanent || e.ExpiresAt == null || e.ExpiresAt > now)
                .OrderBy(e => e.Level)
                .ThenByDescending(e => e.CreatedAt);

            return allEvents.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error retrieving active events for character {CharacterId}",
                characterId);
            return Enumerable.Empty<EventDto>();
        }
    }

    /// <summary>
    /// Validates the event level and corresponding entity ID.
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage)> ValidateEventLevelAsync(CreateEventDto dto, int userId)
    {
        switch (dto.Level)
        {
            case EventLevel.World:
                if (!dto.WorldId.HasValue)
                {
                    return (false, "WorldId is required for world-level events");
                }
                var world = await this.dbContext.Worlds.FindAsync(dto.WorldId.Value);
                if (world == null || !world.IsActive)
                {
                    return (false, $"World {dto.WorldId} not found");
                }
                if (world.UserId != userId)
                {
                    return (false, $"User {userId} is not GM of world {dto.WorldId}");
                }
                break;

            case EventLevel.Campaign:
                if (!dto.CampaignId.HasValue)
                {
                    return (false, "CampaignId is required for campaign-level events");
                }
                var campaign = await this.dbContext.Campaigns
                    .Include(c => c.World)
                    .FirstOrDefaultAsync(c => c.Id == dto.CampaignId.Value && !c.IsDeleted);
                if (campaign == null)
                {
                    return (false, $"Campaign {dto.CampaignId} not found");
                }
                if (campaign.World?.UserId != userId && campaign.CreatedBy != userId)
                {
                    return (false, $"User {userId} is not GM of campaign {dto.CampaignId}");
                }
                break;

            case EventLevel.Chapter:
                if (!dto.ChapterId.HasValue)
                {
                    return (false, "ChapterId is required for chapter-level events");
                }
                var chapter = await this.dbContext.Chapters
                    .Include(ch => ch.Campaign)
                        .ThenInclude(c => c.World)
                    .FirstOrDefaultAsync(ch => ch.Id == dto.ChapterId.Value && ch.IsActive);
                if (chapter == null)
                {
                    return (false, $"Chapter {dto.ChapterId} not found");
                }
                var chCampaign = chapter.Campaign;
                if (chCampaign.World?.UserId != userId && chCampaign.CreatedBy != userId)
                {
                    return (false, $"User {userId} is not GM of chapter {dto.ChapterId}");
                }
                break;
        }

        return (true, null);
    }

    /// <summary>
    /// Checks if the user is authorized to access a world.
    /// </summary>
    private async Task<bool> IsUserAuthorizedForWorldAsync(int worldId, int userId)
    {
        var world = await this.dbContext.Worlds.FindAsync(worldId);
        if (world == null || !world.IsActive)
        {
            return false;
        }

        // GM has access
        if (world.UserId == userId)
        {
            return true;
        }

        // Player with character in world has access
        return await this.dbContext.WorldCharacters
            .AnyAsync(wc => wc.WorldId == worldId && wc.Character.UserId == userId && wc.IsActive);
    }

    /// <summary>
    /// Checks if the user is the GM for the event's context.
    /// </summary>
    private async Task<bool> IsUserGmForEventAsync(Event eventEntity, int userId)
    {
        return eventEntity.Level switch
        {
            EventLevel.World => eventEntity.World?.UserId == userId,
            EventLevel.Campaign => eventEntity.Campaign?.World?.UserId == userId || eventEntity.Campaign?.CreatedBy == userId,
            EventLevel.Chapter => eventEntity.Chapter?.Campaign?.World?.UserId == userId || eventEntity.Chapter?.Campaign?.CreatedBy == userId,
            _ => false
        };
    }

    /// <summary>
    /// Maps an Event entity to an EventDto.
    /// </summary>
    private EventDto MapToDto(Event eventEntity)
    {
        return new EventDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            Level = eventEntity.Level,
            WorldId = eventEntity.WorldId,
            CampaignId = eventEntity.CampaignId,
            ChapterId = eventEntity.ChapterId,
            EffectType = eventEntity.EffectType,
            TargetStat = eventEntity.TargetStat,
            ModifierValue = eventEntity.ModifierValue,
            IsActive = eventEntity.IsActive,
            IsPermanent = eventEntity.IsPermanent,
            ExpiresAt = eventEntity.ExpiresAt,
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt,
            CreatedBy = eventEntity.CreatedBy
        };
    }

    /// <inheritdoc/>
    public async Task<EventDto?> MarkAsPermanentAsync(int eventId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Marking event {EventId} as permanent by user {UserId}",
                eventId,
                userId);

            var eventEntity = await this.dbContext.Events
                .Include(e => e.World)
                .Include(e => e.Campaign)
                .Include(e => e.Chapter)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity == null)
            {
                this.logger.LogWarning("Event {EventId} not found", eventId);
                return null;
            }

            // Verify authorization (must be the creator/GM)
            if (eventEntity.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to modify event {EventId}",
                    userId,
                    eventId);
                return null;
            }

            // Mark as permanent and remove expiration
            eventEntity.IsPermanent = true;
            eventEntity.ExpiresAt = null;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Successfully marked event {EventId} as permanent",
                eventId);

            return this.MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error marking event {EventId} as permanent",
                eventId);

            return null;
        }
    }
}
