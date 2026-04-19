// -----------------------------------------------------------------------
// <copyright file="World.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using Cdm.Common.Enums;

/// <summary>
/// Represents a world (container for campaigns).
/// </summary>
public class World
{
    /// <summary>
    /// Gets or sets the world ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID who created this world.
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
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the invite token for joining the world (generated on demand).
    /// </summary>
    public string? InviteToken { get; set; }

    /// <summary>
    /// Gets or sets the expiry date of the invite token.
    /// </summary>
    public DateTime? InviteTokenExpiry { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the owner user.
    /// </summary>
    public virtual User Owner { get; set; } = null!;

    /// <summary>
    /// Gets or sets the campaigns in this world.
    /// </summary>
    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    /// <summary>
    /// Gets or sets the world characters (characters adapted to this world).
    /// </summary>
    public virtual ICollection<WorldCharacter> WorldCharacters { get; set; } = new List<WorldCharacter>();

    /// <summary>
    /// Gets or sets the events specific to this world.
    /// </summary>
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    /// <summary>
    /// Gets or sets the achievements specific to this world.
    /// </summary>
    public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
}
