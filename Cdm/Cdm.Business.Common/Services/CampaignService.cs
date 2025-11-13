// -----------------------------------------------------------------------
// <copyright file="CampaignService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing campaign operations.
/// </summary>
/// <param name="dbContext">Database context for campaign data access.</param>
/// <param name="imageStorageService">Service for image storage operations.</param>
/// <param name="logger">Logger instance for structured logging.</param>
public class CampaignService(
    AppDbContext dbContext,
    IImageStorageService imageStorageService,
    ILogger<CampaignService> logger) : ICampaignService
{
    private readonly AppDbContext dbContext = dbContext;
    private readonly IImageStorageService imageStorageService = imageStorageService;
    private readonly ILogger<CampaignService> logger = logger;

    /// <inheritdoc/>
    public async Task<CampaignDto?> CreateCampaignAsync(CreateCampaignDto dto, int userId)
    {
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

        try
        {
            this.logger.LogInformation(
                "Creating campaign '{CampaignName}' for user {UserId}",
                dto.Name,
                userId);

            // Handle image upload if provided
            string? coverImageUrl = null;
            if (!string.IsNullOrWhiteSpace(dto.CoverImageBase64))
            {
                this.logger.LogDebug("Uploading cover image for campaign '{CampaignName}'", dto.Name);

                // Upload image (campaignId will be 0 initially, so we'll use a temporary ID)
                coverImageUrl = await this.imageStorageService.UploadCampaignCoverAsync(
                    dto.CoverImageBase64,
                    0); // Temporary ID

                if (coverImageUrl == null)
                {
                    this.logger.LogWarning(
                        "Failed to upload cover image for campaign '{CampaignName}'",
                        dto.Name);
                    return null;
                }
            }

            // Create campaign entity
            var campaign = new Campaign
            {
                Name = dto.Name,
                Description = dto.Description,
                GameType = dto.GameType,
                Visibility = dto.Visibility,
                MaxPlayers = dto.MaxPlayers,
                CoverImageUrl = coverImageUrl,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            this.dbContext.Campaigns.Add(campaign);
            await this.dbContext.SaveChangesAsync();

            // If we uploaded an image with temporary ID, rename it with actual campaign ID
            if (coverImageUrl != null)
            {
                var actualImageUrl = await this.imageStorageService.UploadCampaignCoverAsync(
                    dto.CoverImageBase64,
                    campaign.Id);

                if (actualImageUrl != null)
                {
                    // Delete old temporary file
                    await this.imageStorageService.DeleteCampaignCoverAsync(coverImageUrl);

                    // Update campaign with new URL
                    campaign.CoverImageUrl = actualImageUrl;
                    await this.dbContext.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();

            this.logger.LogInformation(
                "Successfully created campaign {CampaignId} '{CampaignName}'",
                campaign.Id,
                campaign.Name);

            return this.MapToDto(campaign);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            this.logger.LogError(
                ex,
                "Error creating campaign '{CampaignName}' for user {UserId}",
                dto.Name,
                userId);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CampaignDto>> GetMyCampaignsAsync(int userId)
    {
        try
        {
            this.logger.LogInformation("Retrieving campaigns for user {UserId}", userId);

            var campaigns = await this.dbContext.Campaigns
                .Where(c => c.CreatedBy == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            this.logger.LogInformation(
                "Retrieved {Count} campaigns for user {UserId}",
                campaigns.Count,
                userId);

            return campaigns.Select(this.MapToDto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving campaigns for user {UserId}", userId);
            return Enumerable.Empty<CampaignDto>();
        }
    }

    /// <inheritdoc/>
    public async Task<CampaignDto?> GetCampaignByIdAsync(int campaignId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Retrieving campaign {CampaignId} for user {UserId}",
                campaignId,
                userId);

            var campaign = await this.dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return null;
            }

            // Authorization check: user must be the creator
            if (campaign.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to access campaign {CampaignId}",
                    userId,
                    campaignId);
                return null;
            }

            this.logger.LogInformation(
                "Successfully retrieved campaign {CampaignId}",
                campaignId);

            return this.MapToDto(campaign);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error retrieving campaign {CampaignId} for user {UserId}",
                campaignId,
                userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CampaignDto?> UpdateCampaignAsync(int campaignId, UpdateCampaignDto dto, int userId)
    {
        this.logger.LogInformation("Updating campaign {CampaignId} by user {UserId}", campaignId, userId);

        try
        {
            // 1. Retrieve campaign with tracking
            var campaign = await this.dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return null;
            }

            // 2. Authorization check: only creator can update
            if (campaign.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to update campaign {CampaignId}. Creator is {CreatorId}",
                    userId,
                    campaignId,
                    campaign.CreatedBy);
                return null; // Not authorized
            }

            // 3. Validate MaxPlayers constraint (cannot be less than current player count)
            // Note: Cette fonctionnalité nécessite une table CampaignPlayers (future US)
            // Pour l'instant, on autorise toute valeur entre 1-20
            // TODO: Uncomment when CampaignPlayers table exists
            /*
            var currentPlayerCount = await this.dbContext.CampaignPlayers
                .CountAsync(cp => cp.CampaignId == campaignId);

            if (dto.MaxPlayers < currentPlayerCount)
            {
                throw new ValidationException(
                    $"MaxPlayers must be at least {currentPlayerCount} (current player count).");
            }
            */

            // 4. Update scalar properties
            campaign.Name = dto.Name;
            campaign.Description = dto.Description;
            campaign.Visibility = dto.Visibility;
            campaign.MaxPlayers = dto.MaxPlayers;
            campaign.UpdatedAt = DateTime.UtcNow;

            // 5. Handle cover image update
            if (!string.IsNullOrWhiteSpace(dto.CoverImageBase64))
            {
                this.logger.LogInformation("Updating cover image for campaign {CampaignId}", campaignId);

                // Delete old image if exists
                if (!string.IsNullOrEmpty(campaign.CoverImageUrl))
                {
                    this.logger.LogDebug("Deleting old cover image: {ImageUrl}", campaign.CoverImageUrl);
                    await this.imageStorageService.DeleteCampaignCoverAsync(campaign.CoverImageUrl);
                }

                // Upload new image
                var newImageUrl = await this.imageStorageService.UploadCampaignCoverAsync(
                    dto.CoverImageBase64,
                    campaignId);

                if (newImageUrl == null)
                {
                    this.logger.LogWarning(
                        "Failed to upload new cover image for campaign {CampaignId}",
                        campaignId);
                    return null;
                }

                campaign.CoverImageUrl = newImageUrl;
            }

            // 6. Save changes
            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation("Campaign {CampaignId} updated successfully", campaignId);

            // 7. Return updated DTO
            return this.MapToDto(campaign);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error updating campaign {CampaignId} by user {UserId}",
                campaignId,
                userId);
            return null;
        }
    }

    /// <summary>
    /// Maps a Campaign entity to a CampaignDto.
    /// </summary>
    /// <param name="campaign">Campaign entity to map.</param>
    /// <returns>Mapped CampaignDto.</returns>
    private CampaignDto MapToDto(Campaign campaign)
    {
        return new CampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            GameType = campaign.GameType,
            Visibility = campaign.Visibility,
            MaxPlayers = campaign.MaxPlayers,
            CoverImageUrl = campaign.CoverImageUrl,
            CreatedBy = campaign.CreatedBy,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt,
            IsActive = campaign.IsActive
        };
    }
}
