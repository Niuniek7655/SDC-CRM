# Katalog zdarzeń

Wersja maszynowa: [`events_catalog.json`](events_catalog.json). Oznaczenia: ✔ zaimplementowane, ◐ częściowo,
○ planowane, ❓ do decyzji. Konwencje nazw: [`naming_decisions.md`](naming_decisions.md).

Stan publikacji: zdarzenia są rejestrowane w `Entity.DomainEvents`, ale jeszcze nie są publikowane (T-02).
Zdarzenia z kolumną „Audyt” = tak zasilają `AuditLog` (CRM-030, sekcja „Auditing” w `.github/copilot-instructions.md`).

## Zdarzenia domenowe

| Zdarzenie | Kontekst | Agregat | Komenda | Kluczowe dane | Audyt | Stan | Story |
|---|---|---|---|---|---|---|---|
| `CustomerCreated` | Customer Management | `Customer` | `CreateCustomer` | CustomerId, nazwa, TaxIdentifier | — | ○ | CRM-007 |
| `ContactPersonAdded` | Customer Management | `Customer` | `AddContactPerson` | CustomerId, ContactPersonId, czy główny | — | ○ | CRM-008 |
| `LeadRegistered` | Sales Pipeline | `Lead` | `RegisterLead` | LeadId, CompanyName, AssignedSalespersonId | — | ✔ | CRM-001 |
| `LeadAssigned` | Sales Pipeline | `Lead` | `AssignLeadToSalesperson` | LeadId, poprzedni i nowy właściciel | tak | ○ | CRM-004 |
| `LeadQualified` | Sales Pipeline | `Lead` | `QualifyLead` | LeadId | — | ◐ | CRM-005 |
| `LeadRejected` | Sales Pipeline | `Lead` | `RejectLead` | LeadId, powód odrzucenia | tak | ◐ | CRM-006 |
| `OpportunityCreated` | Sales Pipeline | `Opportunity` | `CreateOpportunityFromLead` | OpportunityId, LeadId, CustomerId, właściciel, etap | — | ○ | CRM-005, CRM-014 |
| `OpportunityStageChanged` | Sales Pipeline | `Opportunity` | `ChangeOpportunityStage` | OpportunityId, etap poprzedni i nowy | tak | ○ | CRM-015 |
| `OpportunityWon` | Sales Pipeline | `Opportunity` | `WinOpportunity` | OpportunityId, CustomerId, wartość | tak | ○ | CRM-016 |
| `OpportunityLost` | Sales Pipeline | `Opportunity` | `LoseOpportunity` | OpportunityId, LostReason | tak | ○ | CRM-017 |
| `SalesActivityLogged` | Sales Activities | `SalesActivity` | `LogSalesActivity` | SalesActivityId, kanał, RelatedTo, data, autor | — | ○ | CRM-011 |
| `SalesNoteAdded` | Sales Activities | `SalesActivity` | `AddSalesNote` | SalesActivityId, RelatedTo, autor | — | ○ | CRM-010 |
| `FollowUpScheduled` | Sales Activities | `SalesActivity` | `ScheduleFollowUp` | SalesActivityId, RelatedTo, termin, właściciel | — | ○ | CRM-012 |
| `FollowUpCompleted` | Sales Activities | `SalesActivity` | `CompleteFollowUp` | SalesActivityId, data wykonania | — | ○ | CRM-013 |
| `SalesOrderCreated` | Order Capture | `SalesOrder` | `CreateOrderFromOpportunity` | SalesOrderId, OpportunityId, CustomerId, właściciel | — | ○ | CRM-018 |
| `SalesOrderSubmittedToBackoffice` | Order Capture | `SalesOrder` | `SubmitOrderToBackoffice` | SalesOrderId, numer przekazania, przekazujący | tak | ○ | CRM-020, CRM-026 |
| `BackofficeOrderCaseOpened` | Order Backoffice | `BackofficeOrderCase` | `OpenBackofficeOrderCase` | BackofficeOrderCaseId, SalesOrderId | — | ○ | CRM-020, CRM-022 |
| `BackofficeOrderAssigned` | Order Backoffice | `BackofficeOrderCase` | `AssignBackofficeOrder` | BackofficeOrderCaseId, pracownik, przypisujący | tak | ○ | CRM-023 |
| `BackofficeOrderStatusChanged` | Order Backoffice | `BackofficeOrderCase` | `ChangeBackofficeOrderStatus` | BackofficeOrderCaseId, status poprzedni i nowy | tak | ○ | CRM-024 |
| `BackofficeOrderBlocked` | Order Backoffice | `BackofficeOrderCase` | `ChangeBackofficeOrderStatus` | BackofficeOrderCaseId, BlockingReason | tak | ○ | CRM-024 |
| `BackofficeCommentAdded` | Order Backoffice | `BackofficeOrderCase` | `AddBackofficeComment` | BackofficeOrderCaseId, widoczność, autor | — | ○ | CRM-025 |
| `BackofficeOrderReturnedToSales` | Order Backoffice | `BackofficeOrderCase` | `ReturnOrderToSales` | BackofficeOrderCaseId, SalesOrderId, komentarz | tak | ○ | CRM-026 |
| `BackofficeOrderCompleted` | Order Backoffice | `BackofficeOrderCase` | `CompleteOrder` | BackofficeOrderCaseId, SalesOrderId, CompletionDate | tak | ○ | CRM-027 |
| `BackofficeOrderCancelled` | Order Backoffice | `BackofficeOrderCase` | `CancelOrder` | BackofficeOrderCaseId, SalesOrderId, CancellationReason | tak | ❓ Q-07 | — |
| `IntegrationJobSucceeded` | Integrations | `IntegrationJob` | `ExportOrderToErp`, `RetryIntegrationJob` | IntegrationJobId, system, ExternalId | — | ○ | CRM-038 |
| `IntegrationJobFailed` | Integrations | `IntegrationJob` | `ExportOrderToErp`, `RetryIntegrationJob` | IntegrationJobId, system, numer próby, kod błędu | — | ○ | CRM-038 |

Zmiany ról i uprawnień również podlegają audytowi, ale zachodzą w dostawcy tożsamości (Q-12).

## Zdarzenia integracyjne

Publikowane przez warstwę aplikacji po zapisie transakcji (docelowo outbox); agregat ich nie publikuje.

| Zdarzenie integracyjne | Zdarzenie źródłowe | Publikuje | Odbiorcy → reakcja | Stan |
|---|---|---|---|---|
| `SalesOrderSubmittedIntegrationEvent` | `SalesOrderSubmittedToBackoffice` | Order Capture | Order Backoffice → `OpenBackofficeOrderCase` (lub wznowienie, Q-08); Reporting & KPI → projekcje | ○ |
| `BackofficeOrderReturnedToSalesIntegrationEvent` | `BackofficeOrderReturnedToSales` | Order Backoffice | Order Capture → `ReopenSalesOrderAfterReturn`; Reporting & KPI → projekcje; Sales Activities → `FollowUp` tylko po decyzji Q-09 | ○ |
| `BackofficeOrderCompletedIntegrationEvent` | `BackofficeOrderCompleted` | Order Backoffice | Integrations → `ExportOrderToErp` (Could Have, Q-10); Reporting & KPI → projekcje | ○ |
| `BackofficeOrderCancelledIntegrationEvent` | `BackofficeOrderCancelled` | Order Backoffice | Order Capture, Reporting & KPI — po decyzji Q-07 | ❓ |

Zdarzenia zwrotne z ERP (wystawienie faktury, płatność) nie są częścią modelu do czasu decyzji Q-10.

## Zasilanie read modeli

Projekcje i audyt subskrybują zdarzenia publikowane po zapisie (T-02). Do tego czasu widoki czytają dane
modułów bezpośrednio (T-06). Przy wydzieleniu modułu do osobnej usługi zdarzenia z tej tabeli staną się
zdarzeniami integracyjnymi.

| Read model | Story | Zdarzenia |
|---|---|---|
| `SalespersonDashboard` | CRM-031 | `LeadRegistered`, `LeadAssigned`, `LeadQualified`, `LeadRejected`, `OpportunityCreated`, `OpportunityWon`, `OpportunityLost`, `FollowUpScheduled`, `FollowUpCompleted`, `SalesOrderSubmittedToBackoffice`, `BackofficeOrderReturnedToSales` |
| `SalesManagerDashboard` | CRM-032 | `OpportunityCreated`, `OpportunityStageChanged`, `OpportunityWon`, `OpportunityLost`, `SalesActivityLogged`, `SalesNoteAdded`, `FollowUpCompleted` |
| `BackofficeReport` | CRM-033 | `BackofficeOrderCaseOpened`, `BackofficeOrderAssigned`, `BackofficeOrderStatusChanged`, `BackofficeOrderBlocked`, `BackofficeOrderReturnedToSales`, `BackofficeOrderCompleted` |
| `Customer360` (`GetCustomer360Query`) | CRM-009 | `CustomerCreated`, `ContactPersonAdded`, `LeadRegistered`, `OpportunityCreated`, `OpportunityWon`, `OpportunityLost`, `SalesActivityLogged`, `SalesNoteAdded`, `SalesOrderCreated`, `BackofficeOrderCompleted` |
| `SalesOrderStatus` (`GetSalesOrderStatusQuery`) | CRM-021 | `SalesOrderCreated`, `SalesOrderSubmittedToBackoffice`, `BackofficeOrderCaseOpened`, `BackofficeOrderStatusChanged`, `BackofficeOrderBlocked`, `BackofficeCommentAdded`, `BackofficeOrderReturnedToSales`, `BackofficeOrderCompleted` |
| `AuditLog` | CRM-030 | `LeadAssigned`, `LeadRejected`, `OpportunityStageChanged`, `OpportunityWon`, `OpportunityLost`, `SalesOrderSubmittedToBackoffice`, `BackofficeOrderAssigned`, `BackofficeOrderStatusChanged`, `BackofficeOrderBlocked`, `BackofficeOrderReturnedToSales`, `BackofficeOrderCompleted`, `BackofficeOrderCancelled` |

