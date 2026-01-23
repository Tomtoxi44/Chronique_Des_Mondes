// <copyright file="RequestGameMasterRoleResponse.cs" company="Chronique Des Mondes">
// Copyright (c) Chronique Des Mondes. All rights reserved.
// </copyright>

namespace Cdm.ApiService.Endpoints.Models;

/// <summary>
/// Response for RequestGameMasterRole endpoint.
/// </summary>
public class RequestGameMasterRoleResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets response message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
