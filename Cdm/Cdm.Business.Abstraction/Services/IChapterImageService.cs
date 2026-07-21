// -----------------------------------------------------------------------
// <copyright file="IChapterImageService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Abstraction.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cdm.Business.Abstraction.DTOs;

/// <summary>
/// Manages a chapter's image gallery (maps/place visuals). Only the campaign's GM may manage it.
/// </summary>
public interface IChapterImageService
{
    /// <summary>Lists a chapter's images (GM only).</summary>
    Task<IEnumerable<ChapterImageDto>> GetForChapterAsync(int chapterId, int userId);

    /// <summary>Attaches an image to a chapter (GM only).</summary>
    Task<ChapterImageDto?> AddAsync(int chapterId, AddChapterImageDto dto, int userId);

    /// <summary>Removes a chapter image (GM only).</summary>
    Task<bool> DeleteAsync(int imageId, int userId);
}
