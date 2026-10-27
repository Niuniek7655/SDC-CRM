using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads;
using SDC.CRM.Domain.Leads.Events;

namespace SDC.CRM.Domain.Tests.Leads;

public sealed class LeadBehaviorTests
{
    private static readonly Guid Owner = Guid.Parse("7d1c1a52-1b8e-4f2e-9a57-5c3f0e6a9b10");
    private static Lead Register(string? phone = " 123 ", string? source = " Targi ") =>
        Lead.Register("  Żółw SA  ", "  Jan Kowalski  ", Email.Create(" jan@example.test "), phone, source, Owner);

    [Test]
    public async Task Register__When_names_have_surrounding_whitespace__Should_normalize_contact_data()
    {
        var lead = Register();
        await Assert.That(lead.CompanyName).IsEqualTo("Żółw SA");
        await Assert.That(lead.ContactName).IsEqualTo("Jan Kowalski");
        await Assert.That(lead.ContactEmail.Value).IsEqualTo("jan@example.test");
        await Assert.That(lead.ContactPhone).IsEqualTo("123");
        await Assert.That(lead.Source).IsEqualTo("Targi");
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Register__When_optional_fields_are_blank__Should_store_null(string? blank)
    {
        var lead = Register(blank, blank);
        await Assert.That(lead.ContactPhone).IsNull();
        await Assert.That(lead.Source).IsNull();
    }

    [Test]
    public async Task Register__When_salesperson_is_missing__Should_reject_registration()
    {
        await Assert.That(() => Lead.Register("Acme", "Jan", Email.Create("jan@example.test"), null, null, Guid.Empty))
            .Throws<DomainException>().WithMessageContaining("salesperson");
    }

    [Test]
    public async Task Register__When_succeeds__Should_record_identity_and_owner_in_event()
    {
        var lead = Register();
        var registered = (LeadRegistered)lead.DomainEvents.Single();
        await Assert.That(registered.LeadId).IsEqualTo(lead.Id);
        await Assert.That(registered.CompanyName).IsEqualTo(lead.CompanyName);
        await Assert.That(registered.AssignedSalespersonId).IsEqualTo(Owner);
    }

    [Test]
    public async Task Qualify__When_called_twice__Should_record_only_one_qualification()
    {
        var lead = Register();
        lead.ClearDomainEvents();
        lead.Qualify();
        lead.Qualify();
        await Assert.That(lead.Status).IsEqualTo(LeadStatus.Qualified);
        await Assert.That(lead.DomainEvents.Count).IsEqualTo(1);
        await Assert.That(((LeadQualified)lead.DomainEvents.Single()).LeadId).IsEqualTo(lead.Id);
    }

    [Test]
    public async Task Reject__When_reason_is_valid__Should_record_normalized_reason_and_event()
    {
        var lead = Register();
        lead.ClearDomainEvents();
        lead.Reject("  Brak budżetu  ");
        await Assert.That(lead.Status).IsEqualTo(LeadStatus.Rejected);
        await Assert.That(lead.RejectionReason).IsEqualTo("Brak budżetu");
        var rejected = (LeadRejected)lead.DomainEvents.Single();
        await Assert.That(rejected.LeadId).IsEqualTo(lead.Id);
        await Assert.That(rejected.Reason).IsEqualTo("Brak budżetu");
    }

    [Test]
    public async Task Reject__When_reason_is_blank__Should_preserve_state_and_events()
    {
        var lead = Register();
        lead.ClearDomainEvents();
        await Assert.That(() => lead.Reject(" ")).Throws<DomainException>();
        await Assert.That(lead.Status).IsEqualTo(LeadStatus.New);
        await Assert.That(lead.RejectionReason).IsNull();
        await Assert.That(lead.DomainEvents.Count).IsEqualTo(0);
    }
}
