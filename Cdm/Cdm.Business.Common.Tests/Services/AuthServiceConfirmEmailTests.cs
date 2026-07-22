// <copyright file="AuthServiceConfirmEmailTests.cs" company="Chronique Des Mondes">
// Copyright (c) Chronique Des Mondes. All rights reserved.
// </copyright>

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Common.Services;
using Cdm.Common.Services;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests for <see cref="AuthService.ConfirmEmailAsync"/>, notamment son idempotence :
/// la page de confirmation rappelle l'API quand l'état d'authentification change, et les
/// scanners de messagerie ouvrent le lien avant le destinataire. Sans idempotence,
/// l'utilisateur voit « lien expiré » alors que son adresse vient d'être confirmée.
/// </summary>
public class AuthServiceConfirmEmailTests
{
    private static AuthService NewService(AppDbContext context) => new(
        context,
        new Mock<IPasswordService>().Object,
        new Mock<IJwtService>().Object,
        new Mock<ILogger<AuthService>>().Object);

    private static async Task<(AppDbContext Context, string Token)> SeedAsync(
        string dbName,
        DateTime expiresAt,
        DateTime? usedAt = null,
        bool emailConfirmed = false)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);

        var user = new User
        {
            Email = "joueur@example.com",
            Nickname = "Joueur",
            PasswordHash = "hash",
            IsActive = true,
            EmailConfirmed = emailConfirmed,
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = $"token-{dbName}";
        context.EmailConfirmationTokens.Add(new EmailConfirmationToken
        {
            Token = token,
            UserId = user.Id,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            UsedAt = usedAt,
        });
        await context.SaveChangesAsync();

        return (context, token);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ValidToken_ConfirmsUser()
    {
        var (context, token) = await SeedAsync(nameof(ConfirmEmailAsync_ValidToken_ConfirmsUser), DateTime.UtcNow.AddHours(24));
        using (context)
        {
            var result = await NewService(context).ConfirmEmailAsync(token);

            Assert.True(result.IsSuccess);
            Assert.True(await context.Users.AnyAsync(u => u.EmailConfirmed));
        }
    }

    [Fact]
    public async Task ConfirmEmailAsync_CalledTwice_SecondCallStillSucceeds()
    {
        var (context, token) = await SeedAsync(nameof(ConfirmEmailAsync_CalledTwice_SecondCallStillSucceeds), DateTime.UtcNow.AddHours(24));
        using (context)
        {
            var service = NewService(context);

            var first = await service.ConfirmEmailAsync(token);
            var second = await service.ConfirmEmailAsync(token);

            Assert.True(first.IsSuccess);
            Assert.True(second.IsSuccess);
        }
    }

    [Fact]
    public async Task ConfirmEmailAsync_ExpiredToken_Fails()
    {
        var (context, token) = await SeedAsync(nameof(ConfirmEmailAsync_ExpiredToken_Fails), DateTime.UtcNow.AddHours(-1));
        using (context)
        {
            var result = await NewService(context).ConfirmEmailAsync(token);

            Assert.False(result.IsSuccess);
            Assert.False(await context.Users.AnyAsync(u => u.EmailConfirmed));
        }
    }

    [Fact]
    public async Task ConfirmEmailAsync_UsedTokenButUserNotConfirmed_Fails()
    {
        // Jeton invalidé par l'émission d'un nouveau lien : il ne doit pas confirmer le compte.
        var (context, token) = await SeedAsync(
            nameof(ConfirmEmailAsync_UsedTokenButUserNotConfirmed_Fails),
            DateTime.UtcNow.AddHours(24),
            usedAt: DateTime.UtcNow.AddMinutes(-5));

        using (context)
        {
            var result = await NewService(context).ConfirmEmailAsync(token);

            Assert.False(result.IsSuccess);
            Assert.False(await context.Users.AnyAsync(u => u.EmailConfirmed));
        }
    }

    [Fact]
    public async Task ConfirmEmailAsync_UnknownToken_Fails()
    {
        var (context, _) = await SeedAsync(nameof(ConfirmEmailAsync_UnknownToken_Fails), DateTime.UtcNow.AddHours(24));
        using (context)
        {
            var result = await NewService(context).ConfirmEmailAsync("jeton-inconnu");

            Assert.False(result.IsSuccess);
        }
    }
}
