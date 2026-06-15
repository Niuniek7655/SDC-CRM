using SDC.CRM.Application.Abstractions;
using SDC.CRM.Application.Abstractions.Persistence;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Application.Tests.Doubles;

/// <summary>Simple in-memory test double standing in for persistence + unit of work.</summary>
public sealed class InMemoryLeadRepository : ILeadRepository, IUnitOfWork
{
    private readonly List<Lead> _leads = [];

    public IReadOnlyList<Lead> Stored => _leads;

    public Task AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        _leads.Add(lead);
        return Task.CompletedTask;
    }

    public Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_leads.FirstOrDefault(lead => lead.Id == id));

    public Task<IReadOnlyList<Lead>> ListBySalespersonAsync(Guid salespersonId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Lead>>(
            _leads.Where(lead => lead.AssignedSalespersonId == salespersonId).ToList());

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_leads.Count);
}
