namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// Seam over the OIDC protocol client (Duende OidcClient + system browser). Keeps the session rules in
/// <see cref="OidcAuthService"/> unit-testable without an identity provider; implemented by
/// <see cref="DuendeOidcSessionClient"/>.
/// </summary>
public interface IOidcSessionClient
{
    /// <summary>Interactive Authorization Code + PKCE sign-in in the system browser.</summary>
    Task<OidcSessionResult> LoginAsync(CancellationToken cancellationToken);

    /// <summary>Exchanges a refresh token for new tokens (no UI).</summary>
    Task<OidcSessionResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Ends the identity provider session (end_session endpoint) in the system browser, so the next
    /// sign-in asks for credentials again instead of silently reusing the previous account.
    /// </summary>
    Task<OidcLogoutResult> EndSessionAsync(string identityToken, CancellationToken cancellationToken);
}

/// <summary>Tokens issued by the identity provider.</summary>
public sealed record OidcTokens(
    string AccessToken,
    string? RefreshToken,
    string? IdentityToken,
    DateTimeOffset AccessTokenExpiresAt);

/// <summary>Outcome of a sign-in or refresh: tokens on success, an error code otherwise.</summary>
public sealed record OidcSessionResult(OidcTokens? Tokens, string? Error)
{
    public bool IsError => Tokens is null;

    public static OidcSessionResult Success(OidcTokens tokens) => new(tokens, null);

    public static OidcSessionResult Failure(string? error) => new(null, error ?? "unknown_error");
}

/// <summary>Outcome of ending the identity provider session.</summary>
public sealed record OidcLogoutResult(string? Error)
{
    public bool IsError => Error is not null;

    public static OidcLogoutResult Success() => new((string?)null);

    public static OidcLogoutResult Failure(string? error) => new(error ?? "unknown_error");
}

