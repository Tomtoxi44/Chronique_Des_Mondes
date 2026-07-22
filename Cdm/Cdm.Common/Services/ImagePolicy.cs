// -----------------------------------------------------------------------
// <copyright file="ImagePolicy.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Services;

/// <summary>
/// Limites appliquées à une image selon son usage. Source unique partagée par l'API
/// (qui les fait respecter) et par le front (qui les annonce à l'utilisateur avant
/// l'envoi) — sans ça les deux côtés dérivent et l'utilisateur découvre la règle en
/// se prenant une erreur.
/// </summary>
/// <param name="MaxFileSizeBytes">Poids maximal accepté, en octets.</param>
/// <param name="MaxDimension">Côté maximal en pixels (largeur et hauteur), ou 0 si libre.</param>
public sealed record ImagePolicy(int MaxFileSizeBytes, int MaxDimension)
{
    /// <summary>
    /// Portraits (photo de profil, avatar de personnage, portrait de PNJ) : affichés en
    /// petites vignettes un peu partout, donc bridés — inutile de faire transiter puis
    /// redimensionner une photo de 12 Mpx pour une pastille de 72 px.
    /// </summary>
    public static readonly ImagePolicy Portrait = new(2 * 1024 * 1024, 2048);

    /// <summary>
    /// Illustrations libres (cartes, plans, lieux, visuels de campagne ou de chapitre) :
    /// on veut pouvoir zoomer dedans, seule la taille du fichier est bornée.
    /// </summary>
    public static readonly ImagePolicy Default = new(5 * 1024 * 1024, 0);

    /// <summary>Catégories de stockage soumises aux limites de portrait.</summary>
    private static readonly HashSet<string> PortraitCategories =
        new(StringComparer.OrdinalIgnoreCase) { "avatars", "portraits" };

    /// <summary>Renvoie la politique applicable à une catégorie de stockage.</summary>
    public static ImagePolicy For(string? category) =>
        category is not null && PortraitCategories.Contains(category) ? Portrait : Default;

    /// <summary>Poids maximal exprimé en mégaoctets, pour les messages.</summary>
    public int MaxFileSizeMb => this.MaxFileSizeBytes / (1024 * 1024);

    /// <summary>
    /// Description lisible des limites, à afficher sous un champ d'envoi
    /// (« JPEG, PNG ou WebP · 2 Mo max · 2048 × 2048 px max »).
    /// </summary>
    public string Describe() => this.MaxDimension > 0
        ? $"JPEG, PNG ou WebP · {this.MaxFileSizeMb} Mo max · {this.MaxDimension} × {this.MaxDimension} px max"
        : $"JPEG, PNG ou WebP · {this.MaxFileSizeMb} Mo max";

    /// <summary>Message d'erreur quand le fichier est trop lourd.</summary>
    public string TooHeavyMessage() => $"Image trop lourde (maximum {this.MaxFileSizeMb} Mo).";

    /// <summary>Message d'erreur quand l'image dépasse les dimensions autorisées.</summary>
    public string TooLargeMessage(int width, int height) =>
        $"Image trop grande ({width} × {height} px) : maximum {this.MaxDimension} × {this.MaxDimension} px.";
}
