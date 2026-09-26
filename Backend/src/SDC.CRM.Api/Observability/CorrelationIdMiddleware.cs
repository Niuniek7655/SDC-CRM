using System.Diagnostics;

namespace SDC.CRM.Api.Observability;

/// <summary>
/// Assigns a correlation id to every request (<c>X-Correlation-ID</c>). A caller-provided id is reused
/// when it is safe to log and echo (length limit and character whitelist); otherwise a new id is
/// generated - the W3C trace id of the current request activity when available, so the response
/// header, the logs and the distributed trace share one identifier.
/// The id is returned in the response header, added to the logging scope and tagged on the activity.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";
    public const string LogScopeKey = "CorrelationId";
    public const string ActivityTagName = "correlation.id";

    private const int MaxLength = 64;
    private static readonly object ItemsKey = new();

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context.Request.Headers[HeaderName].ToString());

        context.Items[ItemsKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;
        Activity.Current?.SetTag(ActivityTagName, correlationId);

        using (logger.BeginScope(new Dictionary<string, object> { [LogScopeKey] = correlationId }))
        {
            await next(context);
        }
    }

    /// <summary>Correlation id assigned to the current request, or null outside the middleware.</summary>
    public static string? GetCorrelationId(HttpContext context)
        => context.Items.TryGetValue(ItemsKey, out var value) ? value as string : null;

    /// <summary>
    /// Accepts only short ASCII identifiers so a caller cannot inject log entries or response headers.
    /// </summary>
    public static bool IsValid(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= MaxLength
           && value.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or ':');

    private static string ResolveCorrelationId(string incoming)
        => IsValid(incoming) ? incoming : GenerateCorrelationId();

    private static string GenerateCorrelationId()
    {
        var activity = Activity.Current;
        return activity is { IdFormat: ActivityIdFormat.W3C }
            ? activity.TraceId.ToHexString()
            : Guid.NewGuid().ToString("N");
    }
}

