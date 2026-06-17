---
applyTo: "**/*.cs"
---

# DDD Aggregate and Domain Model Rules

Put business rules inside the domain model.

Core aggregate candidates:

- `Customer`
- `Lead`
- `Opportunity`
- `SalesActivity`
- `SalesOrder`
- `BackofficeOrderCase`

Use aggregate roots to protect invariants.

Examples:

- `Lead` controls lead status, assignment, qualification and rejection.
- `Opportunity` controls pipeline stage, won/lost state and lost reason.
- `SalesOrder` controls draft/submission state and order completeness.
- `BackofficeOrderCase` controls backoffice assignment, status transitions, blocking, returning and completion.

Prefer aggregate methods with business names:

```csharp
lead.AssignTo(salespersonId);
lead.Qualify();
lead.Reject(reason);

opportunity.ChangeStage(stage);
opportunity.MarkAsWon();
opportunity.MarkAsLost(reason);

salesOrder.SubmitToBackoffice();
backofficeCase.ReturnToSales(comment);
backofficeCase.Complete(completedAt);
```

Avoid anemic domain models where handlers manipulate public setters.

Use value objects for concepts such as:

- `CustomerId`
- `LeadId`
- `OpportunityId`
- `SalesOrderId`
- `BackofficeOrderCaseId`
- `Money`
- `EmailAddress`
- `PhoneNumber`
- `TaxIdentifier`
- `OrderNumber`
- `PipelineStage`
- `OrderStatus`
- `RejectionReason`
- `LostReason`
- `BlockingReason`

Use references between aggregates by ID, not by object graph.

Prefer:

```csharp
CustomerId CustomerId
OpportunityId OpportunityId
```

Avoid:

```csharp
Customer Customer
Opportunity Opportunity
```

unless there is a strong, explicit reason.
