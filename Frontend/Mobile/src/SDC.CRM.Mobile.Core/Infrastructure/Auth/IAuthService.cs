namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// Handles the interactive OIDC login/logout and provides a valid access token
/// (refreshing transparently) for outgoing API calls.
/// </summary>
public interface IAuthService
{
    /// <summary>Runs the interactive Authorization Code + PKCE login.</summary>
    Task<AuthResult> LoginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// User-initiated sign-out: forgets the local session and ends the identity provider session in the
    /// system browser (best effort - local sign-out happens even if the browser step fails).
    /// </summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Forgets the local session only, without contacting the identity provider - e.g. when the API
    /// rejected the token (HTTP 401) and the user simply has to sign in again.
    /// </summary>
    Task ClearSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a non-expired access token, refreshing it when needed.
    /// Returns null when there is no usable session.
    /// </summary>
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>True when a stored (or refreshable) session exists.</summary>
    Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the current user's display name and CRM roles from a valid token.
    /// Returns null when not authenticated.
    /// </summary>
    Task<AuthUser?> GetUserAsync(CancellationToken cancellationToken = default);
}

/// <summary>Outcome of an interactive login.</summary>
public sealed record AuthResult(bool Succeeded, string? Error)
{
    public static AuthResult Success() => new(true, null);

    public static AuthResult Failure(string? error) => new(false, error);
}

/// <summary>Identity of the authenticated user (for UI display and gating).</summary>
public sealed record AuthUser(string? UserName, IReadOnlyCollection<string> Roles)
{
    public bool IsInRole(string role) => Roles.Contains(role);
}
