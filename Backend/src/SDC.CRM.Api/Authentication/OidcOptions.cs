namespace SDC.CRM.Api.Authentication;

/// <summary>
/// Bound from the <c>Oidc</c> configuration section. Describes how the API
/// validates access tokens issued by the SimpleIdServer identity provider.
/// </summary>
public sealed class OidcOptions
{
    public const string SectionName = "Oidc";

    /// <summary>Token issuer / OIDC authority, e.g. http://localhost:5001/master.</summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>Expected audience of access tokens (the protected API resource).</summary>
    public string Audience { get; set; } = "sdc-crm-api";

    /// <summary>Allow HTTP metadata retrieval. True only for local development.</summary>
    public bool RequireHttpsMetadata { get; set; }

    /// <summary>Validate the token audience. Disable only for local experiments.</summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>Claim type holding the display name.</summary>
    public string NameClaimType { get; set; } = "name";

    /// <summary>Claim type holding CRM roles.</summary>
    public string RoleClaimType { get; set; } = "role";

    /// <summary>
    /// Browser origins allowed to call the API directly (CORS), e.g. the Angular
    /// dev server. The dev proxy makes this optional, but it is required when the
    /// SPA talks to the API cross-origin.
    /// </summary>
    public string[] AllowedCorsOrigins { get; set; } = [];
}
