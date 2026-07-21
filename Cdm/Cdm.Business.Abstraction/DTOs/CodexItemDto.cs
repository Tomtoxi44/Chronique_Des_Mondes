// -----------------------------------------------------------------------
// <copyright file="CodexItemDto.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.DTOs;

using System;
using Cdm.Common.Enums;

/// <summary>
/// A codex item (user-owned generic item template).
/// </summary>
public class CodexItemDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public GameType GameType { get; set; }

    public string? ItemType { get; set; }

    public string? GameSpecificData { get; set; }

    public bool IsShared { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Owner's display name — populated only for marketplace listings.</summary>
    public string? SharedByName { get; set; }
}

/// <summary>
/// Payload to create or update a codex item.
/// </summary>
public class CreateCodexItemDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public GameType GameType { get; set; } = GameType.Generic;

    public string? ItemType { get; set; }

    public string? GameSpecificData { get; set; }
}
