// -----------------------------------------------------------------------
// <copyright file="RefreshToken.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Represents a refresh token issued to a user for renewing JWT access tokens.
/// </summary>
public class RefreshToken
{
    /// <summary>Gets or sets the refresh token ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the opaque token string.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Gets or sets the user this token belongs to.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets when this token expires.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Gets or sets when this token was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets when this token was revoked (null if still valid).</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>Gets a value indicating whether this token is revoked.</summary>
    public bool IsRevoked => this.RevokedAt.HasValue;

    /// <summary>Gets a value indicating whether this token is expired or revoked.</summary>
    public bool IsActive => !this.IsRevoked && this.ExpiresAt > DateTime.UtcNow;

    /// <summary>Gets or sets the user navigation property.</summary>
    public virtual User User { get; set; } = null!;
}
