---
applyTo: "**/*.cs"
---

# Clean Architecture and Modular Monolith Rules

Use Clean Architecture as the main dependency rule.

Allowed dependency direction:

```text
API / Presentation -> Application -> Domain
Infrastructure -> Application / Domain abstractions
```

Domain must not depend on:

- API or controllers
- Entity Framework / persistence
- message broker implementation
- logging implementation
- Docker or environment configuration
- external integrations

Application layer should contain:

- commands
- queries
- command handlers
- query handlers
- use-case DTOs
- orchestration
- transaction boundaries
- authorization checks at use-case level
- event publishing orchestration

Domain layer should contain:

- aggregate roots
- entities
- value objects
- domain events
- domain services only when behavior does not naturally belong to an aggregate
- domain invariants

Infrastructure layer should contain:

- EF Core / PostgreSQL persistence
- message broker implementations
- outbox/inbox if introduced
- observability implementation
- external system clients
- event store implementation if Event Sourcing is used

Modular monolith rule:

- Split code by business modules, not by technical type only.
- Keep module boundaries explicit.
- Avoid direct cross-module entity references.
- Cross-module communication should happen through contracts, commands, events or module APIs.
- Design modules so that future extraction to separate services is possible.
