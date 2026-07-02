using CommunityToolkit.Mvvm.Input;
using SDC.CRM.Mobile.Infrastructure.Auth;
using SDC.CRM.Mobile.Presentation.Navigation;

namespace SDC.CRM.Mobile.Presentation.ViewModels;

/// <summary>
/// Drives the login screen: silently continues an existing session or starts
/// the interactive OIDC sign-in.
/// </summary>
public partial class LoginViewModel(IAuthService authService, INavigationService navigation) : BaseViewModel
{
    /// <summary>If a valid session already exists, skip the login screen.</summary>
    [RelayCommand]
    private async Task AppearingAsync()
    {
        Title = "Logowanie";

        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            if (await authService.IsAuthenticatedAsync())
            {
                await navigation.GoToAsync("//main");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var result = await authService.LoginAsync();
            if (!result.Succeeded)
            {
                SetError(result.Error is null
                    ? "Logowanie nie powiodło się. Spróbuj ponownie."
                    : $"Logowanie nie powiodło się: {result.Error}");
                return;
            }

            await navigation.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            SetError($"Wystąpił błąd logowania: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
