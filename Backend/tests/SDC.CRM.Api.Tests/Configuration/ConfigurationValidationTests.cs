using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDC.CRM.Api.Authentication;
using SDC.CRM.Infrastructure;
using SDC.CRM.Infrastructure.Persistence;

namespace SDC.CRM.Api.Tests.Configuration;

/// <summary>
/// The composition root must fail fast when environment-specific settings are missing,
/// instead of silently falling back to development values (connection string, identity provider).
/// </summary>
public sealed class ConfigurationValidationTests
{
    private static IConfiguration ConfigurationWith(params (string Key, string? Value)[] settings)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(setting => new KeyValuePair<string, string?>(setting.Key, setting.Value)))
            .Build();

    [Test]
    public async Task AddInfrastructure__When_connection_string_is_missing__Should_throw_with_configuration_hint()
    {
        var services = new ServiceCollection();
        var configuration = ConfigurationWith();

        await Assert.That(() => services.AddInfrastructure(configuration))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("ConnectionStrings__Crm");
    }

    [Test]
    public async Task AddInfrastructure__When_connection_string_is_configured__Should_register_db_context()
    {
        var services = new ServiceCollection();
        var configuration = ConfigurationWith(("ConnectionStrings:Crm", "Host=db.test;Database=crm"));

        services.AddInfrastructure(configuration);

        await Assert.That(services.Any(descriptor => descriptor.ServiceType == typeof(CrmDbContext))).IsTrue();
    }

    [Test]
    public async Task AddCrmAuthentication__When_authority_is_missing__Should_throw_with_configuration_hint()
    {
        var services = new ServiceCollection();
        var configuration = ConfigurationWith(("Oidc:Audience", "sdc-crm-api"));

        await Assert.That(() => services.AddCrmAuthentication(configuration))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("Oidc__Authority");
    }

    [Test]
    public async Task AddCrmAuthentication__When_authority_is_configured__Should_register_oidc_options()
    {
        var services = new ServiceCollection();
        var configuration = ConfigurationWith(("Oidc:Authority", "https://sso.test/master"));

        services.AddCrmAuthentication(configuration);

        var options = services.BuildServiceProvider().GetRequiredService<OidcOptions>();
        await Assert.That(options.Authority).IsEqualTo("https://sso.test/master");
    }
}

