using SDC.CRM.Application.Abstractions.Persistence;

namespace SDC.CRM.Application.Leads.GetMyLeads;

public sealed class GetMyLeadsHandler(ILeadRepository leads)
{
    public async Task<IReadOnlyList<LeadSummaryDto>> HandleAsync(
        GetMyLeadsQuery query,
        CancellationToken cancellationToken = default)
    {
        var ownedLeads = await leads.ListBySalespersonAsync(query.SalespersonId, cancellationToken);

        return ownedLeads
            .Select(lead => new LeadSummaryDto(
                lead.Id,
                lead.CompanyName,
                lead.ContactName,
                lead.ContactEmail.Value,
                lead.Status.ToString(),
                lead.CreatedAtUtc))
            .ToList();
    }
}
