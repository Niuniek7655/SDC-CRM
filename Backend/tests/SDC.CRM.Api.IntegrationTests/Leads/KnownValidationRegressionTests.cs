using System.Net;
using System.Net.Http.Json;
using NSubstitute;
using SDC.CRM.Api.IntegrationTests.Infrastructure;

namespace SDC.CRM.Api.IntegrationTests.Leads;

// Desired API contract for known missing validation. Explicit red tests, not default-suite passes.
[Explicit]
[Category("KnownRegression")]
public sealed class KnownValidationRegressionTests
{
    [Test]
    [Arguments("companyName", 200)]
    [Arguments("contactName", 200)]
    [Arguments("contactEmail", 320)]
    [Arguments("contactPhone", 50)]
    [Arguments("source", 100)]
    public async Task RegisterLead__When_column_limit_is_exceeded__Should_return_400_without_saving(string field, int limit)
    {
        await using var factory = new CrmApiFactory();
        using var client = factory.CreateClientFor("7d1c1a52-1b8e-4f2e-9a57-5c3f0e6a9b10", "Salesperson");
        var body = new Dictionary<string, string> { ["companyName"] = "Acme", ["contactName"] = "Jan", ["contactEmail"] = "jan@example.test" };
        body[field] = field == "contactEmail" ? new string('a', limit - "@example.test".Length + 1) + "@example.test" : new string('a', limit + 1);
        using var response = await client.PostAsJsonAsync("/api/leads", body);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await factory.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
