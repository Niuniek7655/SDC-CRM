using Microsoft.Extensions.Logging;
using SDC.CRM.Mobile.Infrastructure.Api;
using SDC.CRM.Mobile.Infrastructure.Auth;
using SDC.CRM.Mobile.Infrastructure.Configuration;
using SDC.CRM.Mobile.Infrastructure.Connectivity;
using SDC.CRM.Mobile.Presentation.Navigation;
using SDC.CRM.Mobile.Presentation.ViewModels;
using SDC.CRM.Mobile.Presentation.Views;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace SDC.CRM.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Infrastructure services
        builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

        // Authentication (OIDC via system browser)
        builder.Services.AddSingleton<IBrowser, WebAuthenticatorBrowser>();
        builder.Services.AddSingleton<IAuthService, OidcAuthService>();
        builder.Services.AddTransient<AuthHeaderHandler>();

        // Typed API client with bearer-token attachment
        builder.Services
            .AddHttpClient<ICrmApiClient, CrmApiClient>(client =>
                client.BaseAddress = new Uri(AppConfig.ApiBaseUrl))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        // Pages and ViewModels
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MainPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
