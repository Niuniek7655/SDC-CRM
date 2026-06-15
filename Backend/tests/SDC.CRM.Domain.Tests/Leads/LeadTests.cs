using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads;
using Xunit;

namespace SDC.CRM.Domain.Tests.Leads;

public sealed class LeadTests
{
    private static Lead RegisterLead()
        => Lead.Register(
            companyName: "Acme Sp. z o.o.",
            contactName: "Jan Kowalski",
            contactEmail: Email.Create("jan.kowalski@acme.test"),
            contactPhone: "+48 600 100 200",
            source: "Targi",
            assignedSalespersonId: Guid.NewGuid());

    [Fact]
    public void RegisterLead_ShouldStartInNewStatus()
    {
        var lead = RegisterLead();

        Assert.Equal(LeadStatus.New, lead.Status);
        Assert.NotEqual(Guid.Empty, lead.Id);
    }

    [Fact]
    public void RegisterLead_ShouldRequireCompanyName()
    {
        var exception = Assert.Throws<DomainException>(() => Lead.Register(
            companyName: "   ",
            contactName: "Jan Kowalski",
            contactEmail: Email.Create("jan@acme.test"),
            contactPhone: null,
            source: null,
            assignedSalespersonId: Guid.NewGuid()));

        Assert.Contains("company name", exception.Message);
    }

    [Fact]
    public void QualifyLead_ShouldFail_WhenLeadIsRejected()
    {
        var lead = RegisterLead();
        lead.Reject("Brak budżetu");

        Assert.Throws<DomainException>(lead.Qualify);
    }

    [Fact]
    public void RejectLead_ShouldRequireRejectionReason()
    {
        var lead = RegisterLead();

        Assert.Throws<DomainException>(() => lead.Reject("  "));
    }

    [Fact]
    public void QualifyLead_ShouldSetQualifiedStatus()
    {
        var lead = RegisterLead();

        lead.Qualify();

        Assert.Equal(LeadStatus.Qualified, lead.Status);
    }
}
