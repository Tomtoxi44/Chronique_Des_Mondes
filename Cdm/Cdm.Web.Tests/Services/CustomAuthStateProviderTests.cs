// -----------------------------------------------------------------------
// <copyright file="CustomAuthStateProviderTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Tests.Services;

using System.Security.Claims;
using System.Text;
using Cdm.Web.Shared.DTOs.Models;
using Cdm.Web.Shared.DTOs.ViewModels;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.State;
using Cdm.Web.Services.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

/// <summary>
/// Tests de <see cref="CustomAuthStateProvider"/>, en particulier la serialisation du
/// rafraichissement de jeton. Les refresh tokens sont a usage unique cote serveur : deux
/// rafraichissements concurrents suffisaient a deconnecter l'utilisateur, le second
/// presentant un jeton deja revoque.
/// </summary>
public class CustomAuthStateProviderTests
{
    /// <summary>
    /// Faux client d'auth : compte les appels a RefreshAsync et permet d'injecter un delai,
    /// pour reproduire la fenetre pendant laquelle plusieurs composants demandent le
    /// rafraichissement en meme temps.
    /// </summary>
    private sealed class FakeAuthApiClient : IAuthApiClient
    {
        private readonly Func<LoginResponse?> responseFactory;
        private readonly TimeSpan delay;
        private int refreshCalls;

        public FakeAuthApiClient(Func<LoginResponse?> responseFactory, TimeSpan delay)
        {
            this.responseFactory = responseFactory;
            this.delay = delay;
        }

        public int RefreshCalls => Volatile.Read(ref this.refreshCalls);

        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            Interlocked.Increment(ref this.refreshCalls);
            await Task.Delay(this.delay);
            return this.responseFactory();
        }

        public Task<RegisterResponse?> RegisterAsync(RegisterRequest request) => throw new NotSupportedException();

        public Task<LoginResponse?> LoginAsync(LoginRequest request) => throw new NotSupportedException();

        public Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request) => throw new NotSupportedException();

        public Task<bool> ResetPasswordAsync(ResetPasswordRequest request) => throw new NotSupportedException();

        public Task<bool> ConfirmEmailAsync(string token) => throw new NotSupportedException();

        public Task<int> ResendConfirmationAsync() => throw new NotSupportedException();
    }

    /// <summary>Client d'auth inerte, pour les cas ou aucun rafraichissement n'est attendu.</summary>
    private static IAuthApiClient NoRefresh() => new FakeAuthApiClient(() => null, TimeSpan.Zero);

    /// <summary>Stockage en memoire : evite l'interop JS et rend les lectures comptables.</summary>
    private sealed class FakeLocalStorage : ILocalStorageService
    {
        private readonly Dictionary<string, string> items = new(StringComparer.Ordinal);

        public int ReadCount { get; private set; }

        public Task<string?> GetItemAsync(string key)
        {
            this.ReadCount++;
            return Task.FromResult(this.items.TryGetValue(key, out var v) ? v : null);
        }

        public Task SetItemAsync(string key, string value)
        {
            this.items[key] = value;
            return Task.CompletedTask;
        }

        public Task RemoveItemAsync(string key)
        {
            this.items.Remove(key);
            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            this.items.Clear();
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Construit un JWT non signe dont seule la charge utile compte : le provider lit
    /// l'expiration sans verifier la signature (la verification a lieu cote API).
    /// </summary>
    private static string Jwt(DateTimeOffset expiry)
    {
        static string B64(string s) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(s)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return $"{B64("{\"alg\":\"none\"}")}.{B64($"{{\"exp\":{expiry.ToUnixTimeSeconds()}}}")}.signature";
    }

    private static string ValidJwt() => Jwt(DateTimeOffset.UtcNow.AddHours(1));

    private static string ExpiredJwt() => Jwt(DateTimeOffset.UtcNow.AddHours(-1));

    private static async Task<FakeLocalStorage> SeedAsync(string token, string refreshToken = "refresh-1")
    {
        var storage = new FakeLocalStorage();
        await storage.SetItemAsync("auth_token", token);
        await storage.SetItemAsync("auth_refresh_token", refreshToken);
        await storage.SetItemAsync("auth_user_id", "42");
        await storage.SetItemAsync("auth_user_email", "joueur@example.com");
        await storage.SetItemAsync("auth_user_nickname", "Joueur");
        await storage.SetItemAsync("auth_email_confirmed", "true");
        return storage;
    }

    private static CustomAuthStateProvider NewProvider(ILocalStorageService storage, IAuthApiClient authClient) =>
        new(storage, NullLogger<CustomAuthStateProvider>.Instance, authClient);

    [Fact]
    public async Task GetAuthenticationStateAsync_ValidToken_AuthenticatesUser()
    {
        var storage = await SeedAsync(ValidJwt());
        var provider = NewProvider(storage, NoRefresh());

        var state = await provider.GetAuthenticationStateAsync();

        Assert.True(state.User.Identity?.IsAuthenticated);
        Assert.Equal("joueur@example.com", state.User.FindFirst(ClaimTypes.Email)?.Value);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_NoToken_IsAnonymous()
    {
        var provider = NewProvider(new FakeLocalStorage(), NoRefresh());

        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity?.IsAuthenticated ?? false);
    }

    /// <summary>
    /// Le test de non-regression central : N appels simultanes sur un jeton expire ne doivent
    /// declencher qu'UN SEUL appel a /api/auth/refresh. Sans le verrou, chaque composant
    /// protege lancait le sien, le premier revoquait le refresh token et tous les autres
    /// echouaient -> stockage vide -> utilisateur deconnecte (et limite de debit saturee).
    /// </summary>
    [Fact]
    public async Task GetAuthenticationStateAsync_ConcurrentCallsOnExpiredToken_RefreshesExactlyOnce()
    {
        var storage = await SeedAsync(ExpiredJwt());
        var authClient = new FakeAuthApiClient(
            () => new LoginResponse
            {
                UserId = 42,
                Email = "joueur@example.com",
                Nickname = "Joueur",
                Token = ValidJwt(),
                RefreshToken = "refresh-2",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
                EmailConfirmed = true,
            },
            TimeSpan.FromMilliseconds(50));

        var provider = NewProvider(storage, authClient);

        var states = await Task.WhenAll(
            Enumerable.Range(0, 12).Select(_ => provider.GetAuthenticationStateAsync()));

        Assert.Equal(1, authClient.RefreshCalls);
        Assert.All(states, s => Assert.True(s.User.Identity?.IsAuthenticated));
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ExpiredTokenAndRefreshFails_ClearsSessionOnce()
    {
        var storage = await SeedAsync(ExpiredJwt());
        var authClient = new FakeAuthApiClient(() => null, TimeSpan.FromMilliseconds(20));
        var provider = NewProvider(storage, authClient);

        var states = await Task.WhenAll(
            Enumerable.Range(0, 5).Select(_ => provider.GetAuthenticationStateAsync()));

        // Le refresh token ayant ete efface au premier echec, les appels suivants renoncent
        // sans rappeler l'API : un seul appel reseau, et tout le monde est anonyme.
        Assert.Equal(1, authClient.RefreshCalls);
        Assert.All(states, s => Assert.False(s.User.Identity?.IsAuthenticated ?? false));
    }

    /// <summary>
    /// Reconstruire l'etat coute cinq lectures de stockage (aller-retour JS + dechiffrement).
    /// Les appels suivants sur le meme jeton doivent taper le cache.
    /// </summary>
    [Fact]
    public async Task GetAuthenticationStateAsync_RepeatedCalls_DoesNotRereadEveryKey()
    {
        var storage = await SeedAsync(ValidJwt());
        var provider = NewProvider(storage, NoRefresh());

        await provider.GetAuthenticationStateAsync();
        var afterFirst = storage.ReadCount;

        await provider.GetAuthenticationStateAsync();
        await provider.GetAuthenticationStateAsync();

        // Une seule lecture par appel (le jeton, pour verifier qu'il n'a pas change),
        // au lieu des cinq necessaires a la reconstruction complete.
        Assert.Equal(afterFirst + 2, storage.ReadCount);
    }

    [Fact]
    public async Task MarkEmailConfirmedAsync_InvalidatesCachedClaims()
    {
        var storage = await SeedAsync(ValidJwt());
        await storage.SetItemAsync("auth_email_confirmed", "false");
        var provider = NewProvider(storage, NoRefresh());

        var before = await provider.GetAuthenticationStateAsync();
        Assert.Equal("false", before.User.FindFirst("email_confirmed")?.Value);

        await provider.MarkEmailConfirmedAsync();
        var after = await provider.GetAuthenticationStateAsync();

        // Sans invalidation du cache, le bandeau « email non confirme » resterait affiche.
        Assert.Equal("true", after.User.FindFirst("email_confirmed")?.Value);
    }

    [Fact]
    public async Task MarkUserAsLoggedOutAsync_MakesSubsequentStateAnonymous()
    {
        var storage = await SeedAsync(ValidJwt());
        var provider = NewProvider(storage, NoRefresh());

        Assert.True((await provider.GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated);

        await provider.MarkUserAsLoggedOutAsync();

        Assert.False((await provider.GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated ?? false);
    }
}
