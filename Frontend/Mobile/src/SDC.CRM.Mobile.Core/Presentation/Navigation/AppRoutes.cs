namespace SDC.CRM.Mobile.Presentation.Navigation;

/// <summary>
/// Single place for Shell route names (registered in AppShell.xaml via x:Static) and the absolute
/// navigation targets used by view models, so raw route strings are not scattered across the app.
/// </summary>
public static class AppRoutes
{
    /// <summary>Route name of the login page.</summary>
    public const string Login = "login";

    /// <summary>Route name of the main (leads) page.</summary>
    public const string Main = "main";

    /// <summary>Absolute URI to the login page; replaces the navigation stack (after sign-out).</summary>
    public const string ToLogin = "//" + Login;

    /// <summary>Absolute URI to the main page; replaces the navigation stack (after sign-in).</summary>
    public const string ToMain = "//" + Main;
}

