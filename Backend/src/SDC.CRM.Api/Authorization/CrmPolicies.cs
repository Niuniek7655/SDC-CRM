namespace SDC.CRM.Api.Authorization;

/// <summary>
/// Named authorization policies enforced on the API. Policies map the MVP
/// permission matrix (<c>doc/02-uzytkownicy-role-uprawnienia.md</c>) to the
/// CRM roles carried by the access token.
/// </summary>
public static class CrmPolicies
{
    /// <summary>Register a new lead. Salesperson, SalesManager, Admin.</summary>
    public const string RegisterLead = "leads:register";

    /// <summary>View the caller's own pipeline of leads. Sales roles + Admin.</summary>
    public const string ViewOwnLeads = "leads:view-own";
}
