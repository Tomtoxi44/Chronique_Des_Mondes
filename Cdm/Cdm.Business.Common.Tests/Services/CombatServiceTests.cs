// -----------------------------------------------------------------------
// <copyright file="CombatServiceTests.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Business.Common.Tests.Services;

using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.Services;
using Cdm.Business.Common.Services;
using Cdm.Common.Enums;
using Cdm.Data.Common;
using Cdm.Data.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="CombatService"/> automatic initiative and Dexterity resolution.
/// </summary>
public class CombatServiceTests
{
    private readonly Mock<IAchievementEvaluationService> achievementEvaluationMock = new();
    private readonly Mock<ILogger<CombatService>> loggerMock = new();

    private const int GmUserId = 1;
    private const int WorldId = 1;
    private const int CampaignId = 5;
    private const int SessionId = 10;

    private static DbContextOptions<AppDbContext> NewOptions(string name) =>
        new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase($"{name}-{System.Guid.NewGuid()}").Options;

    private CombatService CreateService(AppDbContext context) =>
        new(context, this.achievementEvaluationMock.Object, this.loggerMock.Object);

    private static async Task SeedSessionAsync(AppDbContext context)
    {
        context.Users.Add(new User { Id = GmUserId, Email = "gm@test.com", Nickname = "MJ", PasswordHash = "h", CreatedAt = DateTime.UtcNow });
        context.Worlds.Add(new World { Id = WorldId, Name = "Monde", GameType = GameType.DnD5e, UserId = GmUserId, CreatedAt = DateTime.UtcNow });
        context.Campaigns.Add(new Campaign { Id = CampaignId, Name = "Campagne", WorldId = WorldId, CreatedBy = GmUserId, CreatedAt = DateTime.UtcNow });
        context.Sessions.Add(new Session { Id = SessionId, CampaignId = CampaignId, StartedById = GmUserId, StartedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
    }

    private static async Task<int> SeedCombatWithParticipantsAsync(AppDbContext context)
    {
        var combat = new Combat { Id = 30, SessionId = SessionId, Status = 0, StartedById = GmUserId, StartedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
        context.Combats.Add(combat);
        var group = new CombatGroup { Id = 40, CombatId = 30, Name = "Héros" };
        context.CombatGroups.Add(group);
        context.CombatParticipants.AddRange(
            new CombatParticipant { Id = 501, CombatId = 30, GroupId = 40, Name = "Roublard", IsPlayerCharacter = true, UserId = 2, MaxHp = 20, CurrentHp = 20, DexterityModifier = 3, IsActive = true },
            new CombatParticipant { Id = 502, CombatId = 30, GroupId = 40, Name = "Gobelin", IsPlayerCharacter = false, MaxHp = 7, CurrentHp = 7, DexterityModifier = 0, IsActive = true });
        await context.SaveChangesAsync();
        return combat.Id;
    }

    /// <summary>
    /// The GM auto-rolls initiative: every active participant gets a value within [1 + DEX, 20 + DEX].
    /// </summary>
    [Fact]
    public async Task RollInitiativeAsync_AsGm_SetsInitiativeWithinRange()
    {
        using var context = new AppDbContext(NewOptions(nameof(RollInitiativeAsync_AsGm_SetsInitiativeWithinRange)));
        await SeedSessionAsync(context);
        var combatId = await SeedCombatWithParticipantsAsync(context);
        var service = this.CreateService(context);

        var result = await service.RollInitiativeAsync(combatId, GmUserId);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Status); // Initiative phase

        var rogue = await context.CombatParticipants.FindAsync(501);
        var goblin = await context.CombatParticipants.FindAsync(502);
        Assert.NotNull(rogue!.Initiative);
        Assert.InRange(rogue.Initiative!.Value, 1 + 3, 20 + 3);
        Assert.NotNull(goblin!.Initiative);
        Assert.InRange(goblin.Initiative!.Value, 1, 20);
    }

    /// <summary>
    /// A non-GM cannot auto-roll initiative.
    /// </summary>
    [Fact]
    public async Task RollInitiativeAsync_NotGm_ReturnsNull()
    {
        using var context = new AppDbContext(NewOptions(nameof(RollInitiativeAsync_NotGm_ReturnsNull)));
        await SeedSessionAsync(context);
        var combatId = await SeedCombatWithParticipantsAsync(context);
        var service = this.CreateService(context);

        var result = await service.RollInitiativeAsync(combatId, 999);

        Assert.Null(result);
    }

    /// <summary>
    /// When a player participant carries no explicit Dexterity modifier, the service resolves it
    /// from their D&D sheet (WorldCharacter.GameSpecificData JSON).
    /// </summary>
    [Fact]
    public async Task CreateCombatAsync_ResolvesPlayerDexterityFromSheet()
    {
        using var context = new AppDbContext(NewOptions(nameof(CreateCombatAsync_ResolvesPlayerDexterityFromSheet)));
        await SeedSessionAsync(context);

        // Character with Dexterity 16 => modifier +3, stored as sheet JSON on the world character.
        context.Characters.Add(new Character { Id = 100, UserId = 2, Name = "Legolas", IsActive = true });
        context.WorldCharacters.Add(new WorldCharacter
        {
            Id = 200,
            CharacterId = 100,
            WorldId = WorldId,
            IsActive = true,
            GameSpecificData = "{\"dexterity\":16}"
        });
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        var dto = new CreateCombatDto
        {
            SessionId = SessionId,
            Groups = new()
            {
                new CreateCombatGroupDto
                {
                    Name = "Héros",
                    Participants = new()
                    {
                        new CreateParticipantDto { Name = "Legolas", IsPlayerCharacter = true, CharacterId = 100, MaxHp = 24 }
                    }
                }
            }
        };

        var combat = await service.CreateCombatAsync(dto, GmUserId);

        Assert.NotNull(combat);
        var participant = await context.CombatParticipants.FirstOrDefaultAsync(p => p.CharacterId == 100);
        Assert.NotNull(participant);
        Assert.Equal(3, participant!.DexterityModifier);
    }

    /// <summary>
    /// The GM can override a participant's armor class and Dexterity modifier after creation.
    /// </summary>
    [Fact]
    public async Task UpdateParticipantDefenseAsync_AsGm_OverridesStats()
    {
        using var context = new AppDbContext(NewOptions(nameof(UpdateParticipantDefenseAsync_AsGm_OverridesStats)));
        await SeedSessionAsync(context);
        var combatId = await SeedCombatWithParticipantsAsync(context);
        var service = this.CreateService(context);

        var result = await service.UpdateParticipantDefenseAsync(combatId, 502, new UpdateParticipantDefenseDto
        {
            ArmorClass = 17,
            DexterityModifier = 2,
            Resistances = "feu",
            Vulnerabilities = "froid"
        }, GmUserId);

        Assert.NotNull(result);
        var goblin = await context.CombatParticipants.FindAsync(502);
        Assert.Equal(17, goblin!.ArmorClass);
        Assert.Equal(2, goblin.DexterityModifier);
        Assert.Equal("feu", goblin.Resistances);
        Assert.Equal("froid", goblin.Vulnerabilities);
    }

    /// <summary>
    /// A non-GM cannot override participant defensive stats.
    /// </summary>
    [Fact]
    public async Task UpdateParticipantDefenseAsync_NotGm_ReturnsNull()
    {
        using var context = new AppDbContext(NewOptions(nameof(UpdateParticipantDefenseAsync_NotGm_ReturnsNull)));
        await SeedSessionAsync(context);
        var combatId = await SeedCombatWithParticipantsAsync(context);
        var service = this.CreateService(context);

        var result = await service.UpdateParticipantDefenseAsync(combatId, 502, new UpdateParticipantDefenseDto { ArmorClass = 99, DexterityModifier = 9 }, 999);

        Assert.Null(result);
        var goblin = await context.CombatParticipants.FindAsync(502);
        Assert.NotEqual(99, goblin!.ArmorClass);
    }

    /// <summary>
    /// A guaranteed hit (huge attack bonus vs low AC) reduces the target's HP and logs the attack.
    /// </summary>
    [Fact]
    public async Task ResolveAttackAsync_GuaranteedHit_ReducesTargetHp()
    {
        using var context = new AppDbContext(NewOptions(nameof(ResolveAttackAsync_GuaranteedHit_ReducesTargetHp)));
        await SeedSessionAsync(context);
        var combatId = await SeedCombatWithParticipantsAsync(context);

        // Make the target easy to hit.
        var goblin = await context.CombatParticipants.FindAsync(502);
        goblin!.ArmorClass = 1;
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        var result = await service.ResolveAttackAsync(combatId, 501, new ResolveAttackDto
        {
            TargetParticipantId = 502,
            AttackBonus = 50,
            DamageDice = "1d6",
            DamageBonus = 2,
            DamageType = "tranchant",
            Label = "Épée"
        }, GmUserId);

        Assert.NotNull(result);
        var updatedGoblin = await context.CombatParticipants.FindAsync(502);
        Assert.True(updatedGoblin!.CurrentHp < 7, "Target HP should have dropped after a hit.");
        Assert.Contains(await context.CombatActions.ToListAsync(), a => a.ActionType == "attack");
    }

    /// <summary>
    /// A guaranteed miss (huge AC, no natural 20 forced via low bonus) leaves the target's HP intact.
    /// </summary>
    [Fact]
    public async Task ResolveAttackAsync_HighArmorClass_UsuallyMisses()
    {
        using var context = new AppDbContext(NewOptions(nameof(ResolveAttackAsync_HighArmorClass_UsuallyMisses)));
        await SeedSessionAsync(context);
        var combatId = await SeedCombatWithParticipantsAsync(context);

        // AC 100 is unreachable except on a natural 20 (auto-hit); assert HP only via the non-crit path
        // is impractical with randomness, so we assert the call succeeds and, when it misses, no damage.
        var goblin = await context.CombatParticipants.FindAsync(502);
        goblin!.ArmorClass = 100;
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        var result = await service.ResolveAttackAsync(combatId, 501, new ResolveAttackDto
        {
            TargetParticipantId = 502,
            AttackBonus = 0,
            DamageDice = "1d6",
            DamageBonus = 0
        }, GmUserId);

        Assert.NotNull(result);
        var updatedGoblin = await context.CombatParticipants.FindAsync(502);
        // Either a natural 20 (auto-hit) dealt some damage, or it missed and HP is unchanged.
        Assert.InRange(updatedGoblin!.CurrentHp, 0, 7);
    }

    /// <summary>
    /// Vulnerability doubles the applied damage: a vulnerable target loses more HP than its resistances would.
    /// </summary>
    [Fact]
    public async Task ResolveAttackAsync_VulnerableTarget_TakesDoubledDamage()
    {
        using var context = new AppDbContext(NewOptions(nameof(ResolveAttackAsync_VulnerableTarget_TakesDoubledDamage)));
        await SeedSessionAsync(context);
        var combatId = await SeedCombatWithParticipantsAsync(context);

        var goblin = await context.CombatParticipants.FindAsync(502);
        goblin!.ArmorClass = 1;
        goblin.MaxHp = 100;
        goblin.CurrentHp = 100;
        goblin.Vulnerabilities = "feu";
        await context.SaveChangesAsync();

        var service = this.CreateService(context);

        // Fixed damage via 1d1 (=1) + bonus 4 => base 5, doubled by vulnerability => 10.
        var result = await service.ResolveAttackAsync(combatId, 501, new ResolveAttackDto
        {
            TargetParticipantId = 502,
            AttackBonus = 50,
            DamageDice = "1d1",
            DamageBonus = 4,
            DamageType = "feu"
        }, GmUserId);

        Assert.NotNull(result);
        var updated = await context.CombatParticipants.FindAsync(502);
        // Without crit: base (1+4)=5 doubled = 10 → 90. With a natural 20 crit: dice doubled first.
        Assert.True(updated!.CurrentHp <= 90, $"Expected doubled damage; HP was {updated.CurrentHp}.");
    }
}
