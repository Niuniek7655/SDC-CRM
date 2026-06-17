namespace SDC.CRM.Mobile.Infrastructure.Connectivity;

/// <summary>
/// Abstrakcja do sprawdzania stanu połączenia sieciowego.
/// </summary>
public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<bool> ConnectivityChanged;
}

