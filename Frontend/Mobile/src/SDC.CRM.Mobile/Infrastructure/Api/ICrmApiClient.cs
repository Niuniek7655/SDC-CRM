using SDC.CRM.Mobile.Infrastructure.Api.Contracts;

namespace SDC.CRM.Mobile.Infrastructure.Api;

/// <summary>Typed access to the SDC-CRM backend API.</summary>
public interface ICrmApiClient
{
    Task<IReadOnlyList<LeadSummaryDto>> GetMyLeadsAsync(CancellationToken cancellationToken = default);

    Task<RegisterLeadResponse> RegisterLeadAsync(RegisterLeadRequest request, CancellationToken cancellationToken = default);
}
