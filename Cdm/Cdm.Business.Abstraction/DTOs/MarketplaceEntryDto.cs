// -----------------------------------------------------------------------
// <copyright file="MarketplaceEntryDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using Cdm.Common.Enums;

/// <summary>
/// A unified marketplace entry (world, campaign or base character shared by a user).
/// </summary>
public class MarketplaceEntryDto
{
    /// <summary>Gets or sets the source entity identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the optional image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the game system / theme.</summary>
    public GameType GameType { get; set; }

    /// <summary>Gets or sets the owner's display name.</summary>
    public string? SharedByName { get; set; }
}
