using NSubstitute;
using SDC.CRM.Application.Abstractions;
using SDC.CRM.Application.Abstractions.Persistence;
using SDC.CRM.Application.Leads.RegisterLead;
using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Application.Tests.Leads.RegisterLead;

public sealed class RegisterLeadHandlerTests
{
    private readonly ILeadRepository _leads = Substitute.For<ILeadRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private RegisterLeadHandler CreateHandler() => new(_leads, _unitOfWork);

    private static RegisterLeadCommand CommandWith(
        string email = "jan.kowalski@acme.test",
        Guid? salespersonId = null)
        => new(
            CompanyName: "Acme Sp. z o.o.",
            ContactName: "Jan Kowalski",
            ContactEmail: email,
            ContactPhone: null,
            Source: "Polecenie",
            AssignedSalespersonId: salespersonId ?? Guid.NewGuid());

    [Test]
    public async Task HandleAsync__When_command_is_valid__Should_persist_lead_and_return_id()
    {
        var salespersonId = Guid.NewGuid();
        var handler = CreateHandler();

        var leadId = await handler.HandleAsync(CommandWith(salespersonId: salespersonId));

        await Assert.That(leadId).IsNotEqualTo(Guid.Empty);
        await _leads.Received(1).AddAsync(
            Arg.Is<Lead>(lead => lead.AssignedSalespersonId == salespersonId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleAsync__When_email_is_invalid__Should_fail_validation_and_not_persist()
    {
        var handler = CreateHandler();

        await Assert.That(async () => await handler.HandleAsync(CommandWith(email: "not-an-email")))
            .Throws<DomainException>();

        await _leads.DidNotReceive().AddAsync(Arg.Any<Lead>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
