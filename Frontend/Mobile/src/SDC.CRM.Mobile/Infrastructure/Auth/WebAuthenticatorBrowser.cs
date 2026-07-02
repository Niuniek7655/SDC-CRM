using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient.Browser;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// Bridges Duende OidcClient to the MAUI system browser via
/// <see cref="WebAuthenticator"/>. The system browser is required for a secure,
/// standards-compliant native OIDC login (RFC 8252).
/// </summary>
public sealed class WebAuthenticatorBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(
        BrowserOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = new Uri(options.StartUrl),
                    CallbackUrl = new Uri(options.EndUrl),
                    PrefersEphemeralWebBrowserSession = true,
                });

            // Reconstruct the full callback URL OidcClient expects (code + state).
            var responseUrl = new RequestUrl(options.EndUrl)
                .Create(new Parameters(authResult.Properties));

            return new BrowserResult
            {
                Response = responseUrl,
                ResultType = BrowserResultType.Success,
            };
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult { ResultType = BrowserResultType.UserCancel };
        }
        catch (Exception ex)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UnknownError,
                Error = ex.Message,
            };
        }
    }
}
