namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// Handles the interactive OIDC login/logout and provides a valid access token
/// (refreshing transparently) for outgoing API calls.
/// </summary>
public interface IAuthService
{
    /// <summary>Runs the interactive Authorization Code + PKCE login.</summary>
    Task<AuthResult> LoginAsync(CancellationToken cancellationToken = default);

    /// <summary>Clears the local session (and best-effort server sign-out).</summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);

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
