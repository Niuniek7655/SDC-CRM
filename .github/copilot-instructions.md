# Copilot Instructions

## Project overview

This repository contains a CRM system for sales and backoffice operations.

The core business workflow is:

```text
Lead -> Opportunity -> SalesOrder -> BackofficeOrderCase -> Completed / Cancelled / ReturnedToSales
```

The system is used by:

- salespeople who register leads, manage customers, plan follow-ups, add notes and create orders,
- sales managers who monitor sales pipeline and team activity,
- backoffice users who process submitted orders,
- administrators who manage users, roles and configuration.

Use domain language consistently. Prefer these terms in code, tests, API contracts and UI models:

- `Customer`
- `ContactPerson`
- `Lead`
- `Opportunity`
- `SalesActivity`
- `SalesNote`
- `FollowUp`
- `SalesOrder`
- `BackofficeOrderCase`
- `OrderStatus`

Avoid introducing alternative names such as `Deal`, `Prospect`, `Ticket`, `Item` or `Record` unless the domain model is explicitly changed.

## Repository layout

Follow the existing repository structure. If the project structure is being created from scratch, prefer:

```text
/backend
  /src
  /tests

/frontend
  /src
  /tests

/docs
  /domain
  /product
```

Keep product and domain documentation under `/docs`. Do not put full backlog or long domain specifications in this file.

## Build, run and test

When adding or changing backend code, run the relevant backend build and tests.

Expected commands for a .NET backend:

```bash
dotnet restore
dotnet build
dotnet test
```

When adding or changing frontend code, run the relevant frontend install, build and tests.

Expected commands for a TypeScript frontend:

```bash
npm install
npm run build
npm test
```

If the actual repository uses different commands, follow the commands defined in the repository scripts, README, CI configuration or package files.

## Architecture rules

Use a domain-first modular monolith unless the repository already defines another architecture.

Keep clear boundaries between these areas:

- Customer Management
- Sales Pipeline
- Sales Activities
- Order Capture
- Order Backoffice
- Reporting
- Identity and Access
- Integrations

Use these layers where applicable:

```text
API / Presentation
Application
Domain
Infrastructure
```

Rules:

- Keep business rules out of controllers and UI components.
- Keep domain logic in aggregates, entities, value objects or domain services.
- Keep infrastructure concerns out of the domain layer.
- Use read models or projections for reports.
- Use an anti-corruption layer for external integrations such as ERP, invoicing, e-mail, calendar and payments.

## Backend guidelines

Use business-oriented commands and methods.

Prefer names such as:

- `CreateLead`
- `AssignLeadToSalesperson`
- `QualifyLead`
- `RejectLead`
- `CreateOpportunityFromLead`
- `ChangeOpportunityStage`
- `WinOpportunity`
- `LoseOpportunity`
- `CreateOrderFromOpportunity`
- `SubmitOrderToBackoffice`
- `AssignBackofficeOrder`
- `ChangeBackofficeOrderStatus`
- `ReturnOrderToSales`
- `CompleteOrder`

Avoid generic names such as:

- `UpdateEntity`
- `ProcessData`
- `SaveModel`
- `HandleRequest`

Important business rules to preserve:

- A rejected lead cannot be qualified.
- A rejected lead must have a rejection reason.
- A lost opportunity must have a lost reason.
- An order cannot be submitted to backoffice if required data is missing.
- A submitted order cannot be freely edited by a salesperson.
- A blocked order must have a blocking reason.
- A completed order must have a completion date.
- Backoffice status transitions must follow the allowed workflow.

Do not expose persistence entities directly from API endpoints. Use request/response contracts or DTOs.

## Frontend guidelines

Build UI around user workflows, not database tables.

Important screens include:

- My Leads
- Lead Details
- Customer 360
- Opportunity Pipeline
- Follow-up List
- Create Sales Order
- Order Status
- Backoffice Queue
- Backoffice Order Details
- Manager Dashboards

Frontend authorization is not enough. Hide unavailable actions in the UI, but always rely on backend authorization for enforcement.

Forms should:

- show required fields clearly,
- display backend validation errors,
- preserve user input after validation failure,
- avoid overloading initial lead registration with unnecessary fields,
- allow draft order creation before submission to backoffice.

## Authorization and security

Always enforce authentication and authorization on the backend.

Core roles:

- `Salesperson`
- `SalesManager`
- `BackofficeUser`
- `BackofficeManager`
- `Admin`

Rules:

- A salesperson usually works only with their own leads, opportunities, activities and orders.
- A sales manager may manage team sales data.
- A backoffice user works on submitted orders, not on the full sales pipeline.
- An admin manages users, roles and configuration.
- Never hardcode secrets.
- Do not log sensitive customer data.
- Validate all external input on the backend.

## Auditing

Audit important business changes, especially:

- lead assignment,
- lead rejection,
- opportunity stage change,
- opportunity won or lost,
- order submission to backoffice,
- backoffice order assignment,
- order status change,
- order return to sales,
- order completion,
- order cancellation,
- role or permission change.

Audit records should include the user, timestamp, action and business object identifier.

## Testing expectations

Add tests for business behavior, not only technical implementation.

Prefer behavior-oriented test names, for example:

```text
RejectLead_ShouldRequireRejectionReason
QualifyLead_ShouldFail_WhenLeadIsRejected
LoseOpportunity_ShouldRequireLostReason
SubmitOrderToBackoffice_ShouldFail_WhenOrderIsIncomplete
ReturnOrderToSales_ShouldRequireComment
CompleteOrder_ShouldSetCompletionDate
```

For each business feature, cover:

- the main success path,
- at least one validation or authorization failure,
- relevant state transition,
- relevant audit or domain event behavior if implemented.

## Implementation order

When implementing new functionality, prefer small vertical slices in this order:

1. Register new lead with minimal customer and contact data.
2. Show my leads list.
3. Show lead details.
4. Add note or contact to lead.
5. Qualify lead into opportunity.
6. Manage opportunity pipeline stages.
7. Mark opportunity as won or lost.
8. Create sales order from won opportunity.
9. Submit order to backoffice.
10. Process order in backoffice queue.
11. Return order to sales if data is missing.
12. Complete order.
13. Add reports and dashboards.
14. Add external integrations.

A vertical slice should include backend behavior, API contract, frontend change if applicable, tests and authorization checks.

## Do not

Do not:

- implement core workflows as generic CRUD only,
- put business logic only in controllers, handlers or UI components,
- leak EF Core entities or persistence models into API contracts,
- leak external integration DTOs into domain models,
- add speculative features outside the requested story,
- introduce microservices before the modular monolith boundaries are proven,
- physically delete business records that require history,
- bypass backend authorization because the frontend hides a button.

## When requirements are unclear

Prefer the simplest behavior that supports the main CRM workflow.

Do not invent complex business rules. Add a clear TODO with the business question, for example:

```csharp
// TODO Business decision: Can a salesperson edit an order after submitting it to backoffice?
```

Common business questions to clarify:

- Can a lead exist without a customer?
- Can an order be created without a won opportunity?
- Can backoffice cancel an order?
- Which backoffice status transitions are allowed?
- Which reports are required for MVP?
- Which integrations are required for MVP?
