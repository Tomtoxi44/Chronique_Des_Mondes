// -----------------------------------------------------------------------
// <copyright file="EmailConfirmationToken.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Data.Common.Models;

/// <summary>
/// Jeton à usage unique permettant de confirmer l'adresse email d'un compte.
/// </summary>
public class EmailConfirmationToken
{
    /// <summary>Gets or sets the token ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the opaque token value sent by email.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Gets or sets the owning user ID.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the expiration date (UTC).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Gets or sets the creation date (UTC). Sert aussi au calcul du cooldown de renvoi.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets when the token was consumed (null if unused).</summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>Gets or sets the user navigation property.</summary>
    public virtual User User { get; set; } = null!;

    /// <summary>Gets a value indicating whether the token can still be used.</summary>
    public bool IsValid => this.UsedAt == null && this.ExpiresAt > DateTime.UtcNow;
}
