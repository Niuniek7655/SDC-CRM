using SDC.CRM.Application.Leads.GetMyLeads;
using SDC.CRM.Application.Leads.RegisterLead;
using SDC.CRM.Application.Tests.Doubles;
using SDC.CRM.Domain.Common;
using Xunit;

namespace SDC.CRM.Application.Tests.Leads;

public sealed class RegisterLeadHandlerTests
{
    [Fact]
    public async Task RegisterLead_ShouldPersistLead_AndReturnId()
    {
        var repository = new InMemoryLeadRepository();
        var handler = new RegisterLeadHandler(repository, repository);
        var salespersonId = Guid.NewGuid();

        var command = new RegisterLeadCommand(
            CompanyName: "Acme Sp. z o.o.",
            ContactName: "Jan Kowalski",
            ContactEmail: "jan.kowalski@acme.test",
            ContactPhone: null,
            Source: "Polecenie",
            AssignedSalespersonId: salespersonId);

        var leadId = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, leadId);
        Assert.Single(repository.Stored);
        Assert.Equal(salespersonId, repository.Stored[0].AssignedSalespersonId);
    }

    [Fact]
    public async Task RegisterLead_ShouldFail_WhenEmailIsInvalid()
    {
        var repository = new InMemoryLeadRepository();
        var handler = new RegisterLeadHandler(repository, repository);

        var command = new RegisterLeadCommand(
            CompanyName: "Acme Sp. z o.o.",
            ContactName: "Jan Kowalski",
            ContactEmail: "not-an-email",
            ContactPhone: null,
            Source: null,
            AssignedSalespersonId: Guid.NewGuid());

        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Empty(repository.Stored);
    }

    [Fact]
    public async Task GetMyLeads_ShouldReturnOnlyOwnLeads()
    {
        var repository = new InMemoryLeadRepository();
        var registerHandler = new RegisterLeadHandler(repository, repository);
        var getMyLeadsHandler = new GetMyLeadsHandler(repository);

        var mySalespersonId = Guid.NewGuid();
        var otherSalespersonId = Guid.NewGuid();

        await registerHandler.HandleAsync(new RegisterLeadCommand(
            "Acme", "Jan", "jan@acme.test", null, null, mySalespersonId));
        await registerHandler.HandleAsync(new RegisterLeadCommand(
            "Globex", "Anna", "anna@globex.test", null, null, otherSalespersonId));

        var myLeads = await getMyLeadsHandler.HandleAsync(new GetMyLeadsQuery(mySalespersonId));

        Assert.Single(myLeads);
        Assert.Equal("Acme", myLeads[0].CompanyName);
    }
}
