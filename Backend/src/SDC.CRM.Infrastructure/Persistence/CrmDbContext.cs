using Microsoft.EntityFrameworkCore;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Infrastructure.Persistence;

public sealed class CrmDbContext(DbContextOptions<CrmDbContext> options) : DbContext(options)
{
    public DbSet<Lead> Leads => Set<Lead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
    }
}
