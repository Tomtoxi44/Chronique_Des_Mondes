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
    public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Image storage: Azure Blob in prod, local disk in dev/CI (selected by config).
        if (string.Equals(configuration["ImageStorage:Provider"], "AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            var blobServiceUri = configuration["ImageStorage:BlobServiceUri"]
                ?? throw new InvalidOperationException("ImageStorage:BlobServiceUri is required when ImageStorage:Provider=AzureBlob.");
            var containerName = configuration["ImageStorage:ContainerName"] ?? "images";

            // Managed identity in production (no connection string / account key).
            services.AddSingleton(_ =>
                new BlobServiceClient(new Uri(blobServiceUri), new DefaultAzureCredential())
                    .GetBlobContainerClient(containerName));
            services.AddScoped<IImageStorage, AzureBlobImageStorage>();
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
        else
        {
            services.AddScoped<IEmailService, LoggingEmailService>();
        }

        return services;
    }
}
