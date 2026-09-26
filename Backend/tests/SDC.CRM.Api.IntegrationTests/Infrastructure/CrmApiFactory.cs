using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using SDC.CRM.Application.Abstractions;
using SDC.CRM.Application.Abstractions.Persistence;

namespace SDC.CRM.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Hosts the real SDC.CRM.Api in memory (TestServer) with:
/// <list type="bullet">
///   <item>environment "Testing" and placeholder settings required by the fail-fast configuration checks,</item>
///   <item>the test authentication scheme instead of JWT bearer validation,</item>
///   <item>NSubstitute substitutes for the persistence ports (no database, no network).</item>
/// </list>
/// Create one factory per test so substitutes never leak between tests.
/// </summary>
public sealed class CrmApiFactory : WebApplicationFactory<Program>
{
    public ILeadRepository Leads { get; } = Substitute.For<ILeadRepository>();

    public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();

    /// <summary>HTTP client without credentials (anonymous caller).</summary>
    public HttpClient CreateAnonymousClient() => CreateClient();

    /// <summary>HTTP client authenticated as <paramref name="subject"/> with the given CRM roles.</summary>
    public HttpClient CreateClientFor(string subject, params string[] roles)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.SubjectHeader, subject);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RolesHeader, string.Join(',', roles));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Required by the fail-fast checks; never contacted (repositories are substituted, tokens are not validated).
        builder.UseSetting("ConnectionStrings:Crm", "Host=unused.invalid;Database=unused");
        builder.UseSetting("Oidc:Authority", "https://sso.invalid/master");

        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, _ => { });

            services.RemoveAll<ILeadRepository>();
            services.AddSingleton(Leads);

            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton(UnitOfWork);
        });
    }
}

