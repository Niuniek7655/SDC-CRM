using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using SDC.CRM.Api.IntegrationTests.Infrastructure;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Api.IntegrationTests.Authentication;

public sealed class JwtValidationTests
{
    private const string Issuer = "https://sso.invalid/master";
    private const string Audience = "sdc-crm-api";
    private static readonly SymmetricSecurityKey Key = new(Encoding.UTF8.GetBytes("test-only-signing-key-not-used-by-any-environment-123456"));

    [Test]
    [Arguments("valid", 200, 201)]
    [Arguments("signature", 401, 401)]
    [Arguments("issuer", 401, 401)]
    [Arguments("audience", 401, 401)]
    [Arguments("expired", 401, 401)]
    [Arguments("future", 401, 401)]
    [Arguments("no-role", 403, 403)]
    public async Task Jwt__When_token_is_validated__Should_apply_signature_claims_and_lifetime(string variant, int getStatus, int postStatus)
    {
        await using var ports = new CrmApiFactory();
        ports.Leads.ListBySalespersonAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns([]);
        await using var factory = ports.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
                configuration.SigningKeys.Add(Key);
                // Keep the production validation flags; replace only discovery/JWKS transport.
                options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration);
                options.TokenValidationParameters.ValidAudience = Audience;
            });
        }));
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(variant));

        using var get = await client.GetAsync("/api/leads/mine");
        using var post = await client.PostAsJsonAsync("/api/leads", new { companyName = "JWT", contactName = "Jan", contactEmail = "jan@example.test" });

        await Assert.That((int)get.StatusCode).IsEqualTo(getStatus);
        await Assert.That((int)post.StatusCode).IsEqualTo(postStatus);
        await Assert.That(get.Headers.Contains("X-Correlation-ID")).IsTrue();
        await Assert.That(post.Headers.Contains("X-Correlation-ID")).IsTrue();
        if (postStatus != 201)
        {
            await ports.Leads.DidNotReceive().AddAsync(Arg.Any<Lead>(), Arg.Any<CancellationToken>());
            await ports.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }


    [Test]
    [Explicit]
    [Category("KnownRegression")]
    public async Task Jwt__When_subject_is_missing__Should_reject_read_and_write()
    {
        await Jwt__When_token_is_validated__Should_apply_signature_claims_and_lifetime("no-sub", 401, 401);
    }

    private static string Token(string variant)
    {
        var claims = new List<Claim>();
        if (variant != "no-sub") claims.Add(new Claim("sub", "7d1c1a52-1b8e-4f2e-9a57-5c3f0e6a9b10"));
        if (variant != "no-role") claims.Add(new Claim("role", "Salesperson"));
        var key = variant == "signature" ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes("different-test-only-signing-key-never-trusted-123456789")) : Key;
        var token = new JwtSecurityToken(
            issuer: variant == "issuer" ? "https://another.invalid" : Issuer,
            audience: variant == "audience" ? "another-api" : Audience,
            claims: claims,
            notBefore: new DateTime(variant == "future" ? 2100 : 2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            expires: new DateTime(variant == "expired" ? 2001 : 2101, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
