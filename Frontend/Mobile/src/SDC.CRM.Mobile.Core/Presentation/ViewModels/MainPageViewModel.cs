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
    private const string DefaultWelcomeMessage = "Witaj w SDC CRM Mobile!";

    private readonly IConnectivityService _connectivityService;
    private readonly IAuthService _authService;
    private readonly ICrmApiClient _apiClient;
    private readonly INavigationService _navigation;

    private bool _isFollowingConnectivity;

    [ObservableProperty]
    public partial string WelcomeMessage { get; set; }

    [ObservableProperty]
    public partial bool IsOnline { get; set; }

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
        WelcomeMessage = DefaultWelcomeMessage;
        IsOnline = _connectivityService.IsConnected;
    }

    /// <summary>Loads the current user and their leads when the page appears.</summary>
    [RelayCommand]
    private async Task AppearingAsync(CancellationToken cancellationToken)
    {
        StartFollowingConnectivity();

        var user = await _authService.GetUserAsync(cancellationToken);
        if (user is null)
        {
            await _navigation.GoToAsync(AppRoutes.ToLogin);
            return;
        }

        WelcomeMessage = string.IsNullOrWhiteSpace(user.UserName)
            ? DefaultWelcomeMessage
            : $"Witaj, {user.UserName}!";

        await LoadLeadsAsync(cancellationToken);
    }

    /// <summary>
    /// Called when the page is hidden. The connectivity service lives for the whole app, while this
    /// view model is transient - unsubscribing lets it be garbage collected.
    /// </summary>
    [RelayCommand]
    private void Disappearing() => StopFollowingConnectivity();

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
            // Expired/invalid session: forget it locally and sign in again (no SSO end-session round-trip).
            await _authService.ClearSessionAsync(cancellationToken);
            await _navigation.GoToAsync(AppRoutes.ToLogin);
        }
        catch (CrmForbiddenException)
        {
            // Signed in, but the role cannot see leads (e.g. backoffice) - keep the session.
            Leads.Clear();
            SetError("Brak uprawnień do listy leadów dla Twojej roli.");
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
        await _navigation.GoToAsync(AppRoutes.ToLogin);
    }

    private void StartFollowingConnectivity()
    {
        IsOnline = _connectivityService.IsConnected;

        if (_isFollowingConnectivity)
        {
            return;
        }

        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
        _isFollowingConnectivity = true;
    }

    private void StopFollowingConnectivity()
    {
        if (!_isFollowingConnectivity)
        {
            return;
        }

        _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
        _isFollowingConnectivity = false;
    }

    private void OnConnectivityChanged(object? sender, bool isConnected) => IsOnline = isConnected;
}
