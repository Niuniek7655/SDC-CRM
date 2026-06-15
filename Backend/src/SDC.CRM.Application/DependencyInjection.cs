using Microsoft.Extensions.DependencyInjection;
using SDC.CRM.Application.Leads.GetMyLeads;
using SDC.CRM.Application.Leads.RegisterLead;

namespace SDC.CRM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterLeadHandler>();
        services.AddScoped<GetMyLeadsHandler>();

        return services;
    }
}
