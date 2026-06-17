using Microsoft.EntityFrameworkCore;
using SDC.CRM.Application.Abstractions.Persistence;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Infrastructure.Persistence;

public sealed class LeadRepository(CrmDbContext dbContext) : ILeadRepository
{
    public async Task AddAsync(Lead lead, CancellationToken cancellationToken = default)
        => await dbContext.Leads.AddAsync(lead, cancellationToken);

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Leads.FirstOrDefaultAsync(lead => lead.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Lead>> ListBySalespersonAsync(
        Guid salespersonId,
        CancellationToken cancellationToken = default)
        => await dbContext.Leads
            .Where(lead => lead.AssignedSalespersonId == salespersonId)
            .OrderByDescending(lead => lead.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}
