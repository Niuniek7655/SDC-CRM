using SDC.CRM.Mobile.Presentation.ViewModels;

namespace SDC.CRM.Mobile.Presentation.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.AppearingCommand.CanExecute(null))
        {
            _viewModel.AppearingCommand.Execute(null);
        }
    }
}
