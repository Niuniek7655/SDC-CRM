namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// Implementacja przechowywania tokenów z wykorzystaniem SecureStorage.
/// </summary>
public sealed class SecureTokenStorage : ITokenStorage
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string ExpiresAtKey = "token_expires_at";

    public async Task SaveAsync(TokenSet tokens)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, tokens.AccessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, tokens.RefreshToken);
        await SecureStorage.Default.SetAsync(ExpiresAtKey, tokens.ExpiresAt.ToString("O"));
    }

    public async Task<TokenSet?> GetAsync()
    {
        var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
        var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        var expiresAtString = await SecureStorage.Default.GetAsync(ExpiresAtKey);

        if (string.IsNullOrEmpty(accessToken) || 
            string.IsNullOrEmpty(refreshToken) || 
            string.IsNullOrEmpty(expiresAtString))
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(expiresAtString, out var expiresAt))
        {
            return null;
        }

        return new TokenSet(accessToken, refreshToken, expiresAt);
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(ExpiresAtKey);
        return Task.CompletedTask;
    }
}

