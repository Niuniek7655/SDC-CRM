namespace SDC.CRM.Mobile.Infrastructure.Configuration;

/// <summary>
/// Central client configuration (identity provider + API endpoints).
///
/// Uses <c>localhost</c> for every platform on purpose: the token issuer
/// advertised by the local SimpleIdServer is <c>http://localhost:5001/master</c>,
/// so the mobile client must also reach it as <c>localhost</c> for issuer
/// validation to succeed. On an Android emulator run
/// <c>adb reverse tcp:5001 tcp:5001</c> and <c>adb reverse tcp:5080 tcp:5080</c>
/// so the device's localhost maps to the host machine.
/// </summary>
public static class AppConfig
{
    public const string Authority = "http://localhost:5001/master";

    public const string ApiBaseUrl = "http://localhost:5080/";

    public const string ClientId = "sdc-crm-mobile";

    public const string Scope = "openid profile email role offline_access sdc-crm-api";

    /// <summary>Custom-scheme redirect handled by the platform callback activity.</summary>
    public const string RedirectUri = "com.sdc.crm.mobile://callback";

    public const string PostLogoutRedirectUri = "com.sdc.crm.mobile://signout";

    /// <summary>Local SSO is served over HTTP, so disable the HTTPS requirement.</summary>
    public const bool RequireHttps = false;
}
