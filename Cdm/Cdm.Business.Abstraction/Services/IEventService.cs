// -----------------------------------------------------------------------
// <copyright file="IEventService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;

/// <summary>
/// Service for managing events that affect characters.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Creates a new event.
    /// </summary>
    /// <param name="request">The event creation request.</param>
    /// <param name="userId">The user identifier of the event creator (must be GM).</param>
    /// <returns>The created event.</returns>
    Task<EventDto?> CreateEventAsync(CreateEventDto request, int userId);

    /// <summary>
    /// Gets all events for a world.
    /// </summary>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="userId">The user identifier requesting the events.</param>
    /// <returns>A list of world-level events.</returns>
    Task<IEnumerable<EventDto>> GetEventsByWorldAsync(int worldId, int userId);

    /// <summary>
    /// Gets all events for a campaign.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <param name="userId">The user identifier requesting the events.</param>
    /// <returns>A list of campaign-level events.</returns>
    Task<IEnumerable<EventDto>> GetEventsByCampaignAsync(int campaignId, int userId);

    /// <summary>
    /// Gets all events for a chapter.
    /// </summary>
    /// <param name="chapterId">The chapter identifier.</param>
    /// <param name="userId">The user identifier requesting the events.</param>
    /// <returns>A list of chapter-level events.</returns>
    Task<IEnumerable<EventDto>> GetEventsByChapterAsync(int chapterId, int userId);

    /// <summary>
    /// Gets an event by its identifier.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="userId">The user identifier requesting the event.</param>
    /// <returns>The event if found and authorized, null otherwise.</returns>
    Task<EventDto?> GetEventByIdAsync(int eventId, int userId);

    /// <summary>
    /// Updates an event.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="userId">The user identifier requesting the update (must be GM).</param>
    /// <returns>The updated event, or null if not found/unauthorized.</returns>
    Task<EventDto?> UpdateEventAsync(int eventId, CreateEventDto request, int userId);

    /// <summary>
    /// Deletes an event.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="userId">The user identifier requesting the deletion (must be GM).</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteEventAsync(int eventId, int userId);

    /// <summary>
    /// Toggles the active status of an event.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="isActive">The new active status.</param>
    /// <param name="userId">The user identifier requesting the change (must be GM).</param>
    /// <returns>The updated event, or null if not found/unauthorized.</returns>
    Task<EventDto?> SetEventActiveAsync(int eventId, bool isActive, int userId);

    /// <summary>
    /// Gets all active events affecting a character in a given context.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="worldId">The world identifier.</param>
    /// <param name="campaignId">Optional campaign identifier.</param>
    /// <param name="chapterId">Optional chapter identifier.</param>
    /// <returns>A list of active events affecting the character.</returns>
    Task<IEnumerable<EventDto>> GetActiveEventsForCharacterAsync(int characterId, int worldId, int? campaignId = null, int? chapterId = null);
}
