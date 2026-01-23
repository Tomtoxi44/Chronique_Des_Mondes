namespace Cdm.Web.Services.ApiClients.Base;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }
    
    public ApiException(
        int statusCode, 
        string message, 
        string? errorCode = null,
        Dictionary<string, string[]>? validationErrors = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }
}
