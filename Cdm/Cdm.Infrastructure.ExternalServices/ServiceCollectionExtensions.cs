// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Common.Services;

using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the external-service integrations (image storage, email) in one place,
/// selecting the concrete provider from configuration. Keeps the provider-switch logic out of the
/// API composition root.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers image storage (Azure Blob in prod via managed identity, local disk otherwise)
    /// and email (Azure Communication Services if configured, logging fallback otherwise).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="requireExternalProviders">
    /// <c>true</c> hors développement : les repliements local/journal deviennent des erreurs de
    /// démarrage explicites. Les réglages vivent dans Azure App Configuration, source déclarée
    /// « optionnelle » pour que l'API démarre même si le store est injoignable ; sans ce garde-fou,
    /// une panne du store ferait basculer silencieusement le stockage d'images sur le disque
    /// éphémère de l'App Service et l'envoi de mails sur un simple logger — exactement la panne
    /// silencieuse qui a coûté des semaines de diagnostic.
    /// </param>
    public static IServiceCollection AddExternalServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool requireExternalProviders = false)
    {
        // Image storage: Azure Blob in prod, local disk in dev/CI (selected by config).
        if (string.Equals(configuration["ImageStorage:Provider"], "AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            var blobServiceUri = configuration["ImageStorage:BlobServiceUri"]
                ?? throw new InvalidOperationException("ImageStorage:BlobServiceUri is required when ImageStorage:Provider=AzureBlob.");
            var containerName = configuration["ImageStorage:ContainerName"] ?? "images";

            // Managed identity in production (no connection string / account key).
            // On écarte les credentials de développeur : sur un App Service Linux ils sont
            // absents, mais DefaultAzureCredential les sonde quand même (chacun avec son
            // propre délai) avant d'abandonner — plusieurs secondes ajoutées à la première
            // écriture. Même réglage que les sources de configuration dans Program.cs.
            var credentialOptions = new DefaultAzureCredentialOptions
            {
                ExcludeVisualStudioCredential = true,
                ExcludeAzurePowerShellCredential = true,
                ExcludeAzureDeveloperCliCredential = true,
                ExcludeInteractiveBrowserCredential = true,
            };

            services.AddSingleton(_ =>
                new BlobServiceClient(new Uri(blobServiceUri), new DefaultAzureCredential(credentialOptions))
                    .GetBlobContainerClient(containerName));
            services.AddScoped<IImageStorage, AzureBlobImageStorage>();
        }
        else if (requireExternalProviders)
        {
            throw new InvalidOperationException(
                "ImageStorage:Provider doit valoir 'AzureBlob' hors développement. " +
                "Vérifiez qu'Azure App Configuration est joignable et que la clé y est définie : " +
                "le repli sur le disque local écrirait dans un stockage éphémère et renverrait " +
                "des URLs inutilisables depuis le front.");
        }
        else
        {
            services.AddScoped<IImageStorage, LocalImageStorage>();
        }

        // Email: Azure Communication Services if configured, otherwise a logging fallback
        // (keeps the "forgot password" / confirmation flows testable locally — the link is logged).
        if (!string.IsNullOrWhiteSpace(configuration["AzureEmail:ConnectionString"]))
        {
            services.AddScoped<IEmailService, AzureEmailService>();
        }
        else if (requireExternalProviders)
        {
            throw new InvalidOperationException(
                "AzureEmail:ConnectionString est requis hors développement. " +
                "Vérifiez qu'Azure App Configuration est joignable et que la référence Key Vault " +
                "s'y résout : sans cette valeur, les emails seraient seulement journalisés et " +
                "aucun message ne partirait réellement.");
        }
        else
        {
            services.AddScoped<IEmailService, LoggingEmailService>();
        }

        return services;
    }
}
