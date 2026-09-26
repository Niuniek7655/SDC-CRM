using System.Text.Json;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using SDC.CRM.Mobile.Infrastructure.Configuration;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// OIDC relying on party for the mobile client using Duende OidcClient with the
/// system browser. Tokens are persisted through <see cref="ITokenStorage"/>.
/// </summary>
public sealed class OidcAuthService : IAuthService
{
    private static readonly TimeSpan ExpiryLeeway = TimeSpan.FromSeconds(60);

    private readonly ITokenStorage _tokenStorage;
    private readonly ILogger<OidcAuthService> _logger;
    private readonly OidcClient _client;

    public OidcAuthService(
        ITokenStorage tokenStorage,
        IBrowser browser,
        ILogger<OidcAuthService> logger)
    {
        _tokenStorage = tokenStorage;
        _logger = logger;

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

    public async Task<AuthResult> LoginAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.LoginAsync(new LoginRequest(), cancellationToken);

            if (result.IsError)
            {
                _logger.LogWarning("OIDC login failed: {Error}", result.Error);
                return AuthResult.Failure(result.Error);
            }

            await _tokenStorage.SaveAsync(new TokenSet(
                result.AccessToken,
                result.RefreshToken ?? string.Empty,
                result.AccessTokenExpiration));

            return AuthResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OIDC login threw");
            return AuthResult.Failure(ex.Message);
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _tokenStorage.ClearAsync();
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var tokens = await _tokenStorage.GetAsync();
        if (tokens is null)
        {
            return null;
        }

        if (tokens.ExpiresAt - ExpiryLeeway > DateTimeOffset.UtcNow)
        {
            return tokens.AccessToken;
        }

        if (string.IsNullOrEmpty(tokens.RefreshToken))
        {
            await _tokenStorage.ClearAsync();
            return null;
        }

        try
        {
            var refreshed = await _client.RefreshTokenAsync(tokens.RefreshToken, cancellationToken: cancellationToken);
            if (refreshed.IsError)
            {
                _logger.LogWarning("Token refresh failed: {Error}", refreshed.Error);
                await _tokenStorage.ClearAsync();
                return null;
            }

            await _tokenStorage.SaveAsync(new TokenSet(
                refreshed.AccessToken,
                string.IsNullOrEmpty(refreshed.RefreshToken) ? tokens.RefreshToken : refreshed.RefreshToken,
                refreshed.AccessTokenExpiration));

            return refreshed.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh threw");
            await _tokenStorage.ClearAsync();
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<AuthUser?> GetUserAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        if (DecodeJwtPayload(token) is not { } claims)
        {
            return new AuthUser(null, Array.Empty<string>());
        }

        var userName = GetString(claims, "name")
                       ?? GetString(claims, "preferred_username")
                       ?? GetString(claims, "email");

        return new AuthUser(userName, ReadRoles(claims));
    }

    private static string? GetString(JsonElement claims, string name) =>
        claims.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static IReadOnlyCollection<string> ReadRoles(JsonElement claims)
    {
        if (!claims.TryGetProperty("role", out var role) &&
            !claims.TryGetProperty("roles", out role))
        {
            return Array.Empty<string>();
        }

        return role.ValueKind switch
        {
            JsonValueKind.String => [role.GetString()!],
            JsonValueKind.Array => role.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString()!)
                .ToArray(),
            _ => Array.Empty<string>(),
        };
    }

    private static JsonElement? DecodeJwtPayload(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(payload);
            using var document = JsonDocument.Parse(bytes);
            return document.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }
}
