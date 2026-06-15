using SDC.CRM.Domain.Common;

namespace SDC.CRM.Domain.Leads.Events;

public sealed record LeadRejected(Guid LeadId, string Reason) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
