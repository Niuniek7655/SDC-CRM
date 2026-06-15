using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Application.Abstractions.Persistence;

/// <summary>
/// Persistence boundary for the <see cref="Lead"/> aggregate. Implemented in Infrastructure.
/// </summary>
public interface ILeadRepository
{
    Task AddAsync(Lead lead, CancellationToken cancellationToken = default);

    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Lead>> ListBySalespersonAsync(Guid salespersonId, CancellationToken cancellationToken = default);
}
