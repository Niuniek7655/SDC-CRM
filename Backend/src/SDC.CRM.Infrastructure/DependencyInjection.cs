using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDC.CRM.Application.Abstractions;
using SDC.CRM.Application.Abstractions.Persistence;
using SDC.CRM.Infrastructure.Persistence;

namespace SDC.CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Crm")
            ?? "Host=localhost;Port=5432;Database=appdb;Username=app;Password=app";

        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
