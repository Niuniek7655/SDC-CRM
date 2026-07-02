namespace SDC.CRM.Mobile.Infrastructure.Api.Contracts;

/// <summary>Read model returned by the backend lead lists.</summary>
public sealed record LeadSummaryDto(
    Guid Id,
    string CompanyName,
    string ContactName,
    string ContactEmail,
    string Status,
    DateTimeOffset CreatedAtUtc);

/// <summary>
/// Register-lead payload. The owning salesperson is derived from the
/// authenticated identity server-side, so it is not part of the request.
/// </summary>
public sealed record RegisterLeadRequest(
    string CompanyName,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    string? Source);

public sealed record RegisterLeadResponse(Guid Id);
