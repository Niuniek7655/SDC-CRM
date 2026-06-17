namespace SDC.CRM.Application.Abstractions;

/// <summary>
/// Commits all pending changes made within a single business operation.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
