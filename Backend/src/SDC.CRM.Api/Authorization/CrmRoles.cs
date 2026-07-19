namespace SDC.CRM.Api.Authorization;

/// <summary>
/// CRM role names as delivered in the access token <c>role</c> claim by the
/// identity provider. Mirrors the roles defined in
/// <c>doc/02-uzytkownicy-role-uprawnienia.md</c>.
/// </summary>
public static class CrmRoles
{
    public const string Salesperson = "Salesperson";
    public const string SalesManager = "SalesManager";
    public const string BackofficeUser = "BackofficeUser";
    public const string BackofficeManager = "BackofficeManager";
    public const string Admin = "Admin";

    public static readonly string[] All =
    [
        Salesperson,
        SalesManager,
        BackofficeUser,
        BackofficeManager,
        Admin,
    ];
}
