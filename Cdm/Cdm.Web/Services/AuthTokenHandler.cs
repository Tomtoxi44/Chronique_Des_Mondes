using Cdm.Web.Services.Storage;
using System.Net.Http.Headers;

namespace Cdm.Web.Services;

/// <summary>
/// DelegatingHandler that automatically injects the JWT Bearer token
/// from localStorage into every outgoing HTTP request.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthTokenHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsync("auth_token");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
