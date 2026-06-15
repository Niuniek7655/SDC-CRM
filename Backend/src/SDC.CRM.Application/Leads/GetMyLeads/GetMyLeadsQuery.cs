namespace SDC.CRM.Application.Leads.GetMyLeads;

/// <summary>Show the leads owned by a given salesperson (vertical slice #2).</summary>
public sealed record GetMyLeadsQuery(Guid SalespersonId);
