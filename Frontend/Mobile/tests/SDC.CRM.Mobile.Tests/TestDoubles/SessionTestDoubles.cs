using SDC.CRM.Mobile.Infrastructure.Auth;

namespace SDC.CRM.Mobile.Tests.TestDoubles;

/// <summary>In-memory replacement for the secure token storage (MAUI SecureStorage in the app).</summary>
internal sealed class InMemoryTokenStorage : ITokenStorage
{
    public TokenSet? Tokens { get; private set; }

    public int ClearCount { get; private set; }

    public static InMemoryTokenStorage With(TokenSet tokens)
    {
        var storage = new InMemoryTokenStorage();
        storage.Tokens = tokens;
        return storage;
    }

    public Task SaveAsync(TokenSet tokens)
    {
        Tokens = tokens;
        return Task.CompletedTask;
    }

    public Task<TokenSet?> GetAsync() => Task.FromResult(Tokens);

    public Task ClearAsync()
    {
        Tokens = null;
        ClearCount++;
        return Task.CompletedTask;
    }
}

/// <summary>Controlled clock for token expiry checks (no dependency on the real system time).</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

