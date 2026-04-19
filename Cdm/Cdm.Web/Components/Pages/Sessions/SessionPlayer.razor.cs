using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Cdm.Web.Services.State;
using Cdm.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Text.Json;

namespace Cdm.Web.Components.Pages.Sessions;

public partial class SessionPlayer
{
    [Parameter] public int SessionId { get; set; }

    [Inject] private SessionApiClient SessionClient { get; set; } = default!;
    [Inject] private WorldApiClient WorldClient { get; set; } = default!;
    [Inject] private NavigationContextService NavContext { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private SessionDto? Session;
    private WorldCharacterDto? MyCharacter;
    private SessionParticipantDto? MyParticipant;
    private bool IsLoading = true;
    private bool IsJoining = false;
    private string? ErrorMessage;
    private int CurrentUserId;

    private string ActiveTab = "character"; // "character" | "inventory" | "gametype"
    private Dictionary<string, string> GameSpecificFields { get; set; } = new();
    private List<InventoryItem> Inventory { get; set; } = new();
    private string NewItemName = "";

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
            CurrentUserId = uid;

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        Session = await SessionClient.GetSessionAsync(SessionId);

        if (Session == null)
        {
            ErrorMessage = "Session introuvable ou accès refusé.";
            IsLoading = false;
            return;
        }

        if (Session.StartedById == CurrentUserId)
        {
            Nav.NavigateTo($"/sessions/{SessionId}/gm");
            return;
        }

        MyParticipant = Session.Participants.FirstOrDefault(p => p.UserId == CurrentUserId);
        MyCharacter = await WorldClient.GetMyWorldCharacterAsync(Session.WorldId);
        ParseGameSpecificData();

        NavContext.ClearContext();
        IsLoading = false;
    }

    private void ParseGameSpecificData()
    {
        GameSpecificFields = new();
        Inventory = new();
        if (string.IsNullOrWhiteSpace(MyCharacter?.GameSpecificData)) return;
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(MyCharacter.GameSpecificData);
            if (dict == null) return;
            foreach (var kv in dict)
            {
                if (kv.Key == "inventory" && kv.Value.ValueKind == JsonValueKind.Array)
                {
                    Inventory = kv.Value.Deserialize<List<InventoryItem>>() ?? new();
                }
                else
                {
                    GameSpecificFields[kv.Key] = kv.Value.ToString();
                }
            }
        }
        catch { }
    }

    private async Task AddInventoryItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemName) || MyCharacter == null) return;
        Inventory.Add(new InventoryItem { Name = NewItemName.Trim(), Qty = 1 });
        NewItemName = "";
        await SaveInventory();
    }

    private async Task RemoveInventoryItem(InventoryItem item)
    {
        Inventory.Remove(item);
        await SaveInventory();
    }

    private async Task SaveInventory()
    {
        if (MyCharacter == null) return;
        var dict = new Dictionary<string, object>(GameSpecificFields.ToDictionary(kv => kv.Key, kv => (object)kv.Value));
        dict["inventory"] = Inventory;
        var json = JsonSerializer.Serialize(dict);
        var request = new UpdateWorldCharacterProfileRequest(MyCharacter.Level, MyCharacter.CurrentHealth, MyCharacter.MaxHealth, json);
        var result = await WorldClient.UpdateMyWorldCharacterAsync(Session!.WorldId, request);
        if (result != null) MyCharacter = result;
    }

    private async Task JoinSession()
    {
        if (Session == null || MyCharacter == null) return;
        IsJoining = true;
        var result = await SessionClient.JoinSessionAsync(Session.Id, MyCharacter.Id);
        if (result != null)
        {
            Session = result;
            MyParticipant = Session.Participants.FirstOrDefault(p => p.UserId == CurrentUserId);
        }
        IsJoining = false;
    }

    private static string GetStatusLabel(SessionStatus status) => status switch
    {
        SessionStatus.Active => "En cours",
        SessionStatus.Paused => "En pause",
        SessionStatus.Ended => "Terminée",
        _ => "Planifiée"
    };

    private static string GetStatusClass(SessionStatus status) => status switch
    {
        SessionStatus.Active => "status-active",
        SessionStatus.Paused => "status-pending",
        SessionStatus.Ended => "status-completed",
        _ => "status-draft"
    };

    private record InventoryItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("qty")]
        public int Qty { get; set; } = 1;
    }
}
