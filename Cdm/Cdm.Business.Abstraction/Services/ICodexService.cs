// -----------------------------------------------------------------------
// <copyright file="ICodexService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;

/// <summary>
/// Manages a user's personal codex of item templates.
/// </summary>
public interface ICodexService
{
    /// <summary>Creates a codex item owned by the user.</summary>
    Task<CodexItemDto?> CreateAsync(CreateCodexItemDto dto, int userId);

    /// <summary>Lists the user's active codex items, optionally filtered by game type.</summary>
    Task<IEnumerable<CodexItemDto>> GetMyItemsAsync(int userId, GameType? gameType = null);

    /// <summary>Gets one of the user's codex items by id.</summary>
    Task<CodexItemDto?> GetByIdAsync(int id, int userId);

    /// <summary>Updates one of the user's codex items.</summary>
    Task<CodexItemDto?> UpdateAsync(int id, CreateCodexItemDto dto, int userId);

    /// <summary>Soft-deletes one of the user's codex items.</summary>
    Task<bool> DeleteAsync(int id, int userId);
}
