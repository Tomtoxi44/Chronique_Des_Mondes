// -----------------------------------------------------------------------
// <copyright file="CombatSetup.razor.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Cdm.Business.Abstraction.DTOs;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Cdm.Web.Components.Pages.Sessions;

public partial class CombatSetup : IAsyncDisposable
{
    [Parameter] public int SessionId { get; set; }

    [Inject] private CombatApiClient CombatClient { get; set; } = default!;
    [Inject] private SessionApiClient SessionClient { get; set; } = default!;
    [Inject] private NpcApiClient NpcClient { get; set; } = default!;
    [Inject] private ChapterApiClient ChapterClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    private bool IsLoading = true;
    private bool IsLoadingNpcs = false;
    private bool IsCreating = false;
    private bool IsStartingCombat = false;
    private string? ErrorMessage;
    private string? StepError;
    private int CurrentStep = 1;
    private string InitiativeMode = "manual";
    private string InitiativeDiceExpression = "1d20";
    private int CurrentUserId;

    private SessionDto? Session;
    private List<ChapterDto> Chapters = new();
    private int? SelectedChapterId;
    private List<GroupSetupModel> Groups = new();
    private List<AvailableParticipant> AllParticipants = new();

    private CombatDto? CreatedCombat;
    private List<CombatParticipantDto> _orderedParticipants = new();
    private System.Threading.Timer? _pollTimer;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        if (int.TryParse(authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
            CurrentUserId = uid;

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        Session = await SessionClient.GetSessionAsync(SessionId);
        if (Session == null)
        {
            ErrorMessage = "Session introuvable.";
            IsLoading = false;
            return;
        }

        Groups = new List<GroupSetupModel>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Joueurs", Color = "#6366f1" },
            new() { Id = Guid.NewGuid().ToString(), Name = "Monstres", Color = "#ef4444" }
        };

        foreach (var participant in Session.Participants)
        {
            AllParticipants.Add(new AvailableParticipant
            {
                DisplayName = !string.IsNullOrWhiteSpace(participant.CharacterName)
                    ? participant.CharacterName
                    : (!string.IsNullOrWhiteSpace(participant.UserName) ? participant.UserName : "Joueur"),
                IsPlayer = true,
                CharacterId = participant.WorldCharacterId,
                UserId = participant.UserId
            });
        }

        Chapters = await ChapterClient.GetChaptersByCampaignAsync(Session.CampaignId);
        SelectedChapterId = Session.CurrentChapterId ?? Chapters.FirstOrDefault()?.Id;
        if (SelectedChapterId.HasValue)
            await LoadNpcsAsync(SelectedChapterId.Value);

        IsLoading = false;
    }

    private async Task LoadNpcsAsync(int chapterId)
    {
        IsLoadingNpcs = true;
        AllParticipants.RemoveAll(p => !p.IsPlayer);
        var npcs = await NpcClient.GetNpcsByChapterAsync(chapterId);
        foreach (var npc in npcs)
        {
            AllParticipants.Add(new AvailableParticipant
            {
                DisplayName = $"{npc.FirstName} {npc.Name}".Trim(),
                IsPlayer = false,
                NpcId = npc.Id
            });
        }
        IsLoadingNpcs = false;
    }

    private async Task OnChapterChangedAsync(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var chapterId))
        {
            SelectedChapterId = chapterId;
            await LoadNpcsAsync(chapterId);
            StateHasChanged();
        }
    }

    private void AddGroup()
    {
        Groups.Add(new GroupSetupModel { Id = Guid.NewGuid().ToString(), Name = "", Color = "#8b5cf6" });
    }

    private void RemoveGroup(GroupSetupModel group)
    {
        if (Groups.Count <= 2) return;
        Groups.Remove(group);
    }

    private List<AvailableParticipant> GetAvailableParticipants()
    {
        var assigned = Groups.SelectMany(g => g.Participants).ToList();
        return AllParticipants.Where(p =>
            !assigned.Any(a =>
                (a.CharacterId == p.CharacterId && p.CharacterId.HasValue) ||
                (a.NpcId == p.NpcId && p.NpcId.HasValue))).ToList();
    }

    private void AssignToGroup(AvailableParticipant participant, string? groupId)
    {
        if (string.IsNullOrEmpty(groupId)) return;
        var group = Groups.FirstOrDefault(g => g.Id == groupId);
        if (group == null) return;

        group.Participants.Add(new ParticipantSetupModel
        {
            DisplayName = participant.DisplayName,
            IsPlayer = participant.IsPlayer,
            CharacterId = participant.CharacterId,
            NpcId = participant.NpcId,
            UserId = participant.UserId,
            MaxHp = 10
        });
        StepError = null;
        StateHasChanged();
    }

    private void RemoveParticipant(GroupSetupModel group, ParticipantSetupModel participant)
    {
        group.Participants.Remove(participant);
    }

    private void GoToStep2()
    {
        StepError = null;
        if (Groups.Any(g => string.IsNullOrWhiteSpace(g.Name)))
        {
            StepError = "Tous les groupes doivent avoir un nom.";
            return;
        }
        CurrentStep = 2;
    }

    private void GoToStep3()
    {
        StepError = null;
        if (Groups.All(g => g.Participants.Count == 0))
        {
            StepError = "Au moins un participant doit être assigné à un groupe.";
            return;
        }
        CurrentStep = 3;
    }

    private async Task LaunchCombat()
    {
        StepError = null;
        IsCreating = true;

        var dto = new CreateCombatDto
        {
            SessionId = SessionId,
            ChapterId = Session?.CurrentChapterId,
            Groups = Groups.Select(g => new CreateCombatGroupDto
            {
                Name = g.Name,
                Color = g.Color,
                Participants = g.Participants.Select(p => new CreateParticipantDto
                {
                    Name = p.DisplayName,
                    IsPlayerCharacter = p.IsPlayer,
                    CharacterId = p.CharacterId,
                    NpcId = p.NpcId,
                    UserId = p.UserId,
                    MaxHp = 1
                }).ToList()
            }).ToList()
        };

        var combat = await CombatClient.CreateCombatAsync(dto);
        if (combat == null)
        {
            StepError = "Impossible de créer le combat. Vérifiez vos droits.";
            IsCreating = false;
            return;
        }

        var initiativeDto = new StartInitiativeDto
        {
            Mode = InitiativeMode,
            DiceExpression = InitiativeMode == "roll" ? InitiativeDiceExpression : null
        };
        var withInitiative = await CombatClient.StartInitiativePhaseAsync(combat.Id, initiativeDto);
        CreatedCombat = withInitiative ?? combat;

        // Build the ordered list: sorted by initiative desc initially
        _orderedParticipants = CreatedCombat.Participants
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Initiative.HasValue)
            .ThenByDescending(p => p.Initiative ?? 0)
            .ThenBy(p => p.Name)
            .ToList();

        IsCreating = false;
        CurrentStep = 4;

        // Start polling for player initiative updates
        _pollTimer = new System.Threading.Timer(
            _ => _ = PollInitiativeAsync(),
            null,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(2));
    }

    private async Task PollInitiativeAsync()
    {
        if (CreatedCombat == null) return;
        var updated = await CombatClient.GetCombatAsync(CreatedCombat.Id);
        if (updated == null) return;

        // Merge updated initiative values into our ordered list
        foreach (var p in _orderedParticipants)
        {
            var fresh = updated.Participants.FirstOrDefault(x => x.Id == p.Id);
            if (fresh != null) p.Initiative = fresh.Initiative;
        }

        CreatedCombat = updated;
        await InvokeAsync(StateHasChanged);
    }

    private void MoveParticipantUp(CombatParticipantDto p)
    {
        var idx = _orderedParticipants.IndexOf(p);
        if (idx <= 0) return;
        _orderedParticipants.RemoveAt(idx);
        _orderedParticipants.Insert(idx - 1, p);
    }

    private void MoveParticipantDown(CombatParticipantDto p)
    {
        var idx = _orderedParticipants.IndexOf(p);
        if (idx < 0 || idx >= _orderedParticipants.Count - 1) return;
        _orderedParticipants.RemoveAt(idx);
        _orderedParticipants.Insert(idx + 1, p);
    }

    private async Task SetPlayerInitiativeFromGm(CombatParticipantDto p, ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out int val)) return;
        if (CreatedCombat == null) return;
        var updated = await CombatClient.SetInitiativeAsync(CreatedCombat.Id, p.Id, new SetInitiativeDto { Value = val });
        if (updated != null)
        {
            CreatedCombat = updated;
            var orderedP = _orderedParticipants.FirstOrDefault(x => x.Id == p.Id);
            if (orderedP != null) orderedP.Initiative = val;
        }
    }

    private async Task ValidateAndStartCombat()
    {
        if (CreatedCombat == null) return;
        IsStartingCombat = true;
        _pollTimer?.Dispose();
        _pollTimer = null;

        var orderedIds = _orderedParticipants.Select(p => p.Id).ToList();
        var started = await CombatClient.StartCombatAsync(CreatedCombat.Id, new StartCombatDto { ParticipantIds = orderedIds });
        if (started == null)
        {
            StepError = "Impossible de démarrer le combat.";
            IsStartingCombat = false;
            return;
        }

        Nav.NavigateTo($"/sessions/{SessionId}/combat/{CreatedCombat.Id}/gm");
    }

    public async ValueTask DisposeAsync()
    {
        if (_pollTimer != null)
        {
            _pollTimer.Dispose();
            _pollTimer = null;
        }
        await ValueTask.CompletedTask;
    }

    private class GroupSetupModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#6366f1";
        public List<ParticipantSetupModel> Participants { get; set; } = new();
    }

    private class ParticipantSetupModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public bool IsPlayer { get; set; }
        public int? CharacterId { get; set; }
        public int? NpcId { get; set; }
        public int? UserId { get; set; }
        public int MaxHp { get; set; } = 10;
    }

    private class AvailableParticipant
    {
        public string DisplayName { get; set; } = string.Empty;
        public bool IsPlayer { get; set; }
        public int? CharacterId { get; set; }
        public int? NpcId { get; set; }
        public int? UserId { get; set; }
    }
}
