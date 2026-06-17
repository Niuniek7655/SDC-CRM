namespace SDC.CRM.Mobile.Presentation.Navigation;

/// <summary>
/// Implementacja nawigacji Shell.
/// </summary>
public sealed class ShellNavigationService : INavigationService
{
    public async Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (parameters is not null)
        {
            await Shell.Current.GoToAsync(route, parameters);
        }
        else
        {
            await Shell.Current.GoToAsync(route);
        }
    }

    public Task GoBackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
}

