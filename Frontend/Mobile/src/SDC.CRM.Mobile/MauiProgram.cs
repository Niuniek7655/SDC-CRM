using Microsoft.Extensions.Logging;
using SDC.CRM.Mobile.Infrastructure.Auth;
using SDC.CRM.Mobile.Infrastructure.Connectivity;
using SDC.CRM.Mobile.Presentation.Navigation;
using SDC.CRM.Mobile.Presentation.ViewModels;

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

        // Pages and ViewModels
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MainPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
