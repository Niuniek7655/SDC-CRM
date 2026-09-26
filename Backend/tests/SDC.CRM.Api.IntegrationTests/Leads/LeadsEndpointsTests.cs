using System.Net;
using System.Net.Http.Json;
using NSubstitute;
using SDC.CRM.Api.IntegrationTests.Infrastructure;
using SDC.CRM.Api.Observability;
using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Api.IntegrationTests.Leads;

/// <summary>
/// End-to-end behavior of /api/leads through the real HTTP pipeline: authentication, role policies,
/// ownership taken from the identity, domain rule violations mapped to HTTP 400 and the correlation header.
/// </summary>
public sealed class LeadsEndpointsTests
{
    private const string LeadsUrl = "/api/leads";
    private const string MyLeadsUrl = "/api/leads/mine";

    // A GUID subject maps 1:1 to the domain salesperson id (CurrentUser.DeriveDeterministicGuid).
    private static readonly Guid SalespersonId = Guid.Parse("7d1c1a52-1b8e-4f2e-9a57-5c3f0e6a9b10");

    private static object ValidRegisterLeadRequest(string companyName = "Acme Sp. z o.o.") => new
    {
        companyName,
        contactName = "Jan Kowalski",
        contactEmail = "jan.kowalski@acme.test",
        contactPhone = "+48 600 100 200",
        source = "Targi",
    };

    [Test]
    public async Task RegisterLead__When_request_has_no_credentials__Should_return_401_and_not_persist()
    {
        await using var factory = new CrmApiFactory();
        using var client = factory.CreateAnonymousClient();

        using var response = await client.PostAsJsonAsync(LeadsUrl, ValidRegisterLeadRequest());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await factory.Leads.DidNotReceive().AddAsync(Arg.Any<Lead>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterLead__When_user_has_no_sales_role__Should_return_403_and_not_persist()
    {
        await using var factory = new CrmApiFactory();
        using var client = factory.CreateClientFor(SalespersonId.ToString(), "BackofficeUser");

        using var response = await client.PostAsJsonAsync(LeadsUrl, ValidRegisterLeadRequest());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await factory.Leads.DidNotReceive().AddAsync(Arg.Any<Lead>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterLead__When_salesperson_sends_valid_lead__Should_return_201_with_id_and_assign_lead_to_caller()
    {
        await using var factory = new CrmApiFactory();
        using var client = factory.CreateClientFor(SalespersonId.ToString(), "Salesperson");

        using var response = await client.PostAsJsonAsync(LeadsUrl, ValidRegisterLeadRequest());
        var body = await response.Content.ReadFromJsonAsync<RegisterLeadResponseBody>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(body!.Id).IsNotEqualTo(Guid.Empty);
        await factory.Leads.Received(1).AddAsync(
            Arg.Is<Lead>(lead => lead.Id == body.Id && lead.AssignedSalespersonId == SalespersonId),
            Arg.Any<CancellationToken>());
        await factory.UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RegisterLead__When_business_rule_is_violated__Should_return_400_problem_details_and_not_save()
    {
        await using var factory = new CrmApiFactory();
        using var client = factory.CreateClientFor(SalespersonId.ToString(), "Salesperson");

        using var response = await client.PostAsJsonAsync(LeadsUrl, ValidRegisterLeadRequest(companyName: "   "));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsBody>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(problem!.Detail).Contains("company name");
        await factory.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetMyLeads__When_request_has_no_credentials__Should_return_401()
    {
        await using var factory = new CrmApiFactory();
        using var client = factory.CreateAnonymousClient();

        using var response = await client.GetAsync(MyLeadsUrl);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetMyLeads__When_user_has_no_sales_role__Should_return_403()
    {
        await using var factory = new CrmApiFactory();
        using var client = factory.CreateClientFor(SalespersonId.ToString(), "BackofficeManager");

        using var response = await client.GetAsync(MyLeadsUrl);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetMyLeads__When_salesperson_requests_leads__Should_return_leads_owned_by_the_caller()
    {
        await using var factory = new CrmApiFactory();
        var lead = Lead.Register(
            "Acme Sp. z o.o.", "Jan Kowalski", Email.Create("jan.kowalski@acme.test"), null, "Targi", SalespersonId);
        factory.Leads.ListBySalespersonAsync(SalespersonId, Arg.Any<CancellationToken>()).Returns([lead]);
        using var client = factory.CreateClientFor(SalespersonId.ToString(), "Salesperson");

        var leads = await client.GetFromJsonAsync<LeadSummaryBody[]>(MyLeadsUrl);

        await Assert.That(leads!.Length).IsEqualTo(1);
        await Assert.That(leads[0].Id).IsEqualTo(lead.Id);
        await Assert.That(leads[0].CompanyName).IsEqualTo("Acme Sp. z o.o.");
        await Assert.That(leads[0].Status).IsEqualTo("New");
    }

    [Test]
    public async Task AnyEndpoint__When_request_carries_correlation_id__Should_return_the_same_id_in_response()
    {
        await using var factory = new CrmApiFactory();
        factory.Leads.ListBySalespersonAsync(SalespersonId, Arg.Any<CancellationToken>()).Returns([]);
        using var client = factory.CreateClientFor(SalespersonId.ToString(), "Salesperson");
        using var request = new HttpRequestMessage(HttpMethod.Get, MyLeadsUrl);
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, "it-correlation-42");

        using var response = await client.SendAsync(request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single())
            .IsEqualTo("it-correlation-42");
    }

    // Response shapes as seen by API clients (web/mobile), deserialized with default web JSON options.
    private sealed record RegisterLeadResponseBody(Guid Id);

    private sealed record ProblemDetailsBody(string? Title, string? Detail, int? Status);

    private sealed record LeadSummaryBody(
        Guid Id,
        string CompanyName,
        string ContactName,
        string ContactEmail,
        string Status,
        DateTime CreatedAtUtc);
}

