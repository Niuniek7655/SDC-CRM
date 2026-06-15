---
paths:
  - "**/*.cs"
---

# Event-Driven Architecture and Messaging Rules

Design the modular monolith so that modules can later become separate services.

Use events for facts that already happened.

Use commands for requests to perform actions.

Domain event examples:

- `LeadRegistered`
- `LeadQualified`
- `LeadRejected`
- `OpportunityWon`
- `OpportunityLost`
- `SalesOrderCreated`
- `SalesOrderSubmittedToBackoffice`
- `BackofficeOrderAssigned`
- `BackofficeOrderStatusChanged`
- `BackofficeOrderReturnedToSales`
- `BackofficeOrderCompleted`

Integration event examples:

- `SalesOrderSubmittedIntegrationEvent`
- `BackofficeOrderCompletedIntegrationEvent`

Initial messaging:

- use an in-memory broker abstraction,
- keep message contracts independent from implementation,
- avoid coupling domain code to the broker.

Future messaging:

- RabbitMQ should be introduced behind existing abstractions,
- message contracts should remain stable,
- consumers should be idempotent,
- consider outbox/inbox for reliability when moving beyond in-memory messaging.

Do not reference RabbitMQ types in domain or application layers.

Do not publish integration events directly from aggregates. Aggregates may record domain events; application/infrastructure coordinates publishing.
