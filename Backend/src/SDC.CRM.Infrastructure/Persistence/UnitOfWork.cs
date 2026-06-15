using SDC.CRM.Application.Abstractions;

namespace SDC.CRM.Infrastructure.Persistence;

public sealed class UnitOfWork(CrmDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
