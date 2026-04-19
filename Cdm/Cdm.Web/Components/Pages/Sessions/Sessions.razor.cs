using Cdm.Business.Abstraction.DTOs;
using Cdm.Common.Enums;
using Cdm.Web.Resources;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Cdm.Web.Components.Pages.Sessions;

public partial class Sessions
{
    [Inject] private SessionApiClient SessionClient { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private List<SessionDto> MySessions { get; set; } = new();
    private bool IsLoading = true;
    private int CurrentUserId;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
            CurrentUserId = uid;

        IsLoading = true;
        MySessions = await SessionClient.GetMySessionsAsync();
        IsLoading = false;
    }

    private bool IsGm(SessionDto session) => session.StartedById == CurrentUserId;

    private static string GetSessionStatusLabel(SessionStatus status) => status switch
    {
        SessionStatus.Active => "En cours",
        SessionStatus.Paused => "En pause",
        SessionStatus.Ended => "Terminée",
        SessionStatus.Cancelled => "Annulée",
        _ => "Planifiée"
    };

    private static string GetSessionStatusClass(SessionStatus status) => status switch
    {
        SessionStatus.Active => "status-active",
        SessionStatus.Paused => "status-pending",
        SessionStatus.Ended => "status-completed",
        SessionStatus.Cancelled => "status-archived",
        _ => "status-draft"
    };
}
