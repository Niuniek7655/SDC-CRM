namespace SDC.CRM.Application.Abstractions.Identity;

/// <summary>
/// Access to the identity of the user making the current request.
/// Implemented in the presentation layer from the validated access token,
/// so use-case handlers never trust client-supplied identifiers.
/// </summary>
public interface ICurrentUser
{
    /// <summary>True when the request carries a valid authenticated identity.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Stable identity used across the CRM domain (e.g. the owning salesperson).
    /// Derived deterministically from the identity provider subject so it stays
    /// constant for a given SSO user even though the domain models it as a Guid.
    /// </summary>
    Guid Id { get; }

    /// <summary>Raw identity provider subject (the OIDC <c>sub</c> claim).</summary>
    string? Subject { get; }

    /// <summary>Human-readable user name / preferred_username when present.</summary>
    string? UserName { get; }

    /// <summary>Email claim when present.</summary>
    string? Email { get; }

    /// <summary>CRM roles granted to the user (e.g. Salesperson, Admin).</summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>True when the user has the given CRM role.</summary>
    bool IsInRole(string role);
}
