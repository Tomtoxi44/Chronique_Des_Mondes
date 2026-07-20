using Microsoft.AspNetCore.DataProtection;

namespace Cdm.Web.Services.Storage;

/// <summary>
/// Transparent decorator over <see cref="ILocalStorageService"/> that encrypts the sensitive
/// authentication tokens (JWT access token and refresh token) with ASP.NET Core Data Protection
/// before they are written to browser localStorage, and decrypts them on read.
///
/// The plaintext token never leaves the server: what sits in the browser is an opaque, server-only
/// ciphertext, so a cross-site scripting (XSS) payload cannot steal a usable token. All other keys
/// pass through unchanged, so callers keep using <see cref="ILocalStorageService"/> as before.
/// </summary>
public class ProtectedLocalStorageService : ILocalStorageService
{
    private static readonly HashSet<string> ProtectedKeys = new(StringComparer.Ordinal)
    {
        "auth_token",
        "auth_refresh_token",
    };

    private readonly LocalStorageService inner;
    private readonly IDataProtector protector;
    private readonly ILogger<ProtectedLocalStorageService> logger;

    public ProtectedLocalStorageService(
        LocalStorageService inner,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<ProtectedLocalStorageService> logger)
    {
        this.inner = inner;
        this.protector = dataProtectionProvider.CreateProtector("Cdm.Web.AuthTokens.v1");
        this.logger = logger;
    }

    public async Task<string?> GetItemAsync(string key)
    {
        var raw = await this.inner.GetItemAsync(key);
        if (raw is null || !ProtectedKeys.Contains(key))
        {
            return raw;
        }

        try
        {
            return this.protector.Unprotect(raw);
        }
        catch
        {
            // Legacy plaintext token (pre-migration) or rotated protection key: treat as absent
            // so the user simply re-authenticates. Never log the stored value.
            this.logger.LogInformation("A stored auth token could not be decrypted (legacy or rotated key); ignoring it.");
            return null;
        }
    }

    public Task SetItemAsync(string key, string value)
    {
        if (ProtectedKeys.Contains(key) && !string.IsNullOrEmpty(value))
        {
            value = this.protector.Protect(value);
        }

        return this.inner.SetItemAsync(key, value);
    }

    public Task RemoveItemAsync(string key) => this.inner.RemoveItemAsync(key);

    public Task ClearAsync() => this.inner.ClearAsync();
}
