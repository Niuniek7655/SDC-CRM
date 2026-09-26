using System.Globalization;

namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// Implementacja przechowywania tokenów z wykorzystaniem SecureStorage.
/// </summary>
public sealed class SecureTokenStorage : ITokenStorage
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string ExpiresAtKey = "token_expires_at";
    private const string IdentityTokenKey = "id_token";

    public async Task SaveAsync(TokenSet tokens)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, tokens.AccessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, tokens.RefreshToken);
        await SecureStorage.Default.SetAsync(ExpiresAtKey, tokens.ExpiresAt.ToString("O", CultureInfo.InvariantCulture));

        // Needed as id_token_hint when ending the identity provider session on logout.
        if (string.IsNullOrEmpty(tokens.IdentityToken))
        {
            SecureStorage.Default.Remove(IdentityTokenKey);
        }
        else
        {
            await SecureStorage.Default.SetAsync(IdentityTokenKey, tokens.IdentityToken);
        }
    }

    public async Task<TokenSet?> GetAsync()
    {
        var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
        var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        var expiresAtString = await SecureStorage.Default.GetAsync(ExpiresAtKey);
        var identityToken = await SecureStorage.Default.GetAsync(IdentityTokenKey);

        if (string.IsNullOrEmpty(accessToken) || 
            string.IsNullOrEmpty(refreshToken) || 
            string.IsNullOrEmpty(expiresAtString))
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(expiresAtString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAt))
        {
            return null;
        }

        return new TokenSet(accessToken, refreshToken, expiresAt, identityToken);
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(ExpiresAtKey);
        SecureStorage.Default.Remove(IdentityTokenKey);
        return Task.CompletedTask;
    }
}

