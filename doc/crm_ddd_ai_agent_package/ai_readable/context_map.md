# Mapa kontekstów

Relacje między bounded contextami, właściciele agregatów i sposób komunikacji. Szczegóły agregatów:
[`bounded_contexts_and_aggregates.md`](bounded_contexts_and_aggregates.md), zasady komunikacji:
[`communication_rules.md`](communication_rules.md).

<!-- diagram: 01_context_map_with_entities_value_objects -->
```mermaid
flowchart LR
  classDef agg fill:#fef9c3,stroke:#a16207,color:#0f172a
  classDef aggPart fill:#fef9c3,stroke:#d97706,stroke-width:3px,stroke-dasharray:6 3,color:#0f172a
  classDef rm fill:#dcfce7,stroke:#15803d,color:#0f172a
  classDef comp fill:#ffffff,stroke:#64748b,color:#0f172a
  classDef done fill:#ffffff,stroke:#16a34a,stroke-width:3px,color:#0f172a
  classDef ext fill:#fce7f3,stroke:#be185d,color:#0f172a
  classDef extDone fill:#fce7f3,stroke:#16a34a,stroke-width:3px,color:#0f172a

  IDP["<b>SimpleIdServer</b><br/>OIDC — konta i role"]:::extDone
  ERP["<b>ERP / fakturowanie</b><br/>Q-10"]:::ext

  subgraph IA["Identity & Access"]
    direction TB
    IAM["<b>Tożsamość i uprawnienia</b><br/>ICurrentUser, CrmRoles, CrmPolicies"]:::done
    AUDIT["<b>AuditLog</b><br/>użytkownik, czas, akcja, ID obiektu<br/>CRM-030"]:::comp
  end

  subgraph CM["Customer Management"]
    CUST["<b>Customer</b> «agregat»<br/>encje: ContactPerson<br/>VO: CustomerId, TaxIdentifier,<br/>EmailAddress, PhoneNumber"]:::agg
  end

  subgraph SP["Sales Pipeline"]
    direction TB
    LEAD["<b>Lead</b> «agregat» — częściowo<br/>VO: LeadStatus, LeadSource, LeadPriority,<br/>RejectionReason, EmailAddress, PhoneNumber"]:::aggPart
    OPP["<b>Opportunity</b> «agregat»<br/>encje: StageHistoryEntry<br/>VO: PipelineStage, Money,<br/>Probability, LostReason"]:::agg
  end

  subgraph SA["Sales Activities"]
    ACT["<b>SalesActivity</b> «agregat»<br/>typy: kontakt, SalesNote, FollowUp<br/>VO: ContactChannel, NoteContent,<br/>DueDate, FollowUpStatus, RelatedTo"]:::agg
  end

  subgraph OC["Order Capture"]
    SO["<b>SalesOrder</b> «agregat»<br/>encje: SalesOrderLine<br/>VO: OrderNumber, SalesOrderStatus,<br/>OrderCustomerSnapshot, Money"]:::agg
  end

  subgraph OB["Order Backoffice"]
    BOC["<b>BackofficeOrderCase</b> «agregat»<br/>encje: CaseHistoryEntry, BackofficeComment<br/>VO: BackofficeOrderStatus, BlockingReason,<br/>ReturnToSalesComment, CompletionDate"]:::agg
  end

  subgraph RK["Reporting & KPI"]
    RM["<b>Read modele</b><br/>SalespersonDashboard<br/>SalesManagerDashboard<br/>BackofficeReport"]:::rm
  end

  subgraph IN["Integrations — ACL"]
    JOB["<b>IntegrationJob</b><br/><b>ExternalSystemMapping</b><br/>Could Have — CRM-037/038"]:::comp
  end

  IDP -- "token: sub, role" --> IAM
  LEAD -- "LeadQualified → CreateOpportunityFromLead" --> OPP
  OPP -- "CustomerId — Q-01" --> CUST
  ACT -- "RelatedTo: CustomerId" --> CUST
  ACT -- "RelatedTo: LeadId, OpportunityId" --> OPP
  SO -- "zapytanie: czy szansa wygrana" --> OPP
  SO -- "zapytanie: snapshot klienta" --> CUST
  SO -. "SalesOrderSubmittedIntegrationEvent" .-> BOC
  BOC -. "BackofficeOrderReturnedToSalesIntegrationEvent" .-> SO
  BOC -. "BackofficeOrderCompletedIntegrationEvent" .-> JOB
  JOB -- "API przez ACL" --> ERP
  SP -. "zdarzenia → projekcje" .-> RM
  SA -. "zdarzenia → projekcje" .-> RM
  OC -. "zdarzenia → projekcje" .-> RM
  OB -. "zdarzenia → projekcje" .-> RM
  OB -. "zdarzenia audytowane" .-> AUDIT
  SP -. "zdarzenia audytowane" .-> AUDIT

  subgraph LEGEND["Legenda"]
    direction LR
    LG1["agregat — planowany"]:::agg
    LG2["agregat — częściowo zaimplementowany"]:::aggPart
    LG3["zaimplementowane"]:::done
    LG4["read model"]:::rm
    LG5["system zewnętrzny"]:::ext
    LG6["linia ciągła — wywołanie synchroniczne<br/>linia przerywana — zdarzenie asynchroniczne"]:::comp
  end
```

Plik PNG: [`diagrams/png/01_context_map_with_entities_value_objects.png`](../diagrams/png/01_context_map_with_entities_value_objects.png).

## Relacje

| Dostawca (upstream) | Odbiorca (downstream) | Typ relacji | Mechanizm |
|---|---|---|---|
| Identity & Access (SimpleIdServer) | wszystkie konteksty | Open Host Service, odbiorcy przyjmują model tokena (Conformist) | token OIDC (`sub`, `role`), `ICurrentUser`, polityki `CrmPolicies` |
| Customer Management | Sales Pipeline | Customer / Supplier | `CustomerId` w `Opportunity` (Q-01) |
| Customer Management | Order Capture | Customer / Supplier | zapytanie o snapshot klienta → `OrderCustomerSnapshot` |
| Customer Management, Sales Pipeline | Sales Activities | Conformist | `RelatedTo` — identyfikator klienta, leada albo szansy |
| Sales Pipeline | Order Capture | Customer / Supplier | zapytanie: czy szansa jest wygrana, dane szansy |
| Order Capture | Order Backoffice | Customer / Supplier | `SalesOrderSubmittedIntegrationEvent` |
| Order Backoffice | Order Capture | Customer / Supplier | `BackofficeOrderReturnedToSalesIntegrationEvent` |
| Order Backoffice | Integrations | Customer / Supplier | `BackofficeOrderCompletedIntegrationEvent` (Q-10) |
| Integrations | ERP / fakturowanie | Anti-Corruption Layer | API systemu zewnętrznego; DTO nie trafiają do domeny |
| konteksty biznesowe | Reporting & KPI | Published Language | zdarzenia → projekcje (T-02, T-06) |
| konteksty biznesowe | Identity & Access (`AuditLog`) | Published Language | zdarzenia audytowane (T-05) |

## Granice, których nie wolno przekraczać

- Sprzedaż nie zmienia statusu realizacji, a backoffice nie zmienia pipeline sprzedaży:
  `Lead`, `Opportunity`, kontakt, oferta ≠ weryfikacja, przydział, braki, realizacja, zamknięcie.
- `Order Backoffice` nie modyfikuje `SalesOrder` — zwraca zamówienie zdarzeniem, a zmianę wykonuje `Order Capture`.
- `Reporting & KPI` nie wywołuje komend.

