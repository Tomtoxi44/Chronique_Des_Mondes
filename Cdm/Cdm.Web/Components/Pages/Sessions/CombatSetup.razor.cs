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

public partial class CombatSetup
{
    [Parameter] public int SessionId { get; set; }

    [Inject] private CombatApiClient CombatClient { get; set; } = default!;
    [Inject] private SessionApiClient SessionClient { get; set; } = default!;
    [Inject] private NpcApiClient NpcClient { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    private bool IsLoading = true;
    private bool IsCreating = false;
    private string? ErrorMessage;
    private string? StepError;
    private int CurrentStep = 1;
    private string InitiativeMode = "manual";
    private string InitiativeDiceExpression = "1d20";
    private int CurrentUserId;

    private SessionDto? Session;
    private List<GroupSetupModel> Groups = new();
    private List<AvailableParticipant> AllParticipants = new();

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
                DisplayName = participant.CharacterName ?? participant.UserName ?? "Joueur",
                IsPlayer = true,
                CharacterId = participant.WorldCharacterId,
                UserId = participant.UserId
            });
        }

        if (Session.CurrentChapterId.HasValue)
        {
            var npcs = await NpcClient.GetNpcsByChapterAsync(Session.CurrentChapterId.Value);
            foreach (var npc in npcs)
            {
                AllParticipants.Add(new AvailableParticipant
                {
                    DisplayName = $"{npc.FirstName} {npc.Name}".Trim(),
                    IsPlayer = false,
                    NpcId = npc.Id
                });
            }
        }

        IsLoading = false;
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
                    MaxHp = p.MaxHp > 0 ? p.MaxHp : 10
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
        await CombatClient.StartInitiativePhaseAsync(combat.Id, initiativeDto);

        Nav.NavigateTo($"/sessions/{SessionId}/combat/{combat.Id}/gm");
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
