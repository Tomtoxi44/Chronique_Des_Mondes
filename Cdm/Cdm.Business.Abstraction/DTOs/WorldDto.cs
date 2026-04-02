// -----------------------------------------------------------------------
// <copyright file="WorldDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for a World.
/// </summary>
public class WorldDto
{
    /// <summary>
    /// Gets or sets the world identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID who created this world (Game Master).
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the world name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the world description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the game type for this world.
    /// </summary>
    public GameType GameType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the world is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of campaigns in this world.
    /// </summary>
    public int CampaignCount { get; set; }

    /// <summary>
    /// Gets or sets the number of characters in this world.
    /// </summary>
    public int CharacterCount { get; set; }
}
