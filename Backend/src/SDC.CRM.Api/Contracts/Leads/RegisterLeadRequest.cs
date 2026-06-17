namespace SDC.CRM.Api.Contracts.Leads;

/// <summary>API request contract for registering a lead. Kept separate from domain entities.</summary>
public sealed record RegisterLeadRequest(
    string CompanyName,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    string? Source,
    Guid AssignedSalespersonId);
