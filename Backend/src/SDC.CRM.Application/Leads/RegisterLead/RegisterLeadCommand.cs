namespace SDC.CRM.Application.Leads.RegisterLead;

/// <summary>Register a new lead with minimal customer and contact data (vertical slice #1).</summary>
public sealed record RegisterLeadCommand(
    string CompanyName,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    string? Source,
    Guid AssignedSalespersonId);
