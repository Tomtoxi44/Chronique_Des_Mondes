// -----------------------------------------------------------------------
// <copyright file="ErrorCatalog.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Errors;

/// <summary>
/// Catalogue statique qui associe chaque <see cref="CdmErrorCode"/> à sa définition complète.
/// <para>
/// Utilisé côté API pour construire les réponses JSON structurées,
/// et côté Blazor pour localiser les messages dans <c>AppStrings.resx</c>.
/// </para>
/// </summary>
public static class ErrorCatalog
{
    private static readonly IReadOnlyDictionary<CdmErrorCode, CdmError> Catalog =
        new Dictionary<CdmErrorCode, CdmError>
        {
            // ── Générique ────────────────────────────────────────────────────
            [CdmErrorCode.Unknown]          = new(CdmErrorCode.Unknown,          500, "Unknown"),
            [CdmErrorCode.Unauthorized]     = new(CdmErrorCode.Unauthorized,     401, "Unauthorized"),
            [CdmErrorCode.Forbidden]        = new(CdmErrorCode.Forbidden,        403, "Forbidden"),
            [CdmErrorCode.NotFound]         = new(CdmErrorCode.NotFound,         404, "NotFound"),
            [CdmErrorCode.ValidationFailed] = new(CdmErrorCode.ValidationFailed, 422, "ValidationFailed"),
            [CdmErrorCode.NetworkError]     = new(CdmErrorCode.NetworkError,     503, "NetworkError"),

            // ── Authentification ──────────────────────────────────────────────
            [CdmErrorCode.Auth_InvalidCredentials]  = new(CdmErrorCode.Auth_InvalidCredentials,  400, "Auth_InvalidCredentials"),
            [CdmErrorCode.Auth_EmailAlreadyUsed]    = new(CdmErrorCode.Auth_EmailAlreadyUsed,    409, "Auth_EmailAlreadyUsed"),
            [CdmErrorCode.Auth_WeakPassword]        = new(CdmErrorCode.Auth_WeakPassword,        400, "Auth_WeakPassword"),
            [CdmErrorCode.Auth_TokenExpired]        = new(CdmErrorCode.Auth_TokenExpired,        401, "Auth_TokenExpired"),
            [CdmErrorCode.Auth_TokenInvalid]        = new(CdmErrorCode.Auth_TokenInvalid,        401, "Auth_TokenInvalid"),
            [CdmErrorCode.Auth_RefreshTokenExpired] = new(CdmErrorCode.Auth_RefreshTokenExpired, 401, "Auth_RefreshTokenExpired"),

            // ── Monde ─────────────────────────────────────────────────────────
            [CdmErrorCode.World_NotFound]           = new(CdmErrorCode.World_NotFound,           404, "World_NotFound"),
            [CdmErrorCode.World_Unauthorized]       = new(CdmErrorCode.World_Unauthorized,       403, "World_Unauthorized"),
            [CdmErrorCode.World_NameTooLong]        = new(CdmErrorCode.World_NameTooLong,        400, "World_NameTooLong"),
            [CdmErrorCode.World_InviteTokenInvalid] = new(CdmErrorCode.World_InviteTokenInvalid, 404, "World_InviteTokenInvalid"),
            [CdmErrorCode.World_InviteTokenExpired] = new(CdmErrorCode.World_InviteTokenExpired, 410, "World_InviteTokenExpired"),
            [CdmErrorCode.World_AlreadyMember]      = new(CdmErrorCode.World_AlreadyMember,      409, "World_AlreadyMember"),
            [CdmErrorCode.World_CreateFailed]       = new(CdmErrorCode.World_CreateFailed,       500, "World_CreateFailed"),
            [CdmErrorCode.World_UpdateFailed]       = new(CdmErrorCode.World_UpdateFailed,       500, "World_UpdateFailed"),
            [CdmErrorCode.World_DeleteFailed]       = new(CdmErrorCode.World_DeleteFailed,       500, "World_DeleteFailed"),

            // ── Campagne ──────────────────────────────────────────────────────
            [CdmErrorCode.Campaign_NotFound]          = new(CdmErrorCode.Campaign_NotFound,          404, "Campaign_NotFound"),
            [CdmErrorCode.Campaign_Unauthorized]      = new(CdmErrorCode.Campaign_Unauthorized,      403, "Campaign_Unauthorized"),
            [CdmErrorCode.Campaign_MaxPlayersReached] = new(CdmErrorCode.Campaign_MaxPlayersReached, 409, "Campaign_MaxPlayersReached"),
            [CdmErrorCode.Campaign_CreateFailed]      = new(CdmErrorCode.Campaign_CreateFailed,      500, "Campaign_CreateFailed"),
            [CdmErrorCode.Campaign_UpdateFailed]      = new(CdmErrorCode.Campaign_UpdateFailed,      500, "Campaign_UpdateFailed"),
            [CdmErrorCode.Campaign_DeleteFailed]      = new(CdmErrorCode.Campaign_DeleteFailed,      500, "Campaign_DeleteFailed"),

            // ── Personnage ────────────────────────────────────────────────────
            [CdmErrorCode.Character_NotFound]       = new(CdmErrorCode.Character_NotFound,       404, "Character_NotFound"),
            [CdmErrorCode.Character_Unauthorized]   = new(CdmErrorCode.Character_Unauthorized,   403, "Character_Unauthorized"),
            [CdmErrorCode.Character_AlreadyInWorld] = new(CdmErrorCode.Character_AlreadyInWorld, 409, "Character_AlreadyInWorld"),
            [CdmErrorCode.Character_Locked]         = new(CdmErrorCode.Character_Locked,         409, "Character_Locked"),
            [CdmErrorCode.Character_CreateFailed]   = new(CdmErrorCode.Character_CreateFailed,   500, "Character_CreateFailed"),
            [CdmErrorCode.Character_UpdateFailed]   = new(CdmErrorCode.Character_UpdateFailed,   500, "Character_UpdateFailed"),
            [CdmErrorCode.Character_DeleteFailed]   = new(CdmErrorCode.Character_DeleteFailed,   500, "Character_DeleteFailed"),

            // ── Session / Combat ──────────────────────────────────────────────
            [CdmErrorCode.Session_NotFound]        = new(CdmErrorCode.Session_NotFound,        404, "Session_NotFound"),
            [CdmErrorCode.Session_Unauthorized]    = new(CdmErrorCode.Session_Unauthorized,    403, "Session_Unauthorized"),
            [CdmErrorCode.Session_AlreadyActive]   = new(CdmErrorCode.Session_AlreadyActive,   409, "Session_AlreadyActive"),
            [CdmErrorCode.Session_CreateFailed]    = new(CdmErrorCode.Session_CreateFailed,    500, "Session_CreateFailed"),
            [CdmErrorCode.Combat_InvalidAction]    = new(CdmErrorCode.Combat_InvalidAction,    400, "Combat_InvalidAction"),
            [CdmErrorCode.Combat_NotYourTurn]      = new(CdmErrorCode.Combat_NotYourTurn,      400, "Combat_NotYourTurn"),
            [CdmErrorCode.Combat_SessionNotActive] = new(CdmErrorCode.Combat_SessionNotActive, 400, "Combat_SessionNotActive"),

            // ── Profil ────────────────────────────────────────────────────────
            [CdmErrorCode.Profile_NotFound]            = new(CdmErrorCode.Profile_NotFound,            404, "Profile_NotFound"),
            [CdmErrorCode.Profile_NicknameAlreadyUsed] = new(CdmErrorCode.Profile_NicknameAlreadyUsed, 409, "Profile_NicknameAlreadyUsed"),
            [CdmErrorCode.Profile_UpdateFailed]        = new(CdmErrorCode.Profile_UpdateFailed,        500, "Profile_UpdateFailed"),
            [CdmErrorCode.Profile_AvatarUploadFailed]  = new(CdmErrorCode.Profile_AvatarUploadFailed,  500, "Profile_AvatarUploadFailed"),
        };

    /// <summary>
    /// Récupère la définition complète d'un code d'erreur.
    /// Retourne la définition de <see cref="CdmErrorCode.Unknown"/> si le code est absent du catalogue.
    /// </summary>
    public static CdmError Get(CdmErrorCode code)
        => Catalog.TryGetValue(code, out var error) ? error : Catalog[CdmErrorCode.Unknown];

    /// <summary>
    /// Tente de parser un code d'erreur depuis sa représentation textuelle (nom de l'enum).
    /// Retourne <see cref="CdmErrorCode.Unknown"/> si la chaîne est invalide.
    /// </summary>
    public static CdmErrorCode Parse(string? code)
        => Enum.TryParse<CdmErrorCode>(code, ignoreCase: true, out var result) ? result : CdmErrorCode.Unknown;
}
