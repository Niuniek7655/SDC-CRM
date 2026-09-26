namespace SDC.CRM.Mobile.Presentation.Navigation;

/// <summary>
/// Abstrakcja nawigacji w aplikacji.
/// </summary>
public interface INavigationService
{
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoBackAsync();
}

