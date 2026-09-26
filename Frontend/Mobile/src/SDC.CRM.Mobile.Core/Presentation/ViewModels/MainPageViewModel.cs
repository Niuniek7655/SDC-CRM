using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDC.CRM.Mobile.Infrastructure.Api;
using SDC.CRM.Mobile.Infrastructure.Api.Contracts;
using SDC.CRM.Mobile.Infrastructure.Auth;
using SDC.CRM.Mobile.Infrastructure.Connectivity;
using SDC.CRM.Mobile.Presentation.Navigation;

namespace SDC.CRM.Mobile.Presentation.ViewModels;

/// <summary>
/// Home screen: greets the authenticated user, lists their leads and supports
/// logout. Backend access requires a valid bearer token attached by the API
/// client's auth handler.
/// </summary>
public partial class MainPageViewModel : BaseViewModel
{
    private readonly IConnectivityService _connectivityService;
    private readonly IAuthService _authService;
    private readonly ICrmApiClient _apiClient;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private string _welcomeMessage = "Witaj w SDC CRM Mobile!";

    [ObservableProperty]
    private bool _isOnline;

    public ObservableCollection<LeadSummaryDto> Leads { get; } = [];

    public MainPageViewModel(
        IConnectivityService connectivityService,
        IAuthService authService,
        ICrmApiClient apiClient,
        INavigationService navigation)
    {
        _connectivityService = connectivityService;
        _authService = authService;
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "SDC CRM";

        IsOnline = _connectivityService.IsConnected;
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, bool isConnected)
    {
        IsOnline = isConnected;
    }

    /// <summary>Loads the current user and their leads when the page appears.</summary>
    [RelayCommand]
    private async Task AppearingAsync(CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserAsync(cancellationToken);
        if (user is null)
        {
            await _navigation.GoToAsync("//login");
            return;
        }

        WelcomeMessage = string.IsNullOrWhiteSpace(user.UserName)
            ? "Witaj w SDC CRM Mobile!"
            : $"Witaj, {user.UserName}!";

        await LoadLeadsAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await LoadLeadsAsync(cancellationToken);
    }

    private async Task LoadLeadsAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            if (!_connectivityService.IsConnected)
            {
                SetError("Brak połączenia z siecią. Sprawdź swoje połączenie internetowe.");
                return;
            }

            var leads = await _apiClient.GetMyLeadsAsync(cancellationToken);

            Leads.Clear();
            foreach (var lead in leads)
            {
                Leads.Add(lead);
            }
        }
        catch (CrmUnauthorizedException)
        {
            await _authService.LogoutAsync(cancellationToken);
            await _navigation.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            SetError($"Nie udało się pobrać leadów: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        Leads.Clear();
        await _navigation.GoToAsync("//login");
    }
}
