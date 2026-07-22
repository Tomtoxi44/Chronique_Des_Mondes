// -----------------------------------------------------------------------
// <copyright file="ApiClientRegistrationExtensions.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.Web.Extensions;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers a typed API client (named <see cref="HttpClient"/> + scoped instance) in one line,
/// replacing the ~7-line copy-pasted registration block repeated for every client in Program.cs.
/// Uses <see cref="ActivatorUtilities"/> so the client's remaining constructor dependencies
/// (logger, local storage, …) resolve from DI regardless of their order.
/// </summary>
public static class ApiClientRegistrationExtensions
{
    /// <summary>Registers a concrete API client type.</summary>
    public static IServiceCollection AddApiClient<TClient>(this IServiceCollection services, Action<HttpClient> configure)
        where TClient : class
    {
        var name = typeof(TClient).Name;
        services.AddHttpClient(name, configure);
        services.AddScoped(sp =>
        {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(name);
            return ActivatorUtilities.CreateInstance<TClient>(sp, http);
        });
        return services;
    }

    /// <summary>Registers an API client behind an interface (e.g. <c>IRoleService</c> → <c>RoleService</c>).</summary>
    public static IServiceCollection AddApiClient<TInterface, TImplementation>(this IServiceCollection services, Action<HttpClient> configure)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var name = typeof(TImplementation).Name;
        services.AddHttpClient(name, configure);
        services.AddScoped<TInterface>(sp =>
        {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(name);
            return ActivatorUtilities.CreateInstance<TImplementation>(sp, http);
        });
        return services;
    }
}
