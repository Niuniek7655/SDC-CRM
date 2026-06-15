namespace SDC.CRM.Domain.Common;

/// <summary>
/// Value object representing a validated e-mail address.
/// </summary>
public sealed record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Contact e-mail is required.");
        }

        var trimmed = value.Trim();

        // Intentionally lightweight: full RFC validation belongs to infrastructure/input validation.
        if (!trimmed.Contains('@') || trimmed.StartsWith('@') || trimmed.EndsWith('@'))
        {
            throw new DomainException($"Contact e-mail '{value}' is not valid.");
        }

        return new Email(trimmed);
    }

    public override string ToString() => Value;
}
