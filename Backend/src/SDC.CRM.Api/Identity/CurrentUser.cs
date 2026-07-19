using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SDC.CRM.Api.Authentication;
using SDC.CRM.Application.Abstractions.Identity;

namespace SDC.CRM.Api.Identity;

/// <summary>
/// Reads the current identity from the validated JWT on the HTTP context.
/// The domain models the owning salesperson as a <see cref="Guid"/>, while the
/// identity provider subject is an opaque string; we derive a stable Guid from
/// the subject so a given SSO user always maps to the same domain identifier.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor, OidcOptions options) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? Subject =>
        Principal?.FindFirstValue("sub")
        ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        Principal?.FindFirstValue(options.NameClaimType)
        ?? Principal?.FindFirstValue("preferred_username")
        ?? Principal?.FindFirstValue("name");

    public string? Email => Principal?.FindFirstValue("email");

    public Guid Id => DeriveDeterministicGuid(Subject);

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(options.RoleClaimType).Select(c => c.Value).Distinct().ToArray()
        ?? [];

    public bool IsInRole(string role) =>
        Principal?.HasClaim(options.RoleClaimType, role) ?? false;

    /// <summary>
    /// Produces a stable, RFC 4122-shaped Guid from the identity provider subject.
    /// Same input always yields the same Guid, so it is safe to use as an owning
    /// key without persisting an additional mapping table.
    /// </summary>
    internal static Guid DeriveDeterministicGuid(string? subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Guid.Empty;
        }

        if (Guid.TryParse(subject, out var parsed))
        {
            return parsed;
        }

        Span<byte> hash = stackalloc byte[16];
        var input = Encoding.UTF8.GetBytes(subject);
        MD5.HashData(input, hash);

        // Stamp RFC 4122 version 3 (name-based, MD5) and the variant bits so the
        // value is a well-formed, standards-correct name-based UUID.
        hash[6] = (byte)((hash[6] & 0x0F) | 0x30);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new Guid(hash);
    }
}
