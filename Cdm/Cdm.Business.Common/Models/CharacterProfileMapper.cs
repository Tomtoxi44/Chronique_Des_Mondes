using System.Text.Json;
using Cdm.Common.Enums;

namespace Cdm.Business.Common.Models;

/// <summary>
/// Maps JSON game-specific data to typed character profile classes
/// </summary>
public static class CharacterProfileMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Deserializes JSON game-specific data based on game type
    /// </summary>
    /// <param name="gameSpecificDataJson">The JSON string containing game-specific data</param>
    /// <param name="gameType">The type of game system</param>
    /// <returns>Deserialized profile object or null</returns>
    public static object? DeserializeGameProfile(string? gameSpecificDataJson, GameType gameType)
    {
        if (string.IsNullOrWhiteSpace(gameSpecificDataJson))
            return null;

        try
        {
            return gameType switch
            {
                GameType.DnD5e => JsonSerializer.Deserialize<DndCharacterProfile>(gameSpecificDataJson, JsonOptions),
                GameType.Skyrim => JsonSerializer.Deserialize<SkyrimCharacterProfile>(gameSpecificDataJson, JsonOptions),
                GameType.Generic => null,
                _ => null
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Deserializes JSON to DndCharacterProfile
    /// </summary>
    /// <param name="gameSpecificDataJson">The JSON string</param>
    /// <returns>DndCharacterProfile or null</returns>
    public static DndCharacterProfile? DeserializeDndProfile(string? gameSpecificDataJson)
    {
        if (string.IsNullOrWhiteSpace(gameSpecificDataJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<DndCharacterProfile>(gameSpecificDataJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Deserializes JSON to SkyrimCharacterProfile
    /// </summary>
    /// <param name="gameSpecificDataJson">The JSON string</param>
    /// <returns>SkyrimCharacterProfile or null</returns>
    public static SkyrimCharacterProfile? DeserializeSkyrimProfile(string? gameSpecificDataJson)
    {
        if (string.IsNullOrWhiteSpace(gameSpecificDataJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<SkyrimCharacterProfile>(gameSpecificDataJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Serializes a DndCharacterProfile to JSON
    /// </summary>
    /// <param name="profile">The profile to serialize</param>
    /// <returns>JSON string</returns>
    public static string SerializeDndProfile(DndCharacterProfile profile)
    {
        return JsonSerializer.Serialize(profile, JsonOptions);
    }

    /// <summary>
    /// Serializes a SkyrimCharacterProfile to JSON
    /// </summary>
    /// <param name="profile">The profile to serialize</param>
    /// <returns>JSON string</returns>
    public static string SerializeSkyrimProfile(SkyrimCharacterProfile profile)
    {
        return JsonSerializer.Serialize(profile, JsonOptions);
    }

    /// <summary>
    /// Serializes a game profile object to JSON based on game type
    /// </summary>
    /// <param name="profile">The profile object</param>
    /// <param name="gameType">The game type</param>
    /// <returns>JSON string or null</returns>
    public static string? SerializeGameProfile(object profile, GameType gameType)
    {
        return gameType switch
        {
            GameType.DnD5e when profile is DndCharacterProfile dndProfile => SerializeDndProfile(dndProfile),
            GameType.Skyrim when profile is SkyrimCharacterProfile skyrimProfile => SerializeSkyrimProfile(skyrimProfile),
            GameType.Generic => null,
            _ => null
        };
    }

    /// <summary>
    /// Creates a default DndCharacterProfile with starting values
    /// </summary>
    /// <param name="level">Starting level</param>
    /// <returns>Default D&D character profile</returns>
    public static DndCharacterProfile CreateDefaultDndProfile(int level = 1)
    {
        var proficiencyBonus = DndCharacterProfile.CalculateProficiencyBonus(level);
        
        return new DndCharacterProfile
        {
            Level = level,
            Strength = 10,
            Dexterity = 10,
            Constitution = 10,
            Intelligence = 10,
            Wisdom = 10,
            Charisma = 10,
            MaxHitPoints = 10,
            CurrentHitPoints = 10,
            ArmorClass = 10,
            Initiative = 0,
            ProficiencyBonus = proficiencyBonus,
            ExperiencePoints = 0,
            Class = "Fighter",
            Race = "Human",
            Alignment = "Neutral"
        };
    }

    /// <summary>
    /// Creates a default SkyrimCharacterProfile with starting values
    /// </summary>
    /// <param name="level">Starting level</param>
    /// <returns>Default Skyrim character profile</returns>
    public static SkyrimCharacterProfile CreateDefaultSkyrimProfile(int level = 1)
    {
        return new SkyrimCharacterProfile
        {
            Level = level,
            Race = "Nord",
            StoneBlessing = "Warrior Stone",
            MaxHealth = 100,
            CurrentHealth = 100,
            MaxMagicka = 100,
            CurrentMagicka = 100,
            MaxStamina = 100,
            CurrentStamina = 100,
            MaxCarryWeight = 300,
            CurrentCarryWeight = 0,
            Gold = 0,
            // All skills start at base level
            OneHanded = 15,
            TwoHanded = 15,
            Archery = 15,
            Block = 15,
            Smithing = 15,
            HeavyArmor = 15,
            LightArmor = 15,
            Destruction = 15,
            Alteration = 15,
            Restoration = 15,
            Conjuration = 15,
            Illusion = 15,
            Enchanting = 15,
            Sneak = 15,
            Lockpicking = 15,
            Pickpocket = 15,
            Speech = 15,
            Alchemy = 15
        };
    }
}
