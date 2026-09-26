using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SDC.CRM.Mobile.Infrastructure.Api.Contracts;

namespace SDC.CRM.Mobile.Infrastructure.Api;

/// <summary>
/// Default <see cref="ICrmApiClient"/> over <see cref="HttpClient"/>. The bearer
/// token is injected by <see cref="AuthHeaderHandler"/>, so this client focuses
/// on serialization and HTTP status handling.
/// </summary>
public sealed class CrmApiClient(HttpClient httpClient) : ICrmApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<LeadSummaryDto>> GetMyLeadsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/leads/mine", cancellationToken);
        EnsureAuthorized(response);
        response.EnsureSuccessStatusCode();

        var leads = await response.Content.ReadFromJsonAsync<List<LeadSummaryDto>>(JsonOptions, cancellationToken);
        return leads ?? [];
    }

    public async Task<RegisterLeadResponse> RegisterLeadAsync(
        RegisterLeadRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/leads", request, JsonOptions, cancellationToken);
        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new CrmApiException($"Rejestracja leada nie powiodła się ({(int)response.StatusCode}).", detail);
        }

        var result = await response.Content.ReadFromJsonAsync<RegisterLeadResponse>(JsonOptions, cancellationToken);
        return result ?? throw new CrmApiException("Serwer nie zwrócił identyfikatora leada.", null);
    }

    private static void EnsureAuthorized(HttpResponseMessage response)
    {
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                // Missing or expired session: the user has to sign in again.
                throw new CrmUnauthorizedException();
            case HttpStatusCode.Forbidden:
                // Valid session, but the user's role is not allowed to perform the operation.
                throw new CrmForbiddenException();
        }
    }
}

/// <summary>Raised when the API rejects the session (HTTP 401) - the user must sign in again.</summary>
public sealed class CrmUnauthorizedException() : Exception("Brak autoryzacji. Zaloguj się ponownie.");

/// <summary>
/// Raised when the user is signed in but their role does not allow the operation (HTTP 403).
/// The session stays valid, so this must not sign the user out.
/// </summary>
public sealed class CrmForbiddenException() : Exception("Brak uprawnień do wykonania tej operacji.");

/// <summary>Raised for non-success API responses that carry error details.</summary>
public sealed class CrmApiException(string message, string? detail) : Exception(message)
{
    public string? Detail { get; } = detail;
}
