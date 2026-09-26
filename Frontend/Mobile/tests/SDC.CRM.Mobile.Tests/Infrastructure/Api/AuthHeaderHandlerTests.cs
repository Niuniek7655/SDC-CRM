using System.Net;
using NSubstitute;
using SDC.CRM.Mobile.Infrastructure.Api;
using SDC.CRM.Mobile.Infrastructure.Auth;
using SDC.CRM.Mobile.Tests.TestDoubles;

namespace SDC.CRM.Mobile.Tests.Infrastructure.Api;

public sealed class AuthHeaderHandlerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();

    private (HttpClient Client, StubHttpMessageHandler Network) CreateClient()
    {
        var network = StubHttpMessageHandler.Returning(HttpStatusCode.OK, "[]");
        var handler = new AuthHeaderHandler(_authService) { InnerHandler = network };
        return (new HttpClient(handler) { BaseAddress = new Uri("http://api.test/") }, network);
    }

    [Test]
    public async Task SendAsync__When_session_provides_access_token__Should_attach_bearer_authorization_header()
    {
        _authService.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("access-token-123");
        var (client, network) = CreateClient();

        await client.GetAsync("api/leads/mine");

        var authorization = network.Requests.Single().Headers.Authorization;
        await Assert.That(authorization!.Scheme).IsEqualTo("Bearer");
        await Assert.That(authorization.Parameter).IsEqualTo("access-token-123");
    }

    [Test]
    public async Task SendAsync__When_there_is_no_session__Should_send_request_without_authorization_header()
    {
        _authService.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns((string?)null);
        var (client, network) = CreateClient();

        await client.GetAsync("api/leads/mine");

        await Assert.That(network.Requests.Single().Headers.Authorization).IsNull();
    }
}

