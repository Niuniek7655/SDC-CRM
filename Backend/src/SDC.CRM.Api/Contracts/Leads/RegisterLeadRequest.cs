namespace SDC.CRM.Api.Contracts.Leads;

/// <summary>
/// API request contract for registering a lead. The owning salesperson is taken
/// from the authenticated identity, not from the request body.
/// </summary>
public sealed record RegisterLeadRequest(
    string CompanyName,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    string? Source);
