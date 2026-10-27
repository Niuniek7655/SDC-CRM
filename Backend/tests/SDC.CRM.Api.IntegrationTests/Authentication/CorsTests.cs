using Microsoft.AspNetCore.Hosting;
using SDC.CRM.Api.IntegrationTests.Infrastructure;

namespace SDC.CRM.Api.IntegrationTests.Authentication;

public sealed class CorsTests
{
    [Test]
    [Arguments("http://localhost:4200", true)]
    [Arguments("https://untrusted.invalid", false)]
    public async Task Preflight__When_origin_is_evaluated__Should_only_allow_configured_spa(string origin, bool allowed)
    {
        await using var ports = new CrmApiFactory();
        await using var factory = ports.WithWebHostBuilder(builder => builder.UseSetting("Oidc:AllowedCorsOrigins:0", "http://localhost:4200"));
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/leads");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");
        using var response = await client.SendAsync(request);
        await Assert.That(response.Headers.Contains("Access-Control-Allow-Origin")).IsEqualTo(allowed);
        if (allowed)
            await Assert.That(response.Headers.GetValues("Access-Control-Allow-Origin").Single()).IsEqualTo(origin);
    }
}
