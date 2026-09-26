using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SDC.CRM.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Test authentication scheme replacing JWT bearer validation. A request is authenticated only when it
/// carries <see cref="SubjectHeader"/>; <see cref="RolesHeader"/> lists the CRM roles (comma separated).
/// Claims use the same types as the identity provider tokens ("sub", "role", "name"), so
/// <c>CurrentUser</c> and the authorization policies behave exactly as in production.
/// </summary>
public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string SubjectHeader = "X-Test-Subject";
    public const string RolesHeader = "X-Test-Roles";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(SubjectHeader, out var subject) || string.IsNullOrWhiteSpace(subject))
        {
            // No credentials: [Authorize] endpoints answer 401 through the default challenge.
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new("sub", subject.ToString()),
            new("name", "Integration Test User"),
        };

        var roles = Request.Headers[RolesHeader].ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var identity = new ClaimsIdentity(claims, SchemeName, nameType: "name", roleType: "role");
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

