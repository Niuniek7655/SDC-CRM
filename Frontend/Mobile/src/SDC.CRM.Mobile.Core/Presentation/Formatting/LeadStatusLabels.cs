namespace SDC.CRM.Mobile.Presentation.Formatting;

/// <summary>
/// Polish labels of lead status codes returned by the API, taken from the ubiquitous-language glossary
/// (doc/01, section 6.1). Unknown codes (e.g. a status introduced on the backend first) are shown as-is.
/// </summary>
public static class LeadStatusLabels
{
    private static readonly IReadOnlyDictionary<string, string> Labels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["New"] = "Nowy",
        ["Qualified"] = "Zakwalifikowany",
        ["Rejected"] = "Odrzucony",
    };

    public static string For(string? status)
        => status is null ? string.Empty : Labels.GetValueOrDefault(status, status);
}

