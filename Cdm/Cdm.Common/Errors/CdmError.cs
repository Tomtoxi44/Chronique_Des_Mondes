// -----------------------------------------------------------------------
// <copyright file="CdmError.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Errors;

/// <summary>
/// Représente une erreur applicative typée avec son code HTTP et sa clé de localisation.
/// </summary>
/// <param name="Code">Code d'erreur applicatif.</param>
/// <param name="HttpStatus">Code HTTP retourné par l'API pour cette erreur.</param>
/// <param name="MessageKey">
/// Suffixe de la clé de ressource dans AppStrings.resx.
/// La clé complète est obtenue via <see cref="ResourceKey"/> : <c>Error_{MessageKey}</c>.
/// </param>
public record CdmError(CdmErrorCode Code, int HttpStatus, string MessageKey)
{
    /// <summary>
    /// Clé de ressource préfixée pour récupérer le message localisé.
    /// Exemple : <c>Error_Auth_InvalidCredentials</c>
    /// </summary>
    public string ResourceKey => $"Error_{MessageKey}";
}
