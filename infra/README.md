# Local Development Infrastructure

This directory contains local development infrastructure configuration for the SDC-CRM solution.

## Components

- **PostgreSQL** — primary database
- **RabbitMQ** — message broker (for future use, initially in-memory broker is used)
- **Redis** — caching (for future use)
- **Seq** — structured log and trace viewer (most convenient for local development)
- **Loki** — log aggregation (Grafana-based stack)
- **Prometheus** — metrics collection
- **Grafana** — dashboards and visualization
- **Jaeger** (v2) — distributed tracing UI
- **Tempo** — trace backend (Grafana-based stack)
- **OpenTelemetry Collector** — telemetry collection and routing

## Telemetry flow

Applications send all signals over OTLP to the OpenTelemetry Collector (`localhost:4317` gRPC or
`localhost:4318` HTTP). The collector (`observability/otel-collector-config.yaml`) fans them out:

| Signal | Destinations | Where to look |
|---|---|---|
| Traces | Jaeger, Tempo, Seq | Jaeger UI, Grafana (Tempo), Seq |
| Logs | Loki, Seq | Grafana (Loki), Seq |
| Metrics | Prometheus (scrapes the collector on `:9464`) | Prometheus, Grafana |

## Image versions

All observability images are pinned to explicit versions. `:latest` moved silently to new major
versions (e.g. Tempo 3.x) whose configuration format differs from the files in `observability/`.
To upgrade, bump the tag in `docker-compose.yml`, read the release notes and adjust the configuration.

## Usage

Run all commands from the repository root directory.

### Start infrastructure

```bash
docker compose up -d
```

### Stop infrastructure

```bash
docker compose down
```

### Stop and remove volumes

```bash
docker compose down -v
```

## Local URLs

Credentials below are local development defaults. Override them by copying `.env.example`
(repository root) to `.env` - the file is git-ignored and every variable is optional.

| Component | URL / Port | Credentials |
|---|---:|---|
| PostgreSQL | localhost:5432 | app / app (`POSTGRES_USER` / `POSTGRES_PASSWORD`) |
| RabbitMQ AMQP | localhost:5672 | app / app (`RABBITMQ_DEFAULT_USER` / `RABBITMQ_DEFAULT_PASS`) |
| RabbitMQ UI | http://localhost:15672 | app / app |
| Redis | localhost:6379 | - |
| Seq | http://localhost:5341 | - |
| Loki | http://localhost:3100 | - |
| Prometheus | http://localhost:9090 | - |
| Grafana | http://localhost:3000 | admin / admin (`GRAFANA_ADMIN_USER` / `GRAFANA_ADMIN_PASSWORD`) |
| Jaeger UI | http://localhost:16686 | - |
| Tempo API | http://localhost:3200 | - |
| OpenTelemetry gRPC | localhost:4317 | - |
| OpenTelemetry HTTP | http://localhost:4318 | - |

## Example .NET appsettings.Development.json

The API reads its database connection from `ConnectionStrings:Crm`
(see `Backend/src/SDC.CRM.Api/appsettings.Development.json`). The remaining entries are examples
for future integrations (RabbitMQ, Redis).

```json
{
  "ConnectionStrings": {
    "Crm": "Host=localhost;Port=5432;Database=appdb;Username=app;Password=app",
    "Redis": "localhost:6379"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "app",
    "Password": "app"
  }
}
```

## Directory Structure

```
infra/
├── grafana/
│   └── provisioning/
│       └── datasources/
│           └── datasources.yaml    # Auto-provisioned Grafana datasources
└── observability/
    ├── loki-config.yaml            # Loki configuration
    ├── otel-collector-config.yaml  # OpenTelemetry Collector configuration
    ├── prometheus.yml              # Prometheus configuration
    └── tempo-config.yaml           # Tempo configuration
```

## Notes

- Grafana datasources are provisioned automatically from `grafana/provisioning/datasources/datasources.yaml`.
- Send application telemetry to the **OpenTelemetry Collector**, not directly to Jaeger, Tempo, Loki, Seq or Prometheus.
- For local development, **Seq** is usually the most convenient log viewer (it also shows traces). Loki and Tempo are included for a Grafana-based observability stack.
- Seq runs without authentication (`SEQ_FIRSTRUN_NOAUTHENTICATION`) - local development only.
- The `docker-compose.yml` file is located in the repository root.

