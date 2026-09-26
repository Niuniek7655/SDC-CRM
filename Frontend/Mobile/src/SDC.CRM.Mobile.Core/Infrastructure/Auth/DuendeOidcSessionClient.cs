using Duende.IdentityModel.OidcClient;
using SDC.CRM.Mobile.Infrastructure.Configuration;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// <see cref="IOidcSessionClient"/> implemented with Duende OidcClient. The browser (system browser via
/// MAUI WebAuthenticator in the app) handles both the sign-in and the end-session redirect
/// (<see cref="AppConfig.PostLogoutRedirectUri"/>).
/// </summary>
public sealed class DuendeOidcSessionClient : IOidcSessionClient
{
    private readonly OidcClient _client;

    public DuendeOidcSessionClient(IBrowser browser)
    {
        var options = new OidcClientOptions
        {
            Authority = AppConfig.Authority,
            ClientId = AppConfig.ClientId,
            Scope = AppConfig.Scope,
            RedirectUri = AppConfig.RedirectUri,
            PostLogoutRedirectUri = AppConfig.PostLogoutRedirectUri,
            Browser = browser,
            // SimpleIdServer may reject PAR from a public client; keep it simple.
            DisablePushedAuthorization = true,
        };

        // Allow HTTP against the local identity provider.
        options.Policy.Discovery.RequireHttps = AppConfig.RequireHttps;

        _client = new OidcClient(options);
    }

    public async Task<OidcSessionResult> LoginAsync(CancellationToken cancellationToken)
    {
        var result = await _client.LoginAsync(new LoginRequest(), cancellationToken);

        return result.IsError
            ? OidcSessionResult.Failure(result.Error)
            : OidcSessionResult.Success(new OidcTokens(
                result.AccessToken, result.RefreshToken, result.IdentityToken, result.AccessTokenExpiration));
    }

    public async Task<OidcSessionResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var result = await _client.RefreshTokenAsync(refreshToken, cancellationToken: cancellationToken);

        return result.IsError
            ? OidcSessionResult.Failure(result.Error)
            : OidcSessionResult.Success(new OidcTokens(
                result.AccessToken, result.RefreshToken, result.IdentityToken, result.AccessTokenExpiration));
    }

    public async Task<OidcLogoutResult> EndSessionAsync(string identityToken, CancellationToken cancellationToken)
    {
        var result = await _client.LogoutAsync(new LogoutRequest { IdTokenHint = identityToken }, cancellationToken);

        return result.IsError ? OidcLogoutResult.Failure(result.Error) : OidcLogoutResult.Success();
    }
}

