using SDC.CRM.Mobile.Presentation.ViewModels;

namespace SDC.CRM.Mobile;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
