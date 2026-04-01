namespace Cdm.Business.Common.Models;

/// <summary>
/// Represents a Skyrim character profile with game-specific attributes and business logic
/// </summary>
public class SkyrimCharacterProfile
{
    /// <summary>
    /// Character level (1-81+)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Character race (e.g., Nord, Imperial, Khajiit)
    /// </summary>
    public string? Race { get; set; }

    /// <summary>
    /// Character's chosen stone blessing (e.g., Warrior, Mage, Thief)
    /// </summary>
    public string? StoneBlessing { get; set; }

    #region Core Attributes

    /// <summary>
    /// Current Health
    /// </summary>
    public int CurrentHealth { get; set; }

    /// <summary>
    /// Maximum Health
    /// </summary>
    public int MaxHealth { get; set; }

    /// <summary>
    /// Current Magicka
    /// </summary>
    public int CurrentMagicka { get; set; }

    /// <summary>
    /// Maximum Magicka
    /// </summary>
    public int MaxMagicka { get; set; }

    /// <summary>
    /// Current Stamina
    /// </summary>
    public int CurrentStamina { get; set; }

    /// <summary>
    /// Maximum Stamina
    /// </summary>
    public int MaxStamina { get; set; }

    #endregion

    #region Combat Skills

    /// <summary>
    /// One-Handed weapon skill (0-100)
    /// </summary>
    public int OneHanded { get; set; }

    /// <summary>
    /// Two-Handed weapon skill (0-100)
    /// </summary>
    public int TwoHanded { get; set; }

    /// <summary>
    /// Archery skill (0-100)
    /// </summary>
    public int Archery { get; set; }

    /// <summary>
    /// Block skill (0-100)
    /// </summary>
    public int Block { get; set; }

    /// <summary>
    /// Smithing skill (0-100)
    /// </summary>
    public int Smithing { get; set; }

    /// <summary>
    /// Heavy Armor skill (0-100)
    /// </summary>
    public int HeavyArmor { get; set; }

    /// <summary>
    /// Light Armor skill (0-100)
    /// </summary>
    public int LightArmor { get; set; }

    #endregion

    #region Magic Skills

    /// <summary>
    /// Destruction magic skill (0-100)
    /// </summary>
    public int Destruction { get; set; }

    /// <summary>
    /// Alteration magic skill (0-100)
    /// </summary>
    public int Alteration { get; set; }

    /// <summary>
    /// Restoration magic skill (0-100)
    /// </summary>
    public int Restoration { get; set; }

    /// <summary>
    /// Conjuration magic skill (0-100)
    /// </summary>
    public int Conjuration { get; set; }

    /// <summary>
    /// Illusion magic skill (0-100)
    /// </summary>
    public int Illusion { get; set; }

    /// <summary>
    /// Enchanting skill (0-100)
    /// </summary>
    public int Enchanting { get; set; }

    #endregion

    #region Stealth Skills

    /// <summary>
    /// Sneak skill (0-100)
    /// </summary>
    public int Sneak { get; set; }

    /// <summary>
    /// Lockpicking skill (0-100)
    /// </summary>
    public int Lockpicking { get; set; }

    /// <summary>
    /// Pickpocket skill (0-100)
    /// </summary>
    public int Pickpocket { get; set; }

    /// <summary>
    /// Speech skill (0-100)
    /// </summary>
    public int Speech { get; set; }

    /// <summary>
    /// Alchemy skill (0-100)
    /// </summary>
    public int Alchemy { get; set; }

    #endregion

    #region Carry Weight and Gold

    /// <summary>
    /// Current carry weight
    /// </summary>
    public int CurrentCarryWeight { get; set; }

    /// <summary>
    /// Maximum carry weight capacity
    /// </summary>
    public int MaxCarryWeight { get; set; }

    /// <summary>
    /// Amount of gold septims
    /// </summary>
    public int Gold { get; set; }

    #endregion

    /// <summary>
    /// Applies damage to the character's health
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    public void TakeDamage(int damage)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - damage);
    }

    /// <summary>
    /// Heals the character
    /// </summary>
    /// <param name="healing">Amount of healing to apply</param>
    public void Heal(int healing)
    {
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + healing);
    }

    /// <summary>
    /// Consumes magicka for spellcasting
    /// </summary>
    /// <param name="magickaCost">Amount of magicka to consume</param>
    /// <returns>True if enough magicka was available, false otherwise</returns>
    public bool ConsumeMagicka(int magickaCost)
    {
        if (CurrentMagicka < magickaCost)
            return false;

        CurrentMagicka -= magickaCost;
        return true;
    }

    /// <summary>
    /// Restores magicka
    /// </summary>
    /// <param name="amount">Amount of magicka to restore</param>
    public void RestoreMagicka(int amount)
    {
        CurrentMagicka = Math.Min(MaxMagicka, CurrentMagicka + amount);
    }

    /// <summary>
    /// Consumes stamina for actions
    /// </summary>
    /// <param name="staminaCost">Amount of stamina to consume</param>
    /// <returns>True if enough stamina was available, false otherwise</returns>
    public bool ConsumeStamina(int staminaCost)
    {
        if (CurrentStamina < staminaCost)
            return false;

        CurrentStamina -= staminaCost;
        return true;
    }

    /// <summary>
    /// Restores stamina
    /// </summary>
    /// <param name="amount">Amount of stamina to restore</param>
    public void RestoreStamina(int amount)
    {
        CurrentStamina = Math.Min(MaxStamina, CurrentStamina + amount);
    }

    /// <summary>
    /// Checks if the character is alive
    /// </summary>
    public bool IsAlive => CurrentHealth > 0;

    /// <summary>
    /// Checks if the character is at full health
    /// </summary>
    public bool IsFullHealth => CurrentHealth == MaxHealth;

    /// <summary>
    /// Gets the health percentage (0-100)
    /// </summary>
    public double HealthPercentage => MaxHealth > 0
        ? (double)CurrentHealth / MaxHealth * 100
        : 0;

    /// <summary>
    /// Gets the magicka percentage (0-100)
    /// </summary>
    public double MagickaPercentage => MaxMagicka > 0
        ? (double)CurrentMagicka / MaxMagicka * 100
        : 0;

    /// <summary>
    /// Gets the stamina percentage (0-100)
    /// </summary>
    public double StaminaPercentage => MaxStamina > 0
        ? (double)CurrentStamina / MaxStamina * 100
        : 0;

    /// <summary>
    /// Checks if character is over-encumbered
    /// </summary>
    public bool IsOverEncumbered => CurrentCarryWeight > MaxCarryWeight;

    /// <summary>
    /// Gets available carry weight
    /// </summary>
    public int AvailableCarryWeight => Math.Max(0, MaxCarryWeight - CurrentCarryWeight);

    /// <summary>
    /// Validates the character profile data
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return Level >= 1
            && CurrentHealth >= 0 && MaxHealth > 0 && CurrentHealth <= MaxHealth
            && CurrentMagicka >= 0 && MaxMagicka >= 0 && CurrentMagicka <= MaxMagicka
            && CurrentStamina >= 0 && MaxStamina >= 0 && CurrentStamina <= MaxStamina
            && OneHanded is >= 0 and <= 100
            && TwoHanded is >= 0 and <= 100
            && Archery is >= 0 and <= 100
            && Block is >= 0 and <= 100
            && Smithing is >= 0 and <= 100
            && HeavyArmor is >= 0 and <= 100
            && LightArmor is >= 0 and <= 100
            && Destruction is >= 0 and <= 100
            && Alteration is >= 0 and <= 100
            && Restoration is >= 0 and <= 100
            && Conjuration is >= 0 and <= 100
            && Illusion is >= 0 and <= 100
            && Enchanting is >= 0 and <= 100
            && Sneak is >= 0 and <= 100
            && Lockpicking is >= 0 and <= 100
            && Pickpocket is >= 0 and <= 100
            && Speech is >= 0 and <= 100
            && Alchemy is >= 0 and <= 100
            && CurrentCarryWeight >= 0
            && MaxCarryWeight > 0
            && Gold >= 0;
    }
}
