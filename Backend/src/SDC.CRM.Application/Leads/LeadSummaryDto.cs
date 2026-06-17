namespace SDC.CRM.Application.Leads;

/// <summary>Read model for lead lists. Never expose domain/persistence entities directly.</summary>
public sealed record LeadSummaryDto(
    Guid Id,
    string CompanyName,
    string ContactName,
    string ContactEmail,
    string Status,
    DateTime CreatedAtUtc);
