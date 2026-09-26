namespace SDC.CRM.Mobile.Infrastructure.Auth;

/// <summary>
/// Abstrakcja do bezpiecznego przechowywania tokenów uwierzytelniających.
/// </summary>
public interface ITokenStorage
{
    Task SaveAsync(TokenSet tokens);
    Task<TokenSet?> GetAsync();
    Task ClearAsync();
}

/// <summary>
/// Zestaw tokenów uwierzytelniających. <paramref name="IdentityToken"/> jest potrzebny do wylogowania
/// z dostawcy tożsamości (id_token_hint w żądaniu end_session).
/// </summary>
public sealed record TokenSet(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, string? IdentityToken = null);

