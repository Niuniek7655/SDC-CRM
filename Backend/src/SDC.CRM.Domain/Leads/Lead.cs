using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads.Events;

namespace SDC.CRM.Domain.Leads;

/// <summary>
/// Lead aggregate root. Captures minimal customer and contact data and the
/// qualification workflow: register -> qualify (into an opportunity) or reject.
/// </summary>
public sealed class Lead : Entity, IAggregateRoot
{
    public string CompanyName { get; private set; }

    public string ContactName { get; private set; }

    public Email ContactEmail { get; private set; }

    public string? ContactPhone { get; private set; }

    public string? Source { get; private set; }

    public LeadStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    public Guid AssignedSalespersonId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    // Required by EF Core.
    private Lead()
    {
        CompanyName = null!;
        ContactName = null!;
        ContactEmail = null!;
    }

    private Lead(
        string companyName,
        string contactName,
        Email contactEmail,
        string? contactPhone,
        string? source,
        Guid assignedSalespersonId)
    {
        CompanyName = companyName;
        ContactName = contactName;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        Source = source;
        AssignedSalespersonId = assignedSalespersonId;
        Status = LeadStatus.New;
        CreatedAtUtc = DateTime.UtcNow;

        Raise(new LeadRegistered(Id, CompanyName, AssignedSalespersonId));
    }

    public static Lead Register(
        string companyName,
        string contactName,
        Email contactEmail,
        string? contactPhone,
        string? source,
        Guid assignedSalespersonId)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new DomainException("A lead must have a company name.");
        }

        if (string.IsNullOrWhiteSpace(contactName))
        {
            throw new DomainException("A lead must have a contact name.");
        }

        if (assignedSalespersonId == Guid.Empty)
        {
            throw new DomainException("A lead must be assigned to a salesperson.");
        }

        return new Lead(
            companyName.Trim(),
            contactName.Trim(),
            contactEmail,
            string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim(),
            string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
            assignedSalespersonId);
    }

    /// <summary>Qualifies the lead so it can be turned into an opportunity.</summary>
    public void Qualify()
    {
        if (Status == LeadStatus.Rejected)
        {
            throw new DomainException("A rejected lead cannot be qualified.");
        }

        if (Status == LeadStatus.Qualified)
        {
            return;
        }

        Status = LeadStatus.Qualified;
        Raise(new LeadQualified(Id));
    }

    /// <summary>Rejects the lead. A rejection reason is mandatory.</summary>
    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("A rejected lead must have a rejection reason.");
        }

        Status = LeadStatus.Rejected;
        RejectionReason = reason.Trim();
        Raise(new LeadRejected(Id, RejectionReason));
    }
}
