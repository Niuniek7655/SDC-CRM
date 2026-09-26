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
/// Zestaw tokenów uwierzytelniających.
/// </summary>
public sealed record TokenSet(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

