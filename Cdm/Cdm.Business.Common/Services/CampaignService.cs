// -----------------------------------------------------------------------
// <copyright file="CampaignService.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Common.Enums;
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

            // Verify world exists
            var world = await this.dbContext.Worlds.FindAsync(dto.WorldId);
            if (world == null)
            {
                this.logger.LogWarning(
                    "World {WorldId} not found for campaign creation",
                    dto.WorldId);
                return null;
            }

            // Create campaign entity
            var campaign = new Campaign
            {
                Name = dto.Name,
                Description = dto.Description,
                WorldId = dto.WorldId,
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
                .Include(c => c.World)
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
                .Include(c => c.World)
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
    public async Task<CampaignDto?> UpdateCampaignAsync(int campaignId, CreateCampaignDto dto, int userId)
    {
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();

        try
        {
            this.logger.LogInformation(
                "Updating campaign {CampaignId} for user {UserId}",
                campaignId,
                userId);

            var campaign = await this.dbContext.Campaigns
                .Include(c => c.World)
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
                    "User {UserId} not authorized to update campaign {CampaignId}",
                    userId,
                    campaignId);
                return null;
            }

            // Handle image upload if provided and different
            if (!string.IsNullOrWhiteSpace(dto.CoverImageBase64))
            {
                this.logger.LogDebug("Uploading new cover image for campaign {CampaignId}", campaignId);

                // Delete old image if exists
                if (!string.IsNullOrWhiteSpace(campaign.CoverImageUrl))
                {
                    await this.imageStorageService.DeleteCampaignCoverAsync(campaign.CoverImageUrl);
                }

                // Upload new image
                var coverImageUrl = await this.imageStorageService.UploadCampaignCoverAsync(
                    dto.CoverImageBase64,
                    campaignId);

                if (coverImageUrl != null)
                {
                    campaign.CoverImageUrl = coverImageUrl;
                }
            }

            // Verify world exists if changed
            if (dto.WorldId != campaign.WorldId)
            {
                var world = await this.dbContext.Worlds.FindAsync(dto.WorldId);
                if (world == null)
                {
                    this.logger.LogWarning(
                        "World {WorldId} not found for campaign update",
                        dto.WorldId);
                    return null;
                }

                campaign.WorldId = dto.WorldId;
            }

            // Update campaign properties
            campaign.Name = dto.Name;
            campaign.Description = dto.Description;
            campaign.Visibility = dto.Visibility;
            campaign.MaxPlayers = dto.MaxPlayers;
            campaign.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            this.logger.LogInformation(
                "Successfully updated campaign {CampaignId}",
                campaignId);

            return this.MapToDto(campaign);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            this.logger.LogError(
                ex,
                "Error updating campaign {CampaignId} for user {UserId}",
                campaignId,
                userId);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteCampaignAsync(int campaignId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Deleting campaign {CampaignId} for user {UserId}",
                campaignId,
                userId);

            var campaign = await this.dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return false;
            }

            // Authorization check: user must be the creator
            if (campaign.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to delete campaign {CampaignId}",
                    userId,
                    campaignId);
                return false;
            }

            // Soft delete
            campaign.IsDeleted = true;
            campaign.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Successfully deleted campaign {CampaignId}",
                campaignId);

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error deleting campaign {CampaignId} for user {UserId}",
                campaignId,
                userId);

            return false;
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
            WorldId = campaign.WorldId,
            GameType = campaign.World?.GameType ?? GameType.Custom,
            Visibility = campaign.Visibility,
            MaxPlayers = campaign.MaxPlayers,
            CoverImageUrl = campaign.CoverImageUrl,
            Status = campaign.Status,
            InviteToken = campaign.InviteToken,
            CreatedBy = campaign.CreatedBy,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt,
            IsActive = campaign.IsActive
        };
    }

    /// <inheritdoc/>
    public async Task<string?> GenerateInviteTokenAsync(int campaignId, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Generating invite token for campaign {CampaignId} by user {UserId}",
                campaignId,
                userId);

            var campaign = await this.dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return null;
            }

            // Authorization: only GM can generate invite token
            if (campaign.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to generate invite token for campaign {CampaignId}",
                    userId,
                    campaignId);
                return null;
            }

            // Return existing token or generate new one
            if (!string.IsNullOrWhiteSpace(campaign.InviteToken))
            {
                this.logger.LogInformation(
                    "Returning existing invite token for campaign {CampaignId}",
                    campaignId);
                return campaign.InviteToken;
            }

            // Generate new unique token
            campaign.InviteToken = Guid.NewGuid().ToString("N")[..16].ToUpper();
            campaign.UpdatedAt = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Successfully generated invite token for campaign {CampaignId}",
                campaignId);

            return campaign.InviteToken;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error generating invite token for campaign {CampaignId} by user {UserId}",
                campaignId,
                userId);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CampaignDto?> JoinCampaignAsync(string inviteToken, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "User {UserId} attempting to join campaign with token {Token}",
                userId,
                inviteToken);

            if (string.IsNullOrWhiteSpace(inviteToken))
            {
                this.logger.LogWarning("Invalid invite token provided");
                return null;
            }

            var campaign = await this.dbContext.Campaigns
                .Include(c => c.World)
                .FirstOrDefaultAsync(c => c.InviteToken == inviteToken && !c.IsDeleted);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign with invite token {Token} not found", inviteToken);
                return null;
            }

            // TODO: Add logic to check MaxPlayers limit
            // TODO: Create a CampaignMember/Player entity to track membership
            // For now, just return the campaign details (user can access it)

            this.logger.LogInformation(
                "User {UserId} successfully joined campaign {CampaignId}",
                userId,
                campaign.Id);

            return this.MapToDto(campaign);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error joining campaign with token {Token} for user {UserId}",
                inviteToken,
                userId);

            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<CampaignDto?> UpdateCampaignStatusAsync(int campaignId, CampaignStatus status, int userId)
    {
        try
        {
            this.logger.LogInformation(
                "Updating campaign {CampaignId} status to {Status} by user {UserId}",
                campaignId,
                status,
                userId);

            var campaign = await this.dbContext.Campaigns
                .Include(c => c.World)
                .FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);

            if (campaign == null)
            {
                this.logger.LogWarning("Campaign {CampaignId} not found", campaignId);
                return null;
            }

            // Authorization: only GM can update status
            if (campaign.CreatedBy != userId)
            {
                this.logger.LogWarning(
                    "User {UserId} not authorized to update status for campaign {CampaignId}",
                    userId,
                    campaignId);
                return null;
            }

            campaign.Status = status;
            campaign.UpdatedAt = DateTime.UtcNow;

            // Sync IsActive with Status for backward compatibility
            campaign.IsActive = status == CampaignStatus.Active;

            await this.dbContext.SaveChangesAsync();

            this.logger.LogInformation(
                "Successfully updated campaign {CampaignId} status to {Status}",
                campaignId,
                status);

            return this.MapToDto(campaign);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error updating campaign {CampaignId} status to {Status} by user {UserId}",
                campaignId,
                status,
                userId);

            return null;
        }
    }
}
