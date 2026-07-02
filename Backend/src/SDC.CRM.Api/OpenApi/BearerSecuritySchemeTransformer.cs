using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SDC.CRM.Api.OpenApi;

/// <summary>
/// Advertises the OAuth2 bearer scheme in the generated OpenAPI document so the
/// UI shows an Authorize button and sends the access token. Uses the
/// Microsoft.OpenApi 2.0 component/reference APIs shipped with .NET 10.
/// </summary>
public sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    private const string SchemeId = "Bearer";

    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (!schemes.Any(scheme => scheme.Name == SchemeId))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.AddComponent(SchemeId, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the access token issued by SimpleIdServer.",
        });

        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeId, document)] = [],
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations.Values))
        {
            operation.Security ??= [];
            operation.Security.Add(requirement);
        }
    }
}
