using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using SDC.CRM.Api.Authentication;
using SDC.CRM.Api.Identity;

namespace SDC.CRM.Api.Tests.Identity;

/// <summary>
/// Verifies how the authenticated identity is projected from the validated
/// access token claims onto the CRM <see cref="ICurrentUser"/> surface.
/// </summary>
public sealed class CurrentUserTests
{
    private static CurrentUser CreateCurrentUser(ClaimsPrincipal? principal, OidcOptions? options = null)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        if (principal is not null)
        {
            accessor.HttpContext.Returns(new DefaultHttpContext { User = principal });
        }

        return new CurrentUser(accessor, options ?? new OidcOptions());
    }

    private static ClaimsPrincipal PrincipalWith(params Claim[] claims)
        => new(new ClaimsIdentity(claims, authenticationType: "TestAuth"));

    [Test]
    public async Task IsAuthenticated__When_there_is_no_http_context__Should_be_false_with_empty_id()
    {
        var currentUser = CreateCurrentUser(principal: null);

        await Assert.That(currentUser.IsAuthenticated).IsFalse();
        await Assert.That(currentUser.Id).IsEqualTo(Guid.Empty);
    }

    [Test]
    public async Task Id__When_subject_is_a_guid__Should_return_that_guid()
    {
        var subject = Guid.NewGuid();

        var currentUser = CreateCurrentUser(PrincipalWith(new Claim("sub", subject.ToString())));

        await Assert.That(currentUser.Id).IsEqualTo(subject);
    }

    [Test]
    public async Task Id__When_subject_is_not_a_guid__Should_be_deterministic_across_instances()
    {
        var first = CreateCurrentUser(PrincipalWith(new Claim("sub", "sso|user-123")));
        var second = CreateCurrentUser(PrincipalWith(new Claim("sub", "sso|user-123")));

        await Assert.That(first.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(first.Id).IsEqualTo(second.Id);
    }

    [Test]
    public async Task Id__When_subjects_differ__Should_produce_different_ids()
    {
        var a = CreateCurrentUser(PrincipalWith(new Claim("sub", "user-a")));
        var b = CreateCurrentUser(PrincipalWith(new Claim("sub", "user-b")));

        await Assert.That(a.Id).IsNotEqualTo(b.Id);
    }

    [Test]
    public async Task Roles__When_role_claims_are_present__Should_expose_distinct_roles()
    {
        var currentUser = CreateCurrentUser(PrincipalWith(
            new Claim("sub", "user-1"),
            new Claim("role", "Salesperson"),
            new Claim("role", "Admin"),
            new Claim("role", "Admin")));

        await Assert.That(currentUser.Roles.Count).IsEqualTo(2);
        await Assert.That(currentUser.IsInRole("Salesperson")).IsTrue();
        await Assert.That(currentUser.IsInRole("Admin")).IsTrue();
        await Assert.That(currentUser.IsInRole("BackofficeUser")).IsFalse();
    }

    [Test]
    public async Task UserName_and_Email__When_claims_are_present__Should_be_read_from_token()
    {
        var currentUser = CreateCurrentUser(PrincipalWith(
            new Claim("sub", "user-1"),
            new Claim("name", "Jan Kowalski"),
            new Claim("email", "jan@acme.test")));

        await Assert.That(currentUser.UserName).IsEqualTo("Jan Kowalski");
        await Assert.That(currentUser.Email).IsEqualTo("jan@acme.test");
    }
}
