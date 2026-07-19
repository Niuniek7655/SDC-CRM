using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SDC.CRM.Api.Authentication;

/// <summary>
/// Configures the API as an OAuth2/OIDC resource server that validates JWT
/// bearer access tokens issued by SimpleIdServer.
/// </summary>
public static class AuthenticationExtensions
{
    public const string CorsPolicyName = "SpaClients";

    public static IServiceCollection AddCrmAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(OidcOptions.SectionName).Get<OidcOptions>()
                      ?? new OidcOptions();

        services.AddSingleton(options);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.Authority = options.Authority;
                jwt.Audience = options.Audience;
                jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                jwt.MapInboundClaims = false;

                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Authority,
                    ValidateAudience = options.ValidateAudience,
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = options.NameClaimType,
                    RoleClaimType = options.RoleClaimType,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                // Observability: log authentication boundaries without ever
                // logging tokens or their contents (only the subject id / reason).
                jwt.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        CreateAuthLogger(context.HttpContext)
                            .LogWarning("JWT authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var subject = context.Principal?.FindFirstValue("sub")
                                      ?? context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        CreateAuthLogger(context.HttpContext)
                            .LogDebug("Access token validated for subject {Subject}", subject);
                        return Task.CompletedTask;
                    },
                };
            });

        AddAuthenticationCors(services, options);

        return services;
    }

    private static ILogger CreateAuthLogger(HttpContext httpContext) =>
        httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SDC.CRM.Api.Authentication");

    private static void AddAuthenticationCors(IServiceCollection services, OidcOptions options)
    {
        services.AddCors(cors => cors.AddPolicy(CorsPolicyName, policy =>
        {
            if (options.AllowedCorsOrigins.Length > 0)
            {
                policy
                    .WithOrigins(options.AllowedCorsOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        }));
    }
}
