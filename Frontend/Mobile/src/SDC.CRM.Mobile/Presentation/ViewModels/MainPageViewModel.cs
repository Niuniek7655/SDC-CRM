using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDC.CRM.Mobile.Infrastructure.Connectivity;

namespace SDC.CRM.Mobile.Presentation.ViewModels;

/// <summary>
/// ViewModel dla głównej strony aplikacji.
/// </summary>
public partial class MainPageViewModel : BaseViewModel
{
    private readonly IConnectivityService _connectivityService;

    [ObservableProperty]
    private string _welcomeMessage = "Witaj w SDC CRM Mobile!";

    [ObservableProperty]
    private bool _isOnline;

    public MainPageViewModel(IConnectivityService connectivityService)
    {
        _connectivityService = connectivityService;
        Title = "SDC CRM";
        
        IsOnline = _connectivityService.IsConnected;
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, bool isConnected)
    {
        IsOnline = isConnected;
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            if (!_connectivityService.IsConnected)
            {
                SetError("Brak połączenia z siecią. Sprawdź swoje połączenie internetowe.");
                return;
            }

            // TODO: Implement refresh logic
            await Task.Delay(500, cancellationToken);
        }
        catch (Exception ex)
        {
            SetError($"Wystąpił błąd: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

