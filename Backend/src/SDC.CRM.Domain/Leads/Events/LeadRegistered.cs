using SDC.CRM.Domain.Common;

namespace SDC.CRM.Domain.Leads.Events;

public sealed record LeadRegistered(Guid LeadId, string CompanyName, Guid AssignedSalespersonId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
