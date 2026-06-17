---
applyTo: "**/*.cs"
---

# Event Sourcing Rules

Do not use Event Sourcing everywhere by default.

Use Event Sourcing only when there is a clear business reason, for example:

- detailed audit trail is a core requirement,
- reconstructing past state is valuable,
- business state changes are complex and meaningful,
- process history is more important than current snapshot only.

Potential candidates:

- `BackofficeOrderCase` status history,
- `SalesOrder` submission/return/completion lifecycle,
- selected audit-critical business processes.

Rules:

- event names must be past tense,
- events must be immutable,
- events must represent business facts,
- event streams should have clear aggregate identity,
- do not store technical DTOs as event payloads,
- version event schemas explicitly when needed,
- provide snapshotting only if performance requires it.

If Event Sourcing is not required for a process, use standard state persistence plus audit log.
