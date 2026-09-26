using Android.App;
using Android.Content;
using Android.Content.PM;

namespace SDC.CRM.Mobile;

/// <summary>
/// Receives the OIDC redirects on the custom scheme and hands them back to <see cref="WebAuthenticator"/>:
/// com.sdc.crm.mobile://callback (sign-in) and com.sdc.crm.mobile://signout (end-session after logout).
/// The intent filter matches the scheme only, so both hosts are handled.
/// </summary>
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "com.sdc.crm.mobile")]
public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}
