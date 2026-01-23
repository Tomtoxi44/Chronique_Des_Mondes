namespace Cdm.Business.Abstraction.Services;

using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration request DTO</param>
    /// <returns>Service result with registration response</returns>
    Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticate a user
    /// </summary>
    /// <param name="request">Login request DTO</param>
    /// <returns>Service result with login response</returns>
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
}

/// <summary>
/// Standard service result wrapper
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }

    public static ServiceResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static ServiceResult<T> Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };

    public static ServiceResult<T> ValidationFailure(Dictionary<string, string[]> validationErrors) => new()
    {
        IsSuccess = false,
        ValidationErrors = validationErrors
    };
}
