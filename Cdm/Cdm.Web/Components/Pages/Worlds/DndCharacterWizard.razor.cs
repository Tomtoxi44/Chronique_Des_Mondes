using Cdm.Business.Abstraction.DTOs.DnD5e;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Shared.DTOs.Models;
using Microsoft.AspNetCore.Components;

namespace Cdm.Web.Components.Pages.Worlds;

public partial class DndCharacterWizard
{
    [Inject] private DndApiClient DndClient { get; set; } = default!;
    [Inject] private ICharacterApiClient CharacterClient { get; set; } = default!;
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter] public int WorldId { get; set; }

    // ── State ────────────────────────────────────────────────────────────

    private bool IsLoading = true;
    private bool IsComplete = false;
    private bool HasLoadError = false;
    private bool IsSavingBase = false;
    private bool IsSavingStats = false;
    private bool IsAddingItem = false;
    private bool IsAddingSpell = false;

    private int CurrentStep = 0;
    private int WorldCharacterId = 0;
    private int CharacterId = 0;

    private readonly string[] StepLabels = { "Identité", "Stats D&D", "Inventaire", "Sorts" };

    // Step 1 — base info
    private string CharacterFirstName = string.Empty;
    private string? CharacterName;
    private int? CharacterAge;
    private string? CharacterDescription;

    // Step 2 — D&D stats
    private DndCharacterStatsDto Stats = new();
    private int SelectedRaceId = 0;
    private int SelectedClassId = 0;
    private DndRaceDto? SelectedRace;
    private DndClassDto? SelectedClass;

    // Step 3 — inventory
    private List<DndInventoryItemDto> InventoryItems = new();
    private DndInventoryItemDto NewItem = new() { Quantity = 1, Category = "Arme" };

    // Step 4 — spells
    private List<DndCharacterSpellDto> CharacterSpells = new();
    private DndCharacterSpellDto NewSpell = new();

    // Reference data
    private List<DndRaceDto> AvailableRaces = new();
    private List<DndClassDto> AvailableClasses = new();
    private List<DndBackgroundDto> AvailableBackgrounds = new();
    private List<DndItemDto> AvailableItems = new();
    private List<DndSpellDto> AvailableSpells = new();

    // Background selection
    private int SelectedBackgroundId = 0;
    private DndBackgroundDto? SelectedBackground;
    private List<string> BackgroundGrantedSkills = new();

    // Class skill choices (separate from background skills)
    private HashSet<string> ClassSkillChoices = new();

    // ── Init ─────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        // Load world character info via WorldId (current user's character in this world)
        var wc = await WorldClient.GetMyWorldCharacterAsync(WorldId);
        if (wc != null)
        {
            WorldCharacterId = wc.Id;
            CharacterId = wc.CharacterId;
        }

        // Load existing stats (if wizard was opened a second time)
        var existingStats = await DndClient.GetCharacterStatsAsync(WorldCharacterId);
        if (existingStats != null)
            Stats = existingStats;
        Stats.WorldCharacterId = WorldCharacterId;

        // Pre-fill base info from character
        if (CharacterId > 0)
        {
            var character = await CharacterClient.GetCharacterByIdAsync(CharacterId);
            if (character != null)
            {
                CharacterFirstName = character.FirstName ?? character.Name;
                CharacterName = character.Name;
                CharacterAge = character.Age;
                CharacterDescription = character.Description;
            }
        }

        // Load existing inventory and spells
        InventoryItems = await DndClient.GetInventoryAsync(WorldCharacterId);
        CharacterSpells = await DndClient.GetCharacterSpellsAsync(WorldCharacterId);

        // Load reference data in parallel
        var racesTask = DndClient.GetRacesAsync();
        var classesTask = DndClient.GetClassesAsync();
        var backgroundsTask = DndClient.GetBackgroundsAsync();
        var itemsTask = DndClient.GetItemsAsync();
        var spellsTask = DndClient.GetSpellsAsync();

        await Task.WhenAll(racesTask, classesTask, backgroundsTask, itemsTask, spellsTask);
        AvailableRaces = racesTask.Result;
        AvailableClasses = classesTask.Result;
        AvailableBackgrounds = backgroundsTask.Result;
        AvailableItems = itemsTask.Result;
        AvailableSpells = spellsTask.Result;

        if (AvailableRaces.Count == 0 || AvailableClasses.Count == 0)
            HasLoadError = true;

        // Restore selections from existing stats
        if (!string.IsNullOrEmpty(Stats.Race))
            SelectedRace = AvailableRaces.FirstOrDefault(r => r.Name == Stats.Race);
        if (!string.IsNullOrEmpty(Stats.CharacterClass))
            SelectedClass = AvailableClasses.FirstOrDefault(c => c.Name == Stats.CharacterClass);
        if (!string.IsNullOrEmpty(Stats.Background))
        {
            SelectedBackground = AvailableBackgrounds.FirstOrDefault(b => b.Name == Stats.Background);
            if (SelectedBackground != null)
            {
                SelectedBackgroundId = SelectedBackground.Id;
                BackgroundGrantedSkills = SelectedBackground.SkillProficiencies;
            }
        }
        if (SelectedRace != null) SelectedRaceId = SelectedRace.Id;
        if (SelectedClass != null) SelectedClassId = SelectedClass.Id;

        // Restore class skill choices (skills already in proficiencies that aren't from background)
        if (SelectedClass != null)
        {
            foreach (var skill in Stats.SkillProficiencies)
                if (!BackgroundGrantedSkills.Contains(skill) && SelectedClass.AvailableSkills.Contains(skill))
                    ClassSkillChoices.Add(skill);
        }

        IsLoading = false;
    }

    // ── Wizard navigation ────────────────────────────────────────────────

    private void GoToStep(int step)
    {
        if (step < CurrentStep) CurrentStep = step;
    }

    private void GoToPrevStep()
    {
        if (CurrentStep > 0) CurrentStep--;
    }

    private async Task GoToNextStep()
    {
        if (CurrentStep == 0)
            await SaveBaseInfoAsync();
        CurrentStep++;
    }

    private void RestartWizard()
    {
        IsComplete = false;
        HasLoadError = false;
        CurrentStep = 0;
    }

    private async Task RetryLoadReferenceDataAsync()
    {
        HasLoadError = false;
        var racesTask = DndClient.GetRacesAsync();
        var classesTask = DndClient.GetClassesAsync();
        var backgroundsTask = DndClient.GetBackgroundsAsync();
        await Task.WhenAll(racesTask, classesTask, backgroundsTask);
        AvailableRaces = racesTask.Result;
        AvailableClasses = classesTask.Result;
        AvailableBackgrounds = backgroundsTask.Result;
        if (AvailableRaces.Count == 0 || AvailableClasses.Count == 0)
            HasLoadError = true;
    }

    // ── Step 1 — Save base info ──────────────────────────────────────────

    private async Task SaveBaseInfoAsync()
    {
        if (CharacterId == 0 || string.IsNullOrWhiteSpace(CharacterFirstName)) return;
        IsSavingBase = true;
        await CharacterClient.UpdateCharacterAsync(CharacterId, new UpdateCharacterDto
        {
            FirstName = CharacterFirstName,
            Name = CharacterName,
            Age = CharacterAge,
            Description = CharacterDescription
        });
        IsSavingBase = false;
    }

    // ── Step 2 — Stats ───────────────────────────────────────────────────

    private void OnStatsChanged() => StateHasChanged();

    private void OnRaceSelected()
    {
        SelectedRace = AvailableRaces.FirstOrDefault(r => r.Id == SelectedRaceId);
        Stats.Race = SelectedRace?.Name;
        Stats.Subrace = null;
        // auto-fill speed from race
        if (SelectedRace != null && !Stats.Speed.HasValue)
            Stats.Speed = SelectedRace.Speed;
        StateHasChanged();
    }

    private void OnClassSelected()
    {
        var previousClass = SelectedClass;
        SelectedClass = AvailableClasses.FirstOrDefault(c => c.Id == SelectedClassId);
        Stats.CharacterClass = SelectedClass?.Name;

        // Reset class skill choices when class changes
        if (previousClass?.Name != SelectedClass?.Name)
        {
            foreach (var skill in ClassSkillChoices)
                Stats.SkillProficiencies.Remove(skill);
            ClassSkillChoices.Clear();
        }

        StateHasChanged();
    }

    private void OnBackgroundSelected()
    {
        // Remove old background skills from proficiencies
        foreach (var skill in BackgroundGrantedSkills)
            Stats.SkillProficiencies.Remove(skill);

        SelectedBackground = AvailableBackgrounds.FirstOrDefault(b => b.Id == SelectedBackgroundId);
        Stats.Background = SelectedBackground?.Name;
        BackgroundGrantedSkills = SelectedBackground?.SkillProficiencies ?? new();

        // Auto-add new background skills
        foreach (var skill in BackgroundGrantedSkills)
            if (!Stats.SkillProficiencies.Contains(skill))
                Stats.SkillProficiencies.Add(skill);

        StateHasChanged();
    }

    private int GetMaxClassSkillChoices() => Stats.CharacterClass switch
    {
        "Barde" or "Rôdeur" => 3,
        "Roublard" => 4,
        _ => 2
    };

    private bool IsSkillFromBackground(string skill) => BackgroundGrantedSkills.Contains(skill);

    private void ToggleClassSkill(string skill)
    {
        if (IsSkillFromBackground(skill)) return;

        if (ClassSkillChoices.Contains(skill))
        {
            ClassSkillChoices.Remove(skill);
            Stats.SkillProficiencies.Remove(skill);
        }
        else if (ClassSkillChoices.Count < GetMaxClassSkillChoices())
        {
            ClassSkillChoices.Add(skill);
            if (!Stats.SkillProficiencies.Contains(skill))
                Stats.SkillProficiencies.Add(skill);
        }
    }

    private async Task SaveStatsAndContinue()
    {
        IsSavingStats = true;
        await DndClient.SaveCharacterStatsAsync(WorldCharacterId, Stats);
        IsSavingStats = false;
        CurrentStep++;
    }

    private IEnumerable<string> GetDisplayBonuses()
    {
        if (SelectedRace == null) yield break;
        var bonuses = new Dictionary<string, int>(SelectedRace.StatBonuses);
        if (!string.IsNullOrEmpty(Stats.Subrace) && SelectedRace.SubraceStatBonuses.TryGetValue(Stats.Subrace, out var subBonuses))
            foreach (var (k, v) in subBonuses)
            {
                bonuses.TryGetValue(k, out var existing);
                bonuses[k] = existing + v;
            }
        foreach (var (stat, val) in bonuses)
            yield return $"{stat}: +{val}";
    }

    private string GetSuggestedHp()
    {
        if (SelectedClass == null || !Stats.Level.HasValue) return "—";
        var conMod = Stats.Constitution.HasValue ? (int)Math.Floor((Stats.Constitution.Value - 10.0) / 2.0) : 0;
        return (SelectedClass.HitDie + (Stats.Level.Value - 1) * (SelectedClass.HitDie / 2 + 1) + conMod * Stats.Level.Value).ToString();
    }

    private static int GetProficiencyBonus(int level) =>
        level switch { <= 4 => 2, <= 8 => 3, <= 12 => 4, <= 16 => 5, _ => 6 };

    // ── Step 3 — Inventory ───────────────────────────────────────────────

    private void OnReferenceItemSelected(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var id) || id == 0) return;
        var refItem = AvailableItems.FirstOrDefault(i => i.Id == id);
        if (refItem == null) return;
        NewItem = new DndInventoryItemDto
        {
            Name = refItem.Name,
            Category = refItem.Category,
            DamageDice = refItem.DamageDice,
            DamageType = refItem.DamageType,
            Quantity = 1,
            DndItemId = refItem.Id
        };
        StateHasChanged();
    }

    private async Task AddInventoryItem()
    {
        if (string.IsNullOrWhiteSpace(NewItem.Name)) return;
        IsAddingItem = true;
        var added = await DndClient.AddInventoryItemAsync(WorldCharacterId, NewItem);
        if (added != null) InventoryItems.Add(added);
        NewItem = new DndInventoryItemDto { Quantity = 1, Category = "Arme" };
        IsAddingItem = false;
    }

    private async Task RemoveInventoryItem(DndInventoryItemDto item)
    {
        if (await DndClient.RemoveInventoryItemAsync(WorldCharacterId, item.Id))
            InventoryItems.Remove(item);
    }

    private static string GetItemIcon(string category) => category switch
    {
        "Arme" => "bi-sword",
        "Armure" => "bi-shield",
        "Potion" => "bi-droplet",
        "Objet magique" => "bi-stars",
        _ => "bi-box"
    };

    // ── Step 4 — Spells ──────────────────────────────────────────────────

    private void OnReferenceSpellSelected(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var id) || id == 0) return;
        var refSpell = AvailableSpells.FirstOrDefault(s => s.Id == id);
        if (refSpell == null) return;
        NewSpell = new DndCharacterSpellDto
        {
            Name = refSpell.Name,
            Level = refSpell.Level,
            School = refSpell.School,
            Description = refSpell.Description,
            DamageDice = refSpell.DamageDice,
            DamageType = refSpell.DamageType,
            DndSpellId = refSpell.Id
        };
        StateHasChanged();
    }

    private async Task AddSpell()
    {
        if (string.IsNullOrWhiteSpace(NewSpell.Name)) return;
        IsAddingSpell = true;
        var added = await DndClient.AddSpellAsync(WorldCharacterId, NewSpell);
        if (added != null) CharacterSpells.Add(added);
        NewSpell = new DndCharacterSpellDto();
        IsAddingSpell = false;
    }

    private async Task RemoveSpell(DndCharacterSpellDto spell)
    {
        if (await DndClient.RemoveSpellAsync(WorldCharacterId, spell.Id))
            CharacterSpells.Remove(spell);
    }

    // ── Finish ───────────────────────────────────────────────────────────

    private async Task FinishWizard()
    {
        IsSavingBase = true;
        await SaveBaseInfoAsync();
        IsSavingBase = false;
        IsComplete = true;
        Nav.NavigateTo($"/world-character/{WorldCharacterId}/dnd");
    }
}
