using Cdm.Business.Abstraction.DTOs.DnD5e;
using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cdm.Web.Services.ApiClients;

public class DndApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DndApiClient> _logger;
    private readonly ILocalStorageService _localStorage;

    public DndApiClient(HttpClient httpClient, ILogger<DndApiClient> logger, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _logger = logger;
        _localStorage = localStorage;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // ── Reference Data ───────────────────────────────────────────────────

    public async Task<List<DndRaceDto>> GetRacesAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/dnd/races");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndRaceDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching D&D races"); }
        return [];
    }

    public async Task<List<DndClassDto>> GetClassesAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/dnd/classes");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndClassDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching D&D classes"); }
        return [];
    }

    public async Task<List<DndItemDto>> GetItemsAsync(string? category = null)
    {
        try
        {
            await AddAuthHeaderAsync();
            var url = string.IsNullOrEmpty(category) ? "api/dnd/items" : $"api/dnd/items?category={Uri.EscapeDataString(category)}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndItemDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching D&D items"); }
        return [];
    }

    public async Task<List<DndSpellDto>> GetSpellsAsync(int? level = null, string? characterClass = null)
    {
        try
        {
            await AddAuthHeaderAsync();
            var query = new List<string>();
            if (level.HasValue) query.Add($"level={level.Value}");
            if (!string.IsNullOrEmpty(characterClass)) query.Add($"characterClass={Uri.EscapeDataString(characterClass)}");
            var url = query.Count > 0 ? $"api/dnd/spells?{string.Join("&", query)}" : "api/dnd/spells";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndSpellDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching D&D spells"); }
        return [];
    }

    public async Task<List<DndMonsterTemplateDto>> GetMonstersAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/dnd/monsters");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndMonsterTemplateDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching D&D monster templates"); }
        return [];
    }

    public async Task<List<DndBackgroundDto>> GetBackgroundsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/dnd/backgrounds");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndBackgroundDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching D&D backgrounds"); }
        return [];
    }

    public async Task<List<DndSkillDto>> GetSkillsAsync()
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync("api/dnd/skills");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndSkillDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching D&D skills"); }
        return [];
    }

    // ── Character Stats ──────────────────────────────────────────────────

    public async Task<DndCharacterStatsDto?> GetCharacterStatsAsync(int wcId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/dnd/world-characters/{wcId}/stats");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DndCharacterStatsDto>();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching D&D character stats for wcId={WcId}", wcId); }
        return null;
    }

    public async Task<bool> SaveCharacterStatsAsync(int wcId, DndCharacterStatsDto stats)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/dnd/world-characters/{wcId}/stats", stats);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error saving D&D character stats for wcId={WcId}", wcId); }
        return false;
    }

    // ── Inventory ────────────────────────────────────────────────────────

    public async Task<List<DndInventoryItemDto>> GetInventoryAsync(int wcId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/dnd/world-characters/{wcId}/inventory");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndInventoryItemDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching inventory for wcId={WcId}", wcId); }
        return [];
    }

    public async Task<DndInventoryItemDto?> AddInventoryItemAsync(int wcId, DndInventoryItemDto item)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/dnd/world-characters/{wcId}/inventory", item);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DndInventoryItemDto>();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error adding inventory item for wcId={WcId}", wcId); }
        return null;
    }

    public async Task<bool> RemoveInventoryItemAsync(int wcId, int itemId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/dnd/world-characters/{wcId}/inventory/{itemId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error removing inventory item {ItemId} for wcId={WcId}", itemId, wcId); }
        return false;
    }

    // ── Spells ───────────────────────────────────────────────────────────

    public async Task<List<DndCharacterSpellDto>> GetCharacterSpellsAsync(int wcId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/dnd/world-characters/{wcId}/spells");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndCharacterSpellDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching spells for wcId={WcId}", wcId); }
        return [];
    }

    public async Task<DndCharacterSpellDto?> AddSpellAsync(int wcId, DndCharacterSpellDto spell)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/dnd/world-characters/{wcId}/spells", spell);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DndCharacterSpellDto>();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error adding spell for wcId={WcId}", wcId); }
        return null;
    }

    public async Task<bool> RemoveSpellAsync(int wcId, int spellId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/dnd/world-characters/{wcId}/spells/{spellId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error removing spell {SpellId} for wcId={WcId}", spellId, wcId); }
        return false;
    }

    // ── D&D NPCs ─────────────────────────────────────────────────────────

    public async Task<List<DndNpcDto>> GetDndNpcsAsync(int chapterId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"api/dnd/npcs/chapter/{chapterId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<DndNpcDto>>() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "Error fetching D&D NPCs for chapterId={ChapterId}", chapterId); }
        return [];
    }

    public async Task<DndNpcDto?> CreateDndNpcAsync(int chapterId, CreateDndNpcDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/dnd/npcs/chapter/{chapterId}", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DndNpcDto>();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error creating D&D NPC in chapterId={ChapterId}", chapterId); }
        return null;
    }

    public async Task<DndNpcDto?> UpdateDndNpcAsync(int id, CreateDndNpcDto dto)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/dnd/npcs/{id}", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DndNpcDto>();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error updating D&D NPC id={Id}", id); }
        return null;
    }

    public async Task<bool> DeleteDndNpcAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/dnd/npcs/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error deleting D&D NPC id={Id}", id); }
        return false;
    }
}
