using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Components.Shared;
using Cdm.Web.Services;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace Cdm.Web.Components.Pages.Worlds;

public partial class WorldMyCharacter
{
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter] public int WorldId { get; set; }

    private WorldDto? World;
    private WorldCharacterDto? WorldCharacter;
    private bool IsLoading = true;
    private bool IsEditing = false;
    private bool IsSaving = false;
    private string? SaveMessage;
    private bool SaveSuccess = false;

    // Generic edit state
    private int? EditLevel;
    private int? EditCurrentHealth;
    private int? EditMaxHealth;

    // Game-specific edit state (D&D 5e)
    private string? EditDndClass;
    private string? EditDndRace;
    private string? EditDndBackground;
    private int? EditDndStrength;
    private int? EditDndDexterity;
    private int? EditDndConstitution;
    private int? EditDndIntelligence;
    private int? EditDndWisdom;
    private int? EditDndCharisma;

    // Parsed game-specific data
    private Dictionary<string, string> GameSpecificFields { get; set; } = new();

    private GameType WorldGameType => World?.GameType ?? GameType.Empty;

    private List<BreadcrumbItem> Breadcrumbs => new()
    {
        new BreadcrumbItem("Mondes", "/worlds"),
        new BreadcrumbItem(World?.Name ?? "...", $"/worlds/{WorldId}"),
        new BreadcrumbItem("Mon personnage")
    };

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        try
        {
            World = await WorldClient.GetWorldByIdAsync(WorldId);
            WorldCharacter = await WorldClient.GetMyWorldCharacterAsync(WorldId);
            ParseGameSpecificData();

            if (World != null)
            {
                NavContext.SetContext(
                    sectionTitle: World.Name,
                    backHref: $"/worlds/{WorldId}",
                    backLabel: World.Name,
                    items: new List<SecondaryNavItem>
                    {
                        new("Vue d'ensemble", $"/worlds/{WorldId}", "bi-info-circle"),
                        new("Mon personnage", $"/worlds/{WorldId}/my-character", "bi-person-badge", IsActive: true),
                    },
                    gameType: World.GameType
                );
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ParseGameSpecificData()
    {
        GameSpecificFields = new();
        if (string.IsNullOrWhiteSpace(WorldCharacter?.GameSpecificData)) return;
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(WorldCharacter.GameSpecificData);
            if (dict == null) return;
            foreach (var kv in dict)
                GameSpecificFields[kv.Key] = kv.Value.ToString();
        }
        catch { /* ignore malformed JSON */ }
    }

    private void BeginEdit()
    {
        if (WorldCharacter == null) return;
        EditLevel = WorldCharacter.Level;
        EditCurrentHealth = WorldCharacter.CurrentHealth;
        EditMaxHealth = WorldCharacter.MaxHealth;

        // Populate D&D fields from GameSpecificData
        EditDndClass = GetField("class");
        EditDndRace = GetField("race");
        EditDndBackground = GetField("background");
        EditDndStrength = ParseInt("strength");
        EditDndDexterity = ParseInt("dexterity");
        EditDndConstitution = ParseInt("constitution");
        EditDndIntelligence = ParseInt("intelligence");
        EditDndWisdom = ParseInt("wisdom");
        EditDndCharisma = ParseInt("charisma");

        IsEditing = true;
        SaveMessage = null;
    }

    private string? GetField(string key) =>
        GameSpecificFields.TryGetValue(key, out var v) ? v : null;

    private int? ParseInt(string key) =>
        GameSpecificFields.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : null;

    private void CancelEdit()
    {
        IsEditing = false;
        SaveMessage = null;
    }

    private string BuildGameSpecificJson()
    {
        if (WorldGameType == GameType.DnD5e)
        {
            var dict = new Dictionary<string, object?>();
            if (!string.IsNullOrEmpty(EditDndClass)) dict["class"] = EditDndClass;
            if (!string.IsNullOrEmpty(EditDndRace)) dict["race"] = EditDndRace;
            if (!string.IsNullOrEmpty(EditDndBackground)) dict["background"] = EditDndBackground;
            if (EditDndStrength.HasValue) dict["strength"] = EditDndStrength;
            if (EditDndDexterity.HasValue) dict["dexterity"] = EditDndDexterity;
            if (EditDndConstitution.HasValue) dict["constitution"] = EditDndConstitution;
            if (EditDndIntelligence.HasValue) dict["intelligence"] = EditDndIntelligence;
            if (EditDndWisdom.HasValue) dict["wisdom"] = EditDndWisdom;
            if (EditDndCharisma.HasValue) dict["charisma"] = EditDndCharisma;
            return dict.Count > 0 ? JsonSerializer.Serialize(dict) : (WorldCharacter?.GameSpecificData ?? "{}");
        }
        return WorldCharacter?.GameSpecificData ?? "{}";
    }

    private async Task SaveProfile()
    {
        if (WorldCharacter == null) return;
        IsSaving = true;
        SaveMessage = null;
        try
        {
            var gameSpecific = BuildGameSpecificJson();
            var request = new UpdateWorldCharacterProfileRequest(
                EditLevel,
                EditCurrentHealth,
                EditMaxHealth,
                gameSpecific
            );
            var result = await WorldClient.UpdateMyWorldCharacterAsync(WorldId, request);
            if (result != null)
            {
                WorldCharacter = result;
                ParseGameSpecificData();
                IsEditing = false;
                SaveSuccess = true;
                SaveMessage = "Profil mis à jour.";
            }
            else
            {
                SaveSuccess = false;
                SaveMessage = "Erreur lors de la sauvegarde.";
            }
        }
        finally
        {
            IsSaving = false;
        }
    }
}
