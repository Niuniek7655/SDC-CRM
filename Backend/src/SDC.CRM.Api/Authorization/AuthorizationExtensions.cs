namespace SDC.CRM.Api.Authorization;

/// <summary>
/// Registers the CRM authorization policies used by controllers/endpoints.
/// </summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddCrmAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Any endpoint decorated with [Authorize] (no policy) requires a valid,
            // authenticated token by default.
            options.AddPolicy(CrmPolicies.RegisterLead, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(CrmRoles.Salesperson, CrmRoles.SalesManager, CrmRoles.Admin));

            options.AddPolicy(CrmPolicies.ViewOwnLeads, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(CrmRoles.Salesperson, CrmRoles.SalesManager, CrmRoles.Admin));
        });

        return services;
    }
}
