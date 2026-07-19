using NSubstitute;
using SDC.CRM.Application.Abstractions.Persistence;
using SDC.CRM.Application.Leads.GetMyLeads;
using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Application.Tests.Leads.GetMyLeads;

public sealed class GetMyLeadsHandlerTests
{
    private readonly ILeadRepository _leads = Substitute.For<ILeadRepository>();

    [Test]
    public async Task HandleAsync__When_salesperson_has_leads__Should_return_mapped_summaries()
    {
        var salespersonId = Guid.NewGuid();
        var lead = Lead.Register("Acme", "Jan", Email.Create("jan@acme.test"), null, null, salespersonId);
        _leads.ListBySalespersonAsync(salespersonId, Arg.Any<CancellationToken>())
            .Returns([lead]);
        var handler = new GetMyLeadsHandler(_leads);

        var result = await handler.HandleAsync(new GetMyLeadsQuery(salespersonId));

        await Assert.That(result.Count).IsEqualTo(1);
        await Assert.That(result[0].CompanyName).IsEqualTo("Acme");
        await Assert.That(result[0].ContactEmail).IsEqualTo("jan@acme.test");
        await Assert.That(result[0].Status).IsEqualTo("New");
    }

    [Test]
    public async Task HandleAsync__When_salesperson_has_no_leads__Should_return_empty_list()
    {
        var salespersonId = Guid.NewGuid();
        _leads.ListBySalespersonAsync(salespersonId, Arg.Any<CancellationToken>())
            .Returns([]);
        var handler = new GetMyLeadsHandler(_leads);

        var result = await handler.HandleAsync(new GetMyLeadsQuery(salespersonId));

        await Assert.That(result).IsEmpty();
    }
}
