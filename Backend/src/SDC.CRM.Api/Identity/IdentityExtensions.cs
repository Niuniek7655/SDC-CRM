using SDC.CRM.Application.Abstractions.Identity;

namespace SDC.CRM.Api.Identity;

public static class IdentityExtensions
{
    /// <summary>Exposes the authenticated caller to use-case handlers.</summary>
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}
