using System.Net;
using SDC.CRM.Mobile.Infrastructure.Api;
using SDC.CRM.Mobile.Infrastructure.Api.Contracts;
using SDC.CRM.Mobile.Tests.TestDoubles;

namespace SDC.CRM.Mobile.Tests.Infrastructure.Api;

public sealed class CrmApiClientTests
{
    private const string LeadJson =
        """
        [{
          "id": "3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77",
          "companyName": "Acme Sp. z o.o.",
          "contactName": "Jan Kowalski",
          "contactEmail": "jan.kowalski@acme.test",
          "status": "New",
          "createdAtUtc": "2026-09-26T10:00:00Z"
        }]
        """;

    private static CrmApiClient ClientFor(StubHttpMessageHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("http://api.test/") });

    private static RegisterLeadRequest ValidRequest()
        => new("Acme Sp. z o.o.", "Jan Kowalski", "jan.kowalski@acme.test", null, "Targi");

    [Test]
    public async Task GetMyLeadsAsync__When_api_returns_leads__Should_request_my_leads_and_map_them()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, LeadJson);

        var leads = await ClientFor(handler).GetMyLeadsAsync();

        await Assert.That(handler.Requests.Single().RequestUri!.AbsolutePath).IsEqualTo("/api/leads/mine");
        await Assert.That(leads.Count).IsEqualTo(1);
        await Assert.That(leads[0].CompanyName).IsEqualTo("Acme Sp. z o.o.");
        await Assert.That(leads[0].Status).IsEqualTo("New");
    }

    [Test]
    public async Task GetMyLeadsAsync__When_api_returns_401__Should_throw_unauthorized_exception()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.Unauthorized);

        await Assert.That(async () => await ClientFor(handler).GetMyLeadsAsync())
            .Throws<CrmUnauthorizedException>();
    }

    [Test]
    public async Task GetMyLeadsAsync__When_api_returns_403__Should_throw_forbidden_exception_not_unauthorized()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.Forbidden);

        // 403 means "signed in, but the role is not allowed" - the session itself is still valid.
        await Assert.That(async () => await ClientFor(handler).GetMyLeadsAsync())
            .ThrowsExactly<CrmForbiddenException>();
    }

    [Test]
    public async Task RegisterLeadAsync__When_api_returns_403__Should_throw_forbidden_exception()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.Forbidden);

        await Assert.That(async () => await ClientFor(handler).RegisterLeadAsync(ValidRequest()))
            .ThrowsExactly<CrmForbiddenException>();
    }

    [Test]
    public async Task RegisterLeadAsync__When_api_creates_lead__Should_post_request_and_return_new_id()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.Created, """{ "id": "3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77" }""");

        var response = await ClientFor(handler).RegisterLeadAsync(ValidRequest());

        var request = handler.Requests.Single();
        await Assert.That(request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(request.RequestUri!.AbsolutePath).IsEqualTo("/api/leads");
        await Assert.That(response.Id).IsEqualTo(Guid.Parse("3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77"));
    }

    [Test]
    public async Task RegisterLeadAsync__When_api_rejects_request__Should_throw_api_exception_with_problem_details()
    {
        var handler = StubHttpMessageHandler.Returning(
            HttpStatusCode.BadRequest,
            """{ "title": "Business rule violated", "detail": "A lead must have a company name.", "status": 400 }""");

        var exception = await Assert.That(async () => await ClientFor(handler).RegisterLeadAsync(ValidRequest()))
            .Throws<CrmApiException>();

        await Assert.That(exception!.Detail).Contains("A lead must have a company name.");
    }
}

