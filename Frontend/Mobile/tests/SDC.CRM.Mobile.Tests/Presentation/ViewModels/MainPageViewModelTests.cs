using NSubstitute;
using SDC.CRM.Mobile.Infrastructure.Api;
using SDC.CRM.Mobile.Infrastructure.Api.Contracts;
using SDC.CRM.Mobile.Infrastructure.Auth;
using SDC.CRM.Mobile.Infrastructure.Connectivity;
using SDC.CRM.Mobile.Presentation.Navigation;
using SDC.CRM.Mobile.Presentation.ViewModels;

namespace SDC.CRM.Mobile.Tests.Presentation.ViewModels;

public sealed class MainPageViewModelTests
{
    private readonly IConnectivityService _connectivity = Substitute.For<IConnectivityService>();
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly ICrmApiClient _apiClient = Substitute.For<ICrmApiClient>();
    private readonly INavigationService _navigation = Substitute.For<INavigationService>();

    private static readonly LeadSummaryDto Lead = new(
        Guid.Parse("3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77"),
        "Acme Sp. z o.o.",
        "Jan Kowalski",
        "jan.kowalski@acme.test",
        "New",
        new DateTimeOffset(2026, 9, 26, 10, 0, 0, TimeSpan.Zero));

    private MainPageViewModel CreateViewModel() => new(_connectivity, _authService, _apiClient, _navigation);

    private void GivenSignedInSalespersonOnline()
    {
        _connectivity.IsConnected.Returns(true);
        _authService.GetUserAsync(Arg.Any<CancellationToken>()).Returns(new AuthUser("Jan Handlowiec", ["Salesperson"]));
        _apiClient.GetMyLeadsAsync(Arg.Any<CancellationToken>()).Returns([Lead]);
    }

    [Test]
    public async Task Appearing__When_user_is_not_signed_in__Should_navigate_to_login_without_calling_api()
    {
        _authService.GetUserAsync(Arg.Any<CancellationToken>()).Returns((AuthUser?)null);
        var viewModel = CreateViewModel();

        await viewModel.AppearingCommand.ExecuteAsync(null);

        await _navigation.Received(1).GoToAsync("//login", Arg.Any<IDictionary<string, object>?>());
        await _apiClient.DidNotReceive().GetMyLeadsAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Appearing__When_user_is_signed_in__Should_greet_user_and_show_leads()
    {
        GivenSignedInSalespersonOnline();
        var viewModel = CreateViewModel();

        await viewModel.AppearingCommand.ExecuteAsync(null);

        await Assert.That(viewModel.WelcomeMessage).IsEqualTo("Witaj, Jan Handlowiec!");
        await Assert.That(viewModel.Leads.Count).IsEqualTo(1);
        await Assert.That(viewModel.Leads[0].CompanyName).IsEqualTo("Acme Sp. z o.o.");
        await Assert.That(viewModel.HasError).IsFalse();
    }

    [Test]
    public async Task Refresh__When_device_is_offline__Should_show_offline_message_without_calling_api()
    {
        _connectivity.IsConnected.Returns(false);
        var viewModel = CreateViewModel();

        await viewModel.RefreshCommand.ExecuteAsync(null);

        await Assert.That(viewModel.HasError).IsTrue();
        await Assert.That(viewModel.ErrorMessage).Contains("Brak połączenia");
        await _apiClient.DidNotReceive().GetMyLeadsAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Refresh__When_api_rejects_session_with_401__Should_log_out_and_navigate_to_login()
    {
        _connectivity.IsConnected.Returns(true);
        _apiClient.GetMyLeadsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<LeadSummaryDto>>(new CrmUnauthorizedException()));
        var viewModel = CreateViewModel();

        await viewModel.RefreshCommand.ExecuteAsync(null);

        await _authService.Received(1).LogoutAsync(Arg.Any<CancellationToken>());
        await _navigation.Received(1).GoToAsync("//login", Arg.Any<IDictionary<string, object>?>());
    }

    [Test]
    public async Task Logout__When_user_logs_out__Should_clear_leads_and_navigate_to_login()
    {
        GivenSignedInSalespersonOnline();
        var viewModel = CreateViewModel();
        await viewModel.AppearingCommand.ExecuteAsync(null);

        await viewModel.LogoutCommand.ExecuteAsync(null);

        await _authService.Received(1).LogoutAsync(Arg.Any<CancellationToken>());
        await Assert.That(viewModel.Leads.Count).IsEqualTo(0);
        await _navigation.Received(1).GoToAsync("//login", Arg.Any<IDictionary<string, object>?>());
    }
}

