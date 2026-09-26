using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SDC.CRM.Mobile.Infrastructure.Auth;
using SDC.CRM.Mobile.Tests.TestDoubles;

namespace SDC.CRM.Mobile.Tests.Infrastructure.Auth;

/// <summary>
/// Session rules of the mobile client: tokens are stored after sign-in, refreshed shortly before they
/// expire, forgotten when refresh fails, and sign-out ends the identity provider session as well.
/// </summary>
public sealed class OidcAuthServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly IOidcSessionClient _sessionClient = Substitute.For<IOidcSessionClient>();
    private readonly FixedTimeProvider _clock = new(Now);

    private OidcAuthService CreateService(InMemoryTokenStorage storage)
        => new(storage, _sessionClient, _clock, NullLogger<OidcAuthService>.Instance);

    private static TokenSet StoredTokens(DateTimeOffset expiresAt, string? identityToken = "id-token-1")
        => new("access-token-1", "refresh-token-1", expiresAt, identityToken);

    [Test]
    public async Task LoginAsync__When_sign_in_succeeds__Should_store_access_refresh_and_identity_tokens()
    {
        var storage = new InMemoryTokenStorage();
        _sessionClient.LoginAsync(Arg.Any<CancellationToken>()).Returns(OidcSessionResult.Success(
            new OidcTokens("access-token-1", "refresh-token-1", "id-token-1", Now.AddMinutes(5))));

        var result = await CreateService(storage).LoginAsync();

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(storage.Tokens).IsEqualTo(new TokenSet("access-token-1", "refresh-token-1", Now.AddMinutes(5), "id-token-1"));
    }

    [Test]
    public async Task LoginAsync__When_identity_provider_returns_error__Should_fail_and_store_nothing()
    {
        var storage = new InMemoryTokenStorage();
        _sessionClient.LoginAsync(Arg.Any<CancellationToken>()).Returns(OidcSessionResult.Failure("access_denied"));

        var result = await CreateService(storage).LoginAsync();

        await Assert.That(result.Succeeded).IsFalse();
        await Assert.That(result.Error).IsEqualTo("access_denied");
        await Assert.That(storage.Tokens).IsNull();
    }

    [Test]
    public async Task GetAccessTokenAsync__When_access_token_is_still_valid__Should_return_it_without_refreshing()
    {
        var storage = InMemoryTokenStorage.With(StoredTokens(expiresAt: Now.AddMinutes(10)));

        var accessToken = await CreateService(storage).GetAccessTokenAsync();

        await Assert.That(accessToken).IsEqualTo("access-token-1");
        await _sessionClient.DidNotReceive().RefreshAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetAccessTokenAsync__When_access_token_expires_within_a_minute__Should_refresh_and_keep_identity_token()
    {
        var storage = InMemoryTokenStorage.With(StoredTokens(expiresAt: Now.AddSeconds(30)));
        _sessionClient.RefreshAsync("refresh-token-1", Arg.Any<CancellationToken>()).Returns(OidcSessionResult.Success(
            new OidcTokens("access-token-2", "refresh-token-2", IdentityToken: null, Now.AddMinutes(5))));

        var accessToken = await CreateService(storage).GetAccessTokenAsync();

        await Assert.That(accessToken).IsEqualTo("access-token-2");
        await Assert.That(storage.Tokens).IsEqualTo(new TokenSet("access-token-2", "refresh-token-2", Now.AddMinutes(5), "id-token-1"));
    }

    [Test]
    public async Task GetAccessTokenAsync__When_refresh_is_rejected__Should_forget_session_and_return_null()
    {
        var storage = InMemoryTokenStorage.With(StoredTokens(expiresAt: Now.AddMinutes(-1)));
        _sessionClient.RefreshAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(OidcSessionResult.Failure("invalid_grant"));

        var accessToken = await CreateService(storage).GetAccessTokenAsync();

        await Assert.That(accessToken).IsNull();
        await Assert.That(storage.Tokens).IsNull();
    }

    [Test]
    public async Task LogoutAsync__When_session_has_identity_token__Should_clear_local_tokens_and_end_sso_session_with_id_token_hint()
    {
        var storage = InMemoryTokenStorage.With(StoredTokens(expiresAt: Now.AddMinutes(10)));
        _sessionClient.EndSessionAsync("id-token-1", Arg.Any<CancellationToken>()).Returns(OidcLogoutResult.Success());

        await CreateService(storage).LogoutAsync();

        await Assert.That(storage.Tokens).IsNull();
        await _sessionClient.Received(1).EndSessionAsync("id-token-1", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task LogoutAsync__When_ending_sso_session_fails__Should_still_clear_local_tokens_without_throwing()
    {
        var storage = InMemoryTokenStorage.With(StoredTokens(expiresAt: Now.AddMinutes(10)));
        _sessionClient.EndSessionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("identity provider unreachable"));

        await CreateService(storage).LogoutAsync();

        await Assert.That(storage.Tokens).IsNull();
    }

    [Test]
    public async Task LogoutAsync__When_session_has_no_identity_token__Should_only_clear_local_tokens()
    {
        var storage = InMemoryTokenStorage.With(StoredTokens(expiresAt: Now.AddMinutes(10), identityToken: null));

        await CreateService(storage).LogoutAsync();

        await Assert.That(storage.Tokens).IsNull();
        await _sessionClient.DidNotReceive().EndSessionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ClearSessionAsync__When_called__Should_forget_local_tokens_without_contacting_identity_provider()
    {
        var storage = InMemoryTokenStorage.With(StoredTokens(expiresAt: Now.AddMinutes(10)));

        await CreateService(storage).ClearSessionAsync();

        await Assert.That(storage.Tokens).IsNull();
        await _sessionClient.DidNotReceive().EndSessionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetUserAsync__When_access_token_carries_name_and_roles__Should_expose_them()
    {
        var accessToken = UnsignedJwt("""{ "sub": "user-1", "name": "Jan Handlowiec", "role": ["Salesperson", "Admin"] }""");
        var storage = InMemoryTokenStorage.With(new TokenSet(accessToken, "refresh-token-1", Now.AddMinutes(10), "id-token-1"));

        var user = await CreateService(storage).GetUserAsync();

        await Assert.That(user!.UserName).IsEqualTo("Jan Handlowiec");
        await Assert.That(user.IsInRole("Salesperson")).IsTrue();
        await Assert.That(user.IsInRole("Admin")).IsTrue();
        await Assert.That(user.IsInRole("BackofficeUser")).IsFalse();
    }

    // Header and payload only - the client reads claims for display; the API validates signatures.
    private static string UnsignedJwt(string payloadJson)
    {
        static string Base64Url(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return $"{Base64Url("""{ "alg": "none" }""")}.{Base64Url(payloadJson)}.";
    }
}

