namespace Cdm.Business.Abstraction.DTOs.ViewModels;

/// <summary>
/// Represents a standard error response.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation errors dictionary.
    /// Maps field names to their respective error messages.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
