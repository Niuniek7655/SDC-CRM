using System.Net.Http.Headers;
using SDC.CRM.Mobile.Infrastructure.Auth;

namespace SDC.CRM.Mobile.Infrastructure.Api;

/// <summary>
/// Attaches the bearer access token (refreshing when needed) to outgoing API
/// requests. Keeps token handling out of view models and API client code.
/// </summary>
public sealed class AuthHeaderHandler(IAuthService authService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await authService.GetAccessTokenAsync(cancellationToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
