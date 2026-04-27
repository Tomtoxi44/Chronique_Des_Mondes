using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.DTOs.DnD5e;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;

namespace Cdm.Web.Components.Pages.Characters;

public partial class WorldCharacterDndDetail
{
    [Inject] private DndApiClient DndClient { get; set; } = default!;
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter] public int WorldCharacterId { get; set; }

    // ── State ─────────────────────────────────────────────────────────────

    private bool IsLoading = true;
    private string? ErrorMessage;
    private string ActiveTab = "fiche";

    // ── Data ──────────────────────────────────────────────────────────────

    private WorldCharacterDto? WorldCharacter;
    private DndCharacterStatsDto Stats = new();
    private List<DndInventoryItemDto> InventoryItems = new();
    private List<DndCharacterSpellDto> CharacterSpells = new();

    // Reference data
    private List<DndItemDto> AvailableItems = new();
    private List<DndSpellDto> AvailableSpells = new();

    // ── Stats edit ────────────────────────────────────────────────────────

    private bool IsEditingStats = false;
    private bool IsSavingStats = false;
    private string? StatsSaveMessage;
    private bool StatsSaveSuccess = false;

    // ── Skills ────────────────────────────────────────────────────────────

    private bool IsSavingSkills = false;

    // ── Item modal ────────────────────────────────────────────────────────

    private bool ShowItemModal = false;
    private string ItemModalTab = "library";
    private DndInventoryItemDto NewItem = new() { Quantity = 1, Category = "Arme" };
    private string LibraryItemFilter = string.Empty;
    private bool IsAddingItem = false;

    // ── Spell modal ───────────────────────────────────────────────────────

    private bool ShowSpellModal = false;
    private string SpellModalTab = "library";
    private DndCharacterSpellDto NewSpell = new();
    private string LibrarySpellFilter = string.Empty;
    private int? SpellLevelFilter = null;
    private bool IsAddingSpell = false;

    private HashSet<int> ExpandedSpells = new();

    // ── Skill definitions ─────────────────────────────────────────────────

    private static readonly (string Name, string Ability)[] AllSkills =
    [
        ("Acrobaties", "DEX"), ("Arcanes", "INT"), ("Athlétisme", "FOR"),
        ("Discrétion", "DEX"), ("Dressage", "SAG"), ("Escamotage", "DEX"),
        ("Histoire", "INT"), ("Intimidation", "CHA"), ("Investigation", "INT"),
        ("Médecine", "SAG"), ("Nature", "INT"), ("Perception", "SAG"),
        ("Perspicacité", "SAG"), ("Persuasion", "CHA"), ("Religion", "INT"),
        ("Représentation", "CHA"), ("Survie", "SAG"), ("Tromperie", "CHA"),
    ];

    private static readonly (string Code, string Label)[] SavingThrows =
    [
        ("FOR", "Force"), ("DEX", "Dextérité"), ("CON", "Constitution"),
        ("INT", "Intelligence"), ("SAG", "Sagesse"), ("CHA", "Charisme"),
    ];

    // ── Init ──────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        try
        {
            var wcTask = WorldClient.GetWorldCharacterByIdAsync(WorldCharacterId);
            var statsTask = DndClient.GetCharacterStatsAsync(WorldCharacterId);
            var inventoryTask = DndClient.GetInventoryAsync(WorldCharacterId);
            var spellsTask = DndClient.GetCharacterSpellsAsync(WorldCharacterId);
            var itemsTask = DndClient.GetItemsAsync();
            var spellsRefTask = DndClient.GetSpellsAsync();

            await Task.WhenAll(wcTask, statsTask, inventoryTask, spellsTask, itemsTask, spellsRefTask);

            WorldCharacter = wcTask.Result;
            Stats = statsTask.Result ?? new DndCharacterStatsDto { WorldCharacterId = WorldCharacterId };
            Stats.WorldCharacterId = WorldCharacterId;
            InventoryItems = inventoryTask.Result;
            CharacterSpells = spellsTask.Result;
            AvailableItems = itemsTask.Result;
            AvailableSpells = spellsRefTask.Result;
        }
        catch
        {
            ErrorMessage = "Impossible de charger la fiche du personnage.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Computed helpers ──────────────────────────────────────────────────

    private static int CalcMod(int? score) => score.HasValue ? (score.Value - 10) / 2 : 0;

    private static string Fmt(int mod) => mod >= 0 ? $"+{mod}" : $"{mod}";

    private int ProfBonus => CalcProfBonus(Stats.Level ?? 1);

    private static int CalcProfBonus(int level) =>
        level switch { <= 4 => 2, <= 8 => 3, <= 12 => 4, <= 16 => 5, _ => 6 };

    private int AbilityModByCode(string code) => code switch
    {
        "FOR" => CalcMod(Stats.Strength),
        "DEX" => CalcMod(Stats.Dexterity),
        "CON" => CalcMod(Stats.Constitution),
        "INT" => CalcMod(Stats.Intelligence),
        "SAG" => CalcMod(Stats.Wisdom),
        "CHA" => CalcMod(Stats.Charisma),
        _ => 0
    };

    private bool IsSkillProficient(string skillName) =>
        Stats.SkillProficiencies.Contains(skillName);

    private void ToggleSkillProficiency(string skillName)
    {
        if (!Stats.SkillProficiencies.Remove(skillName))
            Stats.SkillProficiencies.Add(skillName);
    }

    private bool IsSavingThrowProficient(string code) =>
        Stats.SavingThrowProficiencies.Contains(code);

    private void ToggleSavingThrow(string code)
    {
        if (!Stats.SavingThrowProficiencies.Remove(code))
            Stats.SavingThrowProficiencies.Add(code);
    }

    private int GetSkillTotal(string skillName, string abilityCode) =>
        AbilityModByCode(abilityCode) + (IsSkillProficient(skillName) ? ProfBonus : 0);

    // ── Stats ──────────────────────────────────────────────────────────────

    private void BeginEditStats()
    {
        IsEditingStats = true;
        StatsSaveMessage = null;
    }

    private void CancelEditStats() => IsEditingStats = false;

    private async Task SaveStats()
    {
        IsSavingStats = true;
        StatsSaveMessage = null;
        var ok = await DndClient.SaveCharacterStatsAsync(WorldCharacterId, Stats);
        StatsSaveSuccess = ok;
        StatsSaveMessage = ok ? "Fiche sauvegardée." : "Erreur lors de la sauvegarde.";
        if (ok) IsEditingStats = false;
        IsSavingStats = false;
    }

    private async Task SaveSkillProficiencies()
    {
        IsSavingSkills = true;
        await DndClient.SaveCharacterStatsAsync(WorldCharacterId, Stats);
        IsSavingSkills = false;
    }

    // ── Inventory ─────────────────────────────────────────────────────────

    private void OpenItemModal()
    {
        NewItem = new DndInventoryItemDto { Quantity = 1, Category = "Arme" };
        LibraryItemFilter = string.Empty;
        ItemModalTab = "library";
        ShowItemModal = true;
    }

    private async Task AddItemFromLibrary(DndItemDto refItem)
    {
        IsAddingItem = true;
        var dto = new DndInventoryItemDto
        {
            Name = refItem.Name,
            Category = refItem.Category,
            DamageDice = refItem.DamageDice,
            DamageType = refItem.DamageType,
            Quantity = 1,
            DndItemId = refItem.Id
        };
        var added = await DndClient.AddInventoryItemAsync(WorldCharacterId, dto);
        if (added != null) InventoryItems.Add(added);
        IsAddingItem = false;
        ShowItemModal = false;
    }

    private async Task AddCustomItem()
    {
        if (string.IsNullOrWhiteSpace(NewItem.Name)) return;
        IsAddingItem = true;
        // Compute attack bonus for weapons
        if (NewItem.Category == "Arme" && !NewItem.AttackBonus.HasValue && !string.IsNullOrEmpty(NewItem.DamageDice))
            NewItem.AttackBonus = CalcMod(Stats.Strength);
        var added = await DndClient.AddInventoryItemAsync(WorldCharacterId, NewItem);
        if (added != null) InventoryItems.Add(added);
        NewItem = new DndInventoryItemDto { Quantity = 1, Category = "Arme" };
        IsAddingItem = false;
        ShowItemModal = false;
    }

    private async Task RemoveInventoryItem(DndInventoryItemDto item)
    {
        if (await DndClient.RemoveInventoryItemAsync(WorldCharacterId, item.Id))
            InventoryItems.Remove(item);
    }

    private static string GetItemIcon(string category) => category switch
    {
        "Arme" => "bi-sword",
        "Armure" => "bi-shield-fill",
        "Potion" => "bi-droplet-fill",
        "Objet magique" => "bi-stars",
        _ => "bi-box-seam"
    };

    private IEnumerable<DndInventoryItemDto> GetItemsByCategory(string cat) =>
        InventoryItems.Where(i => i.Category == cat);

    private IEnumerable<string> ItemCategories =>
        InventoryItems.Select(i => i.Category).Distinct().OrderBy(c => c);

    private IEnumerable<DndItemDto> FilteredLibraryItems =>
        string.IsNullOrWhiteSpace(LibraryItemFilter)
            ? AvailableItems.Take(30)
            : AvailableItems.Where(i =>
                i.Name.Contains(LibraryItemFilter, StringComparison.OrdinalIgnoreCase) ||
                i.Category.Contains(LibraryItemFilter, StringComparison.OrdinalIgnoreCase));

    // ── Spells ─────────────────────────────────────────────────────────────

    private void OpenSpellModal()
    {
        NewSpell = new DndCharacterSpellDto();
        LibrarySpellFilter = string.Empty;
        SpellLevelFilter = null;
        SpellModalTab = "library";
        ShowSpellModal = true;
    }

    private async Task AddSpellFromLibrary(DndSpellDto refSpell)
    {
        IsAddingSpell = true;
        var dto = new DndCharacterSpellDto
        {
            Name = refSpell.Name,
            Level = refSpell.Level,
            School = refSpell.School,
            Description = refSpell.Description,
            DamageDice = refSpell.DamageDice,
            DamageType = refSpell.DamageType,
            DndSpellId = refSpell.Id
        };
        var added = await DndClient.AddSpellAsync(WorldCharacterId, dto);
        if (added != null) CharacterSpells.Add(added);
        IsAddingSpell = false;
        ShowSpellModal = false;
    }

    private async Task AddCustomSpell()
    {
        if (string.IsNullOrWhiteSpace(NewSpell.Name)) return;
        IsAddingSpell = true;
        var added = await DndClient.AddSpellAsync(WorldCharacterId, NewSpell);
        if (added != null) CharacterSpells.Add(added);
        NewSpell = new DndCharacterSpellDto();
        IsAddingSpell = false;
        ShowSpellModal = false;
    }

    private async Task RemoveSpell(DndCharacterSpellDto spell)
    {
        if (await DndClient.RemoveSpellAsync(WorldCharacterId, spell.Id))
            CharacterSpells.Remove(spell);
    }

    private void ToggleSpellDescription(int spellId)
    {
        if (!ExpandedSpells.Remove(spellId))
            ExpandedSpells.Add(spellId);
    }

    private static string GetSpellLevelLabel(int level) =>
        level == 0 ? "Tours de magie" : $"Niveau {level}";

    private static string GetSchoolColor(string? school) => school switch
    {
        "Abjuration" => "var(--color-info)",
        "Évocation" => "var(--color-error)",
        "Nécromancie" => "var(--color-violet-500)",
        "Transmutation" => "var(--color-success)",
        "Enchantement" => "var(--color-rose-400)",
        "Illusion" => "var(--color-indigo-400)",
        "Invocation" => "var(--color-warning)",
        "Divination" => "var(--color-indigo-300)",
        _ => "var(--color-text-muted)"
    };

    private IEnumerable<DndSpellDto> FilteredLibrarySpells
    {
        get
        {
            var spells = AvailableSpells.AsEnumerable();
            if (SpellLevelFilter.HasValue) spells = spells.Where(s => s.Level == SpellLevelFilter.Value);
            if (!string.IsNullOrWhiteSpace(LibrarySpellFilter))
                spells = spells.Where(s => s.Name.Contains(LibrarySpellFilter, StringComparison.OrdinalIgnoreCase));
            return spells.Take(40);
        }
    }

    private IEnumerable<int> SpellLevelsPresent =>
        CharacterSpells.Select(s => s.Level).Distinct().OrderBy(l => l);
}
