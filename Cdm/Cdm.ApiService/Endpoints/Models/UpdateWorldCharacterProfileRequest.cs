namespace Cdm.ApiService.Endpoints.Models;

/// <summary>
/// Request model for updating a world character profile.
/// </summary>
public record UpdateWorldCharacterProfileRequest(
    int? Level,
    int? CurrentHealth,
    int? MaxHealth,
    string? GameSpecificData);
