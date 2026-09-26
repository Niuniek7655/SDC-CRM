using NSubstitute;
using SDC.CRM.Mobile.Infrastructure.Auth;
using SDC.CRM.Mobile.Presentation.Navigation;
using SDC.CRM.Mobile.Presentation.ViewModels;

namespace SDC.CRM.Mobile.Tests.Presentation.ViewModels;

public sealed class LoginViewModelTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly INavigationService _navigation = Substitute.For<INavigationService>();

    private LoginViewModel CreateViewModel() => new(_authService, _navigation);

    [Test]
    public async Task Appearing__When_session_already_exists__Should_skip_login_and_open_main_page()
    {
        _authService.IsAuthenticatedAsync(Arg.Any<CancellationToken>()).Returns(true);
        var viewModel = CreateViewModel();

        await viewModel.AppearingCommand.ExecuteAsync(null);

        await _navigation.Received(1).GoToAsync("//main", Arg.Any<IDictionary<string, object>?>());
    }

    [Test]
    public async Task Login__When_sign_in_succeeds__Should_open_main_page()
    {
        _authService.LoginAsync(Arg.Any<CancellationToken>()).Returns(AuthResult.Success());
        var viewModel = CreateViewModel();

        await viewModel.LoginCommand.ExecuteAsync(null);

        await _navigation.Received(1).GoToAsync("//main", Arg.Any<IDictionary<string, object>?>());
        await Assert.That(viewModel.HasError).IsFalse();
    }

    [Test]
    public async Task Login__When_sign_in_fails__Should_show_error_and_stay_on_login_page()
    {
        _authService.LoginAsync(Arg.Any<CancellationToken>()).Returns(AuthResult.Failure("access_denied"));
        var viewModel = CreateViewModel();

        await viewModel.LoginCommand.ExecuteAsync(null);

        await Assert.That(viewModel.HasError).IsTrue();
        await Assert.That(viewModel.ErrorMessage).Contains("access_denied");
        await _navigation.DidNotReceive().GoToAsync(Arg.Any<string>(), Arg.Any<IDictionary<string, object>?>());
    }
}

