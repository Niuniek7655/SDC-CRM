namespace SDC.CRM.Domain.Common;

/// <summary>
/// A meaningful business fact that has already happened inside an aggregate.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
