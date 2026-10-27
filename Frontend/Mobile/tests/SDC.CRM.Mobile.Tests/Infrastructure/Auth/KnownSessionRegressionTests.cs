using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SDC.CRM.Mobile.Infrastructure.Api;
using SDC.CRM.Mobile.Infrastructure.Api.Contracts;
using SDC.CRM.Mobile.Infrastructure.Auth;
using SDC.CRM.Mobile.Infrastructure.Connectivity;
using SDC.CRM.Mobile.Presentation.Navigation;
using SDC.CRM.Mobile.Presentation.ViewModels;
using SDC.CRM.Mobile.Tests.TestDoubles;

namespace SDC.CRM.Mobile.Tests.Infrastructure.Auth;

// Desired invariants for confirmed bugs. Explicitly select this class to run the red regressions.
// See doc/testy/05-pokrycie-i-regresje.md; these are not counted as passing default-suite coverage.
[Explicit]
[Category("KnownRegression")]
public sealed class KnownSessionRegressionTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task Logout__When_old_refresh_completes_later__Should_not_restore_tokens()
    {
        var storage = InMemoryTokenStorage.With(new("old-access", "old-refresh", Now.AddMinutes(-1), "old-id"));
        var session = Substitute.For<IOidcSessionClient>();
        var pending = new TaskCompletionSource<OidcSessionResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        session.RefreshAsync("old-refresh", Arg.Any<CancellationToken>()).Returns(pending.Task);
        session.EndSessionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(OidcLogoutResult.Success());
        var auth = new OidcAuthService(storage, session, new FixedTimeProvider(Now), NullLogger<OidcAuthService>.Instance);
        var refreshing = auth.GetAccessTokenAsync();
        await session.Received(1).RefreshAsync("old-refresh", Arg.Any<CancellationToken>());

        await auth.LogoutAsync();
        pending.SetResult(OidcSessionResult.Success(new("late-access", "late-refresh", "late-id", Now.AddMinutes(10))));
        await refreshing;

        await Assert.That(storage.Tokens).IsNull();
    }

    [Test]
    public async Task Logout__When_old_list_response_completes_later__Should_not_restore_leads()
    {
        var connectivity = Substitute.For<IConnectivityService>();
        connectivity.IsConnected.Returns(true);
        var api = Substitute.For<ICrmApiClient>();
        var pending = new TaskCompletionSource<IReadOnlyList<LeadSummaryDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        api.GetMyLeadsAsync(Arg.Any<CancellationToken>()).Returns(pending.Task);
        var vm = new MainPageViewModel(connectivity, Substitute.For<IAuthService>(), api, Substitute.For<INavigationService>());
        var loading = vm.RefreshCommand.ExecuteAsync(null);
        await api.Received(1).GetMyLeadsAsync(Arg.Any<CancellationToken>());

        await vm.LogoutCommand.ExecuteAsync(null);
        pending.SetResult([new(Guid.Parse("7d1c1a52-1b8e-4f2e-9a57-5c3f0e6a9b10"), "Old account", "Jan", "jan@example.test", "New", Now)]);
        await loading;

        await Assert.That(vm.Leads.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Refresh__When_refresh_view_sets_busy_before_command__Should_complete_refresh()
    {
        var connectivity = Substitute.For<IConnectivityService>();
        connectivity.IsConnected.Returns(true);
        var api = Substitute.For<ICrmApiClient>();
        api.GetMyLeadsAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<LeadSummaryDto>());
        var vm = new MainPageViewModel(connectivity, Substitute.For<IAuthService>(), api, Substitute.For<INavigationService>()) { IsBusy = true };

        await vm.RefreshCommand.ExecuteAsync(null);

        await api.Received(1).GetMyLeadsAsync(Arg.Any<CancellationToken>());
        await Assert.That(vm.IsBusy).IsFalse();
    }

    [Test]
    public async Task GetAccessToken__When_two_calls_need_refresh__Should_share_one_refresh_operation()
    {
        var storage = InMemoryTokenStorage.With(new("old", "refresh", Now.AddMinutes(-1), "id"));
        var session = Substitute.For<IOidcSessionClient>();
        var pending = new TaskCompletionSource<OidcSessionResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        session.RefreshAsync("refresh", Arg.Any<CancellationToken>()).Returns(pending.Task);
        var auth = new OidcAuthService(storage, session, new FixedTimeProvider(Now), NullLogger<OidcAuthService>.Instance);

        var first = auth.GetAccessTokenAsync();
        var second = auth.GetAccessTokenAsync();
        pending.SetResult(OidcSessionResult.Success(new("new", "rotated", "id", Now.AddMinutes(10))));
        await Task.WhenAll(first, second);

        await session.Received(1).RefreshAsync("refresh", Arg.Any<CancellationToken>());
        await Assert.That(storage.Tokens!.AccessToken).IsEqualTo("new");
    }
}
