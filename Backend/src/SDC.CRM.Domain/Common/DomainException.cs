namespace SDC.CRM.Domain.Common;

/// <summary>
/// Raised when a business rule (invariant) is violated.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
