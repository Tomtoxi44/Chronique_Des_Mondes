// -----------------------------------------------------------------------
// <copyright file="StatusDisplayExtensions.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Extensions;

using Cdm.Common.Enums;

/// <summary>
/// Single source of truth for rendering status enums (label + CSS class), replacing the
/// per-page duplicated <c>GetStatusLabel</c>/<c>GetStatusClass</c> helpers.
/// </summary>
public static class StatusDisplayExtensions
{
    public static string ToCssClass(this SessionStatus status) => status switch
    {
        SessionStatus.Active => "status-active",
        SessionStatus.Paused => "status-pending",
        SessionStatus.Ended => "status-completed",
        SessionStatus.Cancelled => "status-archived",
        _ => "status-draft",
    };

    public static string ToLabel(this SessionStatus status) => status switch
    {
        SessionStatus.Active => "En cours",
        SessionStatus.Paused => "En pause",
        SessionStatus.Ended => "Terminée",
        SessionStatus.Cancelled => "Annulée",
        _ => "Planifiée",
    };

    public static string ToCssClass(this SessionParticipantStatus status) => status switch
    {
        SessionParticipantStatus.Joined => "status-active",
        SessionParticipantStatus.Invited => "status-pending",
        _ => "status-draft",
    };

    public static string ToLabel(this SessionParticipantStatus status) => status switch
    {
        SessionParticipantStatus.Joined => "Connecté",
        SessionParticipantStatus.Invited => "Invité",
        _ => "Déconnecté",
    };

    public static string ToCssClass(this CampaignStatus status) => status switch
    {
        CampaignStatus.Active => "status-active",
        CampaignStatus.Planning => "status-planning",
        CampaignStatus.Completed => "status-completed",
        CampaignStatus.Cancelled => "status-cancelled",
        CampaignStatus.OnHold => "status-paused",
        _ => string.Empty,
    };

    /// <summary>Resource key for a campaign status label (used as <c>L[status.ToLabelKey()]</c>).</summary>
    public static string ToLabelKey(this CampaignStatus status) => status switch
    {
        CampaignStatus.Active => "Campaigns_Status_Active",
        CampaignStatus.Planning => "Campaigns_Status_Planning",
        CampaignStatus.Completed => "Campaigns_Status_Completed",
        CampaignStatus.Cancelled => "Campaigns_Status_Cancelled",
        CampaignStatus.OnHold => "Campaigns_Status_OnHold",
        _ => "Campaigns_Status_Planning",
    };
}
