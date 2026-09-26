using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SDC.CRM.Api.Observability;

/// <summary>
/// OpenTelemetry wiring for the API: traces, metrics and logs exported over OTLP to the
/// OpenTelemetry Collector (see infra/README.md). Domain and application code stay free of
/// any telemetry library - instrumentation happens at the ASP.NET Core, HttpClient and Npgsql level.
/// </summary>
public static class ObservabilityExtensions
{
    public const string ServiceName = "sdc-crm-api";

    /// <summary>Standard OpenTelemetry setting; read from configuration or environment variables.</summary>
    public const string OtlpEndpointSetting = "OTEL_EXPORTER_OTLP_ENDPOINT";

    // Npgsql publishes its own ActivitySource and Meter under this name.
    private const string NpgsqlInstrumentationName = "Npgsql";

    public static IServiceCollection AddCrmObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var openTelemetry = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: ServiceName,
                    serviceVersion: typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString())
                .AddAttributes(
                [
                    new KeyValuePair<string, object>("deployment.environment.name", environment.EnvironmentName),
                ]))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(NpgsqlInstrumentationName))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(NpgsqlInstrumentationName))
            .WithLogging(
                configureBuilder: null,
                configureOptions: options =>
                {
                    // Scopes carry the CorrelationId added by CorrelationIdMiddleware.
                    options.IncludeScopes = true;
                    options.IncludeFormattedMessage = true;
                });

        // Export only when a collector endpoint is configured (appsettings.Development.json locally,
        // the OTEL_EXPORTER_OTLP_ENDPOINT environment variable elsewhere). Without it the telemetry stays
        // in-process - e.g. in tests - while trace ids are still attached to log entries.
        if (!string.IsNullOrWhiteSpace(configuration[OtlpEndpointSetting]))
        {
            openTelemetry.UseOtlpExporter();
        }

        return services;
    }
}

