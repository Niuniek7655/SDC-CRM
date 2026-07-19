using SDC.CRM.Mobile.Presentation.ViewModels;

namespace SDC.CRM.Mobile;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;

    public MainPage(MainPageViewModel viewModel)
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
