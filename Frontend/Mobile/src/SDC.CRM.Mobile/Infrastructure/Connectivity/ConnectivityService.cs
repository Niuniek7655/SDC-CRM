namespace SDC.CRM.Mobile.Infrastructure.Connectivity;

/// <summary>
/// Implementacja serwisu sprawdzającego połączenie sieciowe.
/// </summary>
public sealed class ConnectivityService : IConnectivityService, IDisposable
{
    public bool IsConnected => Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler<bool>? ConnectivityChanged;

    public ConnectivityService()
    {
        Microsoft.Maui.Networking.Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var isConnected = e.NetworkAccess == NetworkAccess.Internet;
        ConnectivityChanged?.Invoke(this, isConnected);
    }

    public void Dispose()
    {
        Microsoft.Maui.Networking.Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
    }
}

