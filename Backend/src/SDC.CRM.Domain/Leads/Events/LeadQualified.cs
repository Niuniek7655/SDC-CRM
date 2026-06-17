using SDC.CRM.Domain.Common;

namespace SDC.CRM.Domain.Leads.Events;

public sealed record LeadQualified(Guid LeadId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
