---
applyTo: "**/*.cs"
---

# Vertical Slice Rules

Organize application logic by use case / feature, not by generic technical folders only.

Prefer this shape inside a module:

```text
Modules/
  Sales/
    Leads/
      RegisterLead/
        RegisterLeadCommand.cs
        RegisterLeadHandler.cs
        RegisterLeadValidator.cs
        RegisterLeadEndpoint.cs
        RegisterLeadTests.cs
      QualifyLead/
        QualifyLeadCommand.cs
        QualifyLeadHandler.cs
        QualifyLeadEndpoint.cs
        QualifyLeadTests.cs
```

Current repository layout: the backend is not split into `Modules/` yet — introducing them is an open architecture
decision (T-01 in `doc/crm_ddd_ai_agent_package/ai_readable/open_questions.md`). Until that ADR, keep each slice in the
existing layer projects under a folder per area and use case, for example `Backend/src/SDC.CRM.Application/Leads/RegisterLead/`
(command, handler), `Backend/src/SDC.CRM.Domain/Leads/` (aggregate, events), `Backend/src/SDC.CRM.Api/Controllers/` and
`Backend/src/SDC.CRM.Api/Contracts/Leads/` (endpoint, request/response contracts), and tests in the matching
`Backend/tests/` projects (e.g. `SDC.CRM.Application.Tests/Leads/RegisterLead/`).

A vertical slice should contain everything needed for one business use case:

- command or query
- handler
- request/response contract
- endpoint
- validation
- authorization check
- domain interaction
- tests

Do not create generic service classes such as:

- `LeadService`
- `OrderService`
- `BackofficeService`

unless they represent a clear domain service or infrastructure adapter.

Prefer business operation names:

- `RegisterLead`
- `QualifyLead`
- `WinOpportunity`
- `CreateOrderFromOpportunity`
- `SubmitOrderToBackoffice`
- `ReturnOrderToSales`
- `CompleteOrder`

Each slice should be small, independently testable and understandable.
