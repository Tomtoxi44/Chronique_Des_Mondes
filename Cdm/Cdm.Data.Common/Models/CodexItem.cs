// -----------------------------------------------------------------------
// <copyright file="CodexItem.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

using Cdm.Common.Enums;

/// <summary>
/// A user-owned item template stored in their personal codex. Items are generic across
/// game systems (theme via <see cref="GameType"/>, system-specific stats in
/// <see cref="GameSpecificData"/>) and can later be copied into a character's inventory
/// or shared on the marketplace.
/// </summary>
public class CodexItem
{
    /// <summary>Gets or sets the identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the owner user identifier.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the item name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the optional image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the game system / theme this item targets.</summary>
    public GameType GameType { get; set; } = GameType.Generic;

    /// <summary>Gets or sets a free-form item type (e.g. "Arme", "Armure", "Potion", "Objet").</summary>
    public string? ItemType { get; set; }

    /// <summary>Gets or sets system-specific data as JSON (e.g. D&amp;D damage/AC), mirroring the character pattern.</summary>
    public string? GameSpecificData { get; set; }

    /// <summary>Gets or sets a value indicating whether the item is shared on the marketplace.</summary>
    public bool IsShared { get; set; } = false;

    /// <summary>Gets or sets a value indicating whether the item is active (soft-delete flag).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the creation timestamp (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the last update timestamp (UTC).</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Gets or sets the owner navigation property.</summary>
    public virtual User User { get; set; } = null!;
}
