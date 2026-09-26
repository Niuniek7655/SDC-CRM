using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// Session rules of the mobile client on top of <see cref="IOidcSessionClient"/>: tokens are kept in
/// <see cref="ITokenStorage"/> (secure storage), refreshed shortly before they expire and forgotten when
/// the refresh fails; sign-out also ends the identity provider session.
/// </summary>
public sealed class OidcAuthService(
    ITokenStorage tokenStorage,
    IOidcSessionClient sessionClient,
    TimeProvider timeProvider,
    ILogger<OidcAuthService> logger) : IAuthService
{
    private static readonly TimeSpan ExpiryLeeway = TimeSpan.FromSeconds(60);

    public async Task<AuthResult> LoginAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await sessionClient.LoginAsync(cancellationToken);

            if (result.IsError)
            {
                logger.LogWarning("OIDC login failed: {Error}", result.Error);
                return AuthResult.Failure(result.Error);
            }

            await tokenStorage.SaveAsync(ToTokenSet(result.Tokens!, previous: null));

            return AuthResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OIDC login threw");
            return AuthResult.Failure(ex.Message);
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var tokens = await tokenStorage.GetAsync();

        // Local sign-out first: the device forgets the session even if the browser step fails or is cancelled.
        await tokenStorage.ClearAsync();

        if (string.IsNullOrEmpty(tokens?.IdentityToken))
        {
            return;
        }

        try
        {
            var result = await sessionClient.EndSessionAsync(tokens.IdentityToken, cancellationToken);
            if (result.IsError)
            {
                logger.LogWarning("OIDC end-session failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OIDC end-session threw");
        }
    }

    public Task ClearSessionAsync(CancellationToken cancellationToken = default) => tokenStorage.ClearAsync();

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var tokens = await tokenStorage.GetAsync();
        if (tokens is null)
        {
            return null;
        }

        if (tokens.ExpiresAt - ExpiryLeeway > timeProvider.GetUtcNow())
        {
            return tokens.AccessToken;
        }

        if (string.IsNullOrEmpty(tokens.RefreshToken))
        {
            await tokenStorage.ClearAsync();
            return null;
        }

        try
        {
            var refreshed = await sessionClient.RefreshAsync(tokens.RefreshToken, cancellationToken);
            if (refreshed.IsError)
            {
                logger.LogWarning("Token refresh failed: {Error}", refreshed.Error);
                await tokenStorage.ClearAsync();
                return null;
            }

            var updated = ToTokenSet(refreshed.Tokens!, previous: tokens);
            await tokenStorage.SaveAsync(updated);

            return updated.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token refresh threw");
            await tokenStorage.ClearAsync();
            return null;
        }
    }

    /// <summary>Keeps the previous refresh/identity token when the identity provider does not rotate it.</summary>
    private static TokenSet ToTokenSet(OidcTokens tokens, TokenSet? previous) => new(
        tokens.AccessToken,
        string.IsNullOrEmpty(tokens.RefreshToken) ? previous?.RefreshToken ?? string.Empty : tokens.RefreshToken,
        tokens.AccessTokenExpiresAt,
        string.IsNullOrEmpty(tokens.IdentityToken) ? previous?.IdentityToken : tokens.IdentityToken);

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
