using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SDC.CRM.Api.Observability;

namespace SDC.CRM.Api.Tests.Observability;

/// <summary>
/// Every request gets a correlation id: the caller's X-Correlation-ID when it is safe to reuse,
/// otherwise a generated one (the W3C trace id of the current request activity when available).
/// </summary>
public sealed class CorrelationIdMiddlewareTests
{
    private readonly ILogger<CorrelationIdMiddleware> _logger = Substitute.For<ILogger<CorrelationIdMiddleware>>();

    private CorrelationIdMiddleware CreateMiddleware(RequestDelegate? next = null)
        => new(next ?? (_ => Task.CompletedTask), _logger);

    private static DefaultHttpContext RequestWithCorrelationId(string? correlationId)
    {
        var context = new DefaultHttpContext();
        if (correlationId is not null)
        {
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;
        }

        return context;
    }

    private static string ResponseCorrelationId(HttpContext context)
        => context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();

    [Test]
    public async Task InvokeAsync__When_request_has_valid_correlation_id__Should_return_the_same_id_in_response_header()
    {
        var context = RequestWithCorrelationId("order-42_abc.DEF:1");

        await CreateMiddleware().InvokeAsync(context);

        await Assert.That(ResponseCorrelationId(context)).IsEqualTo("order-42_abc.DEF:1");
    }

    [Test]
    public async Task InvokeAsync__When_request_has_no_correlation_id__Should_generate_one_and_return_it_in_response_header()
    {
        var context = RequestWithCorrelationId(null);

        await CreateMiddleware().InvokeAsync(context);

        await Assert.That(string.IsNullOrWhiteSpace(ResponseCorrelationId(context))).IsFalse();
    }

    [Test]
    [Arguments("line\r\nInjected: header")]
    [Arguments("   ")]
    [Arguments("<script>alert(1)</script>")]
    [Arguments("0123456789012345678901234567890123456789012345678901234567890123456789")]
    public async Task InvokeAsync__When_incoming_correlation_id_is_unsafe__Should_replace_it_with_generated_id(string unsafeValue)
    {
        var context = RequestWithCorrelationId(unsafeValue);

        await CreateMiddleware().InvokeAsync(context);

        var correlationId = ResponseCorrelationId(context);
        await Assert.That(correlationId).IsNotEqualTo(unsafeValue);
        await Assert.That(CorrelationIdMiddleware.IsValid(correlationId)).IsTrue();
    }

    [Test]
    public async Task InvokeAsync__When_request_activity_is_running_and_no_id_is_sent__Should_use_trace_id_as_correlation_id()
    {
        using var activity = new Activity("incoming-request").SetIdFormat(ActivityIdFormat.W3C).Start();
        var context = RequestWithCorrelationId(null);

        await CreateMiddleware().InvokeAsync(context);

        await Assert.That(ResponseCorrelationId(context)).IsEqualTo(activity.TraceId.ToHexString());
    }

    [Test]
    public async Task InvokeAsync__When_request_is_processed__Should_expose_correlation_id_to_the_rest_of_the_pipeline()
    {
        string? correlationIdSeenDownstream = null;
        var context = RequestWithCorrelationId("abc-123");
        var middleware = CreateMiddleware(httpContext =>
        {
            correlationIdSeenDownstream = CorrelationIdMiddleware.GetCorrelationId(httpContext);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        await Assert.That(correlationIdSeenDownstream).IsEqualTo("abc-123");
    }

    [Test]
    public async Task InvokeAsync__When_request_activity_is_running__Should_tag_it_with_correlation_id()
    {
        using var activity = new Activity("incoming-request").Start();
        var context = RequestWithCorrelationId("abc-123");

        await CreateMiddleware().InvokeAsync(context);

        await Assert.That(activity.GetTagItem(CorrelationIdMiddleware.ActivityTagName)).IsEqualTo("abc-123");
    }

    [Test]
    public async Task InvokeAsync__When_request_is_processed__Should_open_logging_scope_with_correlation_id()
    {
        var context = RequestWithCorrelationId("abc-123");

        await CreateMiddleware().InvokeAsync(context);

        // The logging scope is the observable behavior: it attaches CorrelationId to every log entry of the request.
        _logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(state =>
            (string)state[CorrelationIdMiddleware.LogScopeKey] == "abc-123"));
        await Assert.That(ResponseCorrelationId(context)).IsEqualTo("abc-123");
    }
}

