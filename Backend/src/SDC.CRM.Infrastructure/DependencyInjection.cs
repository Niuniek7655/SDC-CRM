using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDC.CRM.Application.Abstractions;
using SDC.CRM.Application.Abstractions.Persistence;
using SDC.CRM.Infrastructure.Persistence;

namespace SDC.CRM.Infrastructure;

public static class DependencyInjection
{
    private const string CrmConnectionStringName = "Crm";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // No fallback on purpose: a missing connection string must stop the application
        // instead of silently pointing it at a local development database.
        var connectionString = configuration.GetConnectionString(CrmConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{CrmConnectionStringName}' is not configured. " +
                $"Set 'ConnectionStrings:{CrmConnectionStringName}' in appsettings.Development.json for local development " +
                $"or the environment variable 'ConnectionStrings__{CrmConnectionStringName}' in other environments.");
        }

        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
