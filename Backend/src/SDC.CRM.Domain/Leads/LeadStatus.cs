namespace SDC.CRM.Domain.Leads;

/// <summary>
/// Lifecycle of a lead before it becomes an opportunity.
/// </summary>
public enum LeadStatus
{
    New = 0,
    Qualified = 1,
    Rejected = 2
}
