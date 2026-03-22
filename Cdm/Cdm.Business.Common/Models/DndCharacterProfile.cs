namespace Cdm.Business.Common.Models;

/// <summary>
/// Represents a D&D 5e character profile with game-specific attributes and business logic
/// </summary>
public class DndCharacterProfile
{
    /// <summary>
    /// Character level (1-20)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Character class (e.g., Fighter, Wizard, Rogue)
    /// </summary>
    public string? Class { get; set; }

    /// <summary>
    /// Character race (e.g., Human, Elf, Dwarf)
    /// </summary>
    public string? Race { get; set; }

    /// <summary>
    /// Character background (e.g., Soldier, Noble, Criminal)
    /// </summary>
    public string? Background { get; set; }

    /// <summary>
    /// Strength ability score (1-30)
    /// </summary>
    public int Strength { get; set; }

    /// <summary>
    /// Dexterity ability score (1-30)
    /// </summary>
    public int Dexterity { get; set; }

    /// <summary>
    /// Constitution ability score (1-30)
    /// </summary>
    public int Constitution { get; set; }

    /// <summary>
    /// Intelligence ability score (1-30)
    /// </summary>
    public int Intelligence { get; set; }

    /// <summary>
    /// Wisdom ability score (1-30)
    /// </summary>
    public int Wisdom { get; set; }

    /// <summary>
    /// Charisma ability score (1-30)
    /// </summary>
    public int Charisma { get; set; }

    /// <summary>
    /// Current hit points
    /// </summary>
    public int CurrentHitPoints { get; set; }

    /// <summary>
    /// Maximum hit points
    /// </summary>
    public int MaxHitPoints { get; set; }

    /// <summary>
    /// Armor Class (AC)
    /// </summary>
    public int ArmorClass { get; set; }

    /// <summary>
    /// Initiative bonus
    /// </summary>
    public int Initiative { get; set; }

    /// <summary>
    /// Proficiency bonus based on level
    /// </summary>
    public int ProficiencyBonus { get; set; }

    /// <summary>
    /// Character alignment (e.g., Lawful Good, Chaotic Neutral)
    /// </summary>
    public string? Alignment { get; set; }

    /// <summary>
    /// Experience points
    /// </summary>
    public int ExperiencePoints { get; set; }

    /// <summary>
    /// Calculates the ability modifier for a given ability score
    /// </summary>
    /// <param name="abilityScore">The ability score (1-30)</param>
    /// <returns>The ability modifier</returns>
    public static int GetModifier(int abilityScore)
    {
        return (abilityScore - 10) / 2;
    }

    /// <summary>
    /// Gets the Strength modifier
    /// </summary>
    public int StrengthModifier => GetModifier(Strength);

    /// <summary>
    /// Gets the Dexterity modifier
    /// </summary>
    public int DexterityModifier => GetModifier(Dexterity);

    /// <summary>
    /// Gets the Constitution modifier
    /// </summary>
    public int ConstitutionModifier => GetModifier(Constitution);

    /// <summary>
    /// Gets the Intelligence modifier
    /// </summary>
    public int IntelligenceModifier => GetModifier(Intelligence);

    /// <summary>
    /// Gets the Wisdom modifier
    /// </summary>
    public int WisdomModifier => GetModifier(Wisdom);

    /// <summary>
    /// Gets the Charisma modifier
    /// </summary>
    public int CharismaModifier => GetModifier(Charisma);

    /// <summary>
    /// Calculates proficiency bonus based on character level
    /// </summary>
    /// <param name="level">Character level (1-20)</param>
    /// <returns>Proficiency bonus</returns>
    public static int CalculateProficiencyBonus(int level)
    {
        return level switch
        {
            >= 1 and <= 4 => 2,
            >= 5 and <= 8 => 3,
            >= 9 and <= 12 => 4,
            >= 13 and <= 16 => 5,
            >= 17 and <= 20 => 6,
            _ => 2
        };
    }

    /// <summary>
    /// Applies damage to the character
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    public void TakeDamage(int damage)
    {
        CurrentHitPoints = Math.Max(0, CurrentHitPoints - damage);
    }

    /// <summary>
    /// Heals the character
    /// </summary>
    /// <param name="healing">Amount of healing to apply</param>
    public void Heal(int healing)
    {
        CurrentHitPoints = Math.Min(MaxHitPoints, CurrentHitPoints + healing);
    }

    /// <summary>
    /// Checks if the character is alive
    /// </summary>
    public bool IsAlive => CurrentHitPoints > 0;

    /// <summary>
    /// Checks if the character is at full health
    /// </summary>
    public bool IsFullHealth => CurrentHitPoints == MaxHitPoints;

    /// <summary>
    /// Gets the health percentage (0-100)
    /// </summary>
    public double HealthPercentage => MaxHitPoints > 0 
        ? (double)CurrentHitPoints / MaxHitPoints * 100 
        : 0;

    /// <summary>
    /// Validates the character profile data
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return Level is >= 1 and <= 20
            && Strength is >= 1 and <= 30
            && Dexterity is >= 1 and <= 30
            && Constitution is >= 1 and <= 30
            && Intelligence is >= 1 and <= 30
            && Wisdom is >= 1 and <= 30
            && Charisma is >= 1 and <= 30
            && CurrentHitPoints >= 0
            && MaxHitPoints > 0
            && CurrentHitPoints <= MaxHitPoints
            && ArmorClass >= 0;
    }
}
