using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Domain.Tests.Leads;

public sealed class LeadTests
{
    private static Lead RegisterSampleLead()
        => Lead.Register(
            companyName: "Acme Sp. z o.o.",
            contactName: "Jan Kowalski",
            contactEmail: Email.Create("jan.kowalski@acme.test"),
            contactPhone: "+48 600 100 200",
            source: "Targi",
            assignedSalespersonId: Guid.NewGuid());

    [Test]
    public async Task Register__When_lead_is_registered__Should_start_in_new_status_with_generated_id()
    {
        var lead = RegisterSampleLead();

        await Assert.That(lead.Status).IsEqualTo(LeadStatus.New);
        await Assert.That(lead.Id).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task Register__When_company_name_is_missing__Should_fail_validation()
    {
        await Assert.That(() => Lead.Register(
                companyName: "   ",
                contactName: "Jan Kowalski",
                contactEmail: Email.Create("jan@acme.test"),
                contactPhone: null,
                source: null,
                assignedSalespersonId: Guid.NewGuid()))
            .Throws<DomainException>()
            .WithMessageContaining("company name");
    }

    [Test]
    public async Task Qualify__When_lead_is_rejected__Should_fail()
    {
        var lead = RegisterSampleLead();
        lead.Reject("Brak budżetu");

        await Assert.That(() => lead.Qualify()).Throws<DomainException>();
    }

    [Test]
    public async Task Reject__When_rejection_reason_is_missing__Should_fail_validation()
    {
        var lead = RegisterSampleLead();

        await Assert.That(() => lead.Reject("  ")).Throws<DomainException>();
    }

    [Test]
    public async Task Qualify__When_lead_is_new__Should_set_qualified_status()
    {
        var lead = RegisterSampleLead();

        lead.Qualify();

        await Assert.That(lead.Status).IsEqualTo(LeadStatus.Qualified);
    }
}
