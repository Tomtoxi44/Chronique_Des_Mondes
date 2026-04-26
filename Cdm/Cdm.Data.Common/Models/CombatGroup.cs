// -----------------------------------------------------------------------
// <copyright file="CombatGroup.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Represents a faction or team within a combat encounter.
/// </summary>
public class CombatGroup
{
    /// <summary>Gets or sets the group ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the combat this group belongs to.</summary>
    public int CombatId { get; set; }

    /// <summary>Gets or sets the group name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the hex color used to display this group.</summary>
    public string Color { get; set; } = "#6366f1";

    /// <summary>Gets or sets the display ordering index.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Gets or sets the combat navigation property.</summary>
    public Combat Combat { get; set; } = null!;

    /// <summary>Gets or sets the participants in this group.</summary>
    public ICollection<CombatParticipant> Participants { get; set; } = new List<CombatParticipant>();
}
