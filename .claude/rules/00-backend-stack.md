---
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/*.sln"
---

# Backend Project Context

This backend is a .NET 10 CRM application for sales and backoffice operations.

Core product workflow:

```text
Lead -> Opportunity -> SalesOrder -> BackofficeOrderCase
```

Main backend assumptions:

- .NET 10
- Clean Architecture
- modular monolith
- vertical slices inside modules
- CQRS for application logic
- DDD for domain logic and aggregate behavior
- event-driven architecture prepared for future service extraction
- initially in-memory message broker
- future RabbitMQ message broker
- PostgreSQL persistence
- selective Event Sourcing for specific business processes
- structured logs, metrics and distributed tracing
- Docker-based local and deployment environment
- TDD workflow using TUnit and NSubstitute

Use English for code identifiers, namespaces, tests and technical documentation.
