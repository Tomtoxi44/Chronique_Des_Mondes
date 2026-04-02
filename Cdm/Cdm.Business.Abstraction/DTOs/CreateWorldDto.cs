// -----------------------------------------------------------------------
// <copyright file="CreateWorldDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System.ComponentModel.DataAnnotations;
using Cdm.Common.Enums;

/// <summary>
/// Data transfer object for creating a new World.
/// </summary>
public class CreateWorldDto
{
    /// <summary>
    /// Gets or sets the world name (3-100 characters).
    /// </summary>
    [Required(ErrorMessage = "World name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "World name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the world description (max 2000 characters).
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the game type for this world.
    /// </summary>
    [Required(ErrorMessage = "Game type is required")]
    public GameType GameType { get; set; }
}
