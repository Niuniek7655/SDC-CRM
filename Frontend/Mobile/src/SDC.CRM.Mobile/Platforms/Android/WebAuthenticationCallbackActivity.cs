using Android.App;
using Android.Content;
using Android.Content.PM;

namespace SDC.CRM.Mobile;

/// <summary>
/// Receives the OIDC redirect on the custom scheme (com.sdc.crm.mobile://callback)
/// and hands it back to <see cref="WebAuthenticator"/> to complete the login.
/// </summary>
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "com.sdc.crm.mobile")]
public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}
