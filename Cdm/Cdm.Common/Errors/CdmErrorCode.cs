// -----------------------------------------------------------------------
// <copyright file="CdmErrorCode.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Errors;

/// <summary>
/// Catalogue centralisé des codes d'erreur applicatifs de Chronique des Mondes.
/// Chaque valeur mappe à une clé de ressource dans AppStrings.resx (préfixe <c>Error_</c>).
/// </summary>
public enum CdmErrorCode
{
    // ── Générique (0–9) ──────────────────────────────────────────────────────
    Unknown = 0,
    Unauthorized = 1,
    Forbidden = 2,
    NotFound = 3,
    ValidationFailed = 4,
    NetworkError = 5,

    // ── Authentification (100–199) ────────────────────────────────────────────
    Auth_InvalidCredentials = 100,
    Auth_EmailAlreadyUsed = 101,
    Auth_WeakPassword = 102,
    Auth_TokenExpired = 103,
    Auth_TokenInvalid = 104,
    Auth_RefreshTokenExpired = 105,

    // ── Monde (200–299) ───────────────────────────────────────────────────────
    World_NotFound = 200,
    World_Unauthorized = 201,
    World_NameTooLong = 202,
    World_InviteTokenInvalid = 203,
    World_InviteTokenExpired = 204,
    World_AlreadyMember = 205,
    World_CreateFailed = 206,
    World_UpdateFailed = 207,
    World_DeleteFailed = 208,

    // ── Campagne (300–399) ────────────────────────────────────────────────────
    Campaign_NotFound = 300,
    Campaign_Unauthorized = 301,
    Campaign_MaxPlayersReached = 302,
    Campaign_CreateFailed = 303,
    Campaign_UpdateFailed = 304,
    Campaign_DeleteFailed = 305,

    // ── Personnage (400–499) ──────────────────────────────────────────────────
    Character_NotFound = 400,
    Character_Unauthorized = 401,
    Character_AlreadyInWorld = 402,
    Character_Locked = 403,
    Character_CreateFailed = 404,
    Character_UpdateFailed = 405,
    Character_DeleteFailed = 406,

    // ── Session / Combat (500–599) ────────────────────────────────────────────
    Session_NotFound = 500,
    Session_Unauthorized = 501,
    Session_AlreadyActive = 502,
    Session_CreateFailed = 503,
    Combat_InvalidAction = 510,
    Combat_NotYourTurn = 511,
    Combat_SessionNotActive = 512,

    // ── Profil (600–699) ──────────────────────────────────────────────────────
    Profile_NotFound = 600,
    Profile_NicknameAlreadyUsed = 601,
    Profile_UpdateFailed = 602,
    Profile_AvatarUploadFailed = 603,
}
