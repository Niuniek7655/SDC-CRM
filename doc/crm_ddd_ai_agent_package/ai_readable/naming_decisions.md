# Nazwy kanoniczne i zakazane aliasy

W kodzie, testach, kontraktach API, UI (web i mobile), dokumentacji i instrukcjach AI używaj **wyłącznie nazw kanonicznych**.
Aliasy z ostatniej kolumny pochodzą z wcześniejszych wersji dokumentów i instrukcji — wolno je wymieniać tylko w tym pliku.
Walidator `tools/validate-docs.ps1` zgłasza je w pozostałych plikach `doc/` oraz w instrukcjach AI
(`.github/copilot-instructions.md`, `.github/instructions`, `.github/prompts`, `.claude`, `.cursor/rules`).

Kolejność rozstrzygania rozbieżności: kod (elementy zaimplementowane) → słownik `doc/01` → backlog `doc/03`
→ role `doc/02` → `.github/copilot-instructions.md` i `.github/instructions/*` (kopie w `.claude` i `.cursor` są z nimi zsynchronizowane).

## Zasady nazewnictwa

- **Komenda** — tryb rozkazujący i nazwa biznesowa (`QualifyLead`). W kodzie klasa `<Nazwa>Command`
  i handler `<Nazwa>Handler` w folderze przypadku użycia (wzór: `Backend/src/SDC.CRM.Application/Leads/RegisterLead/`).
- **Zapytanie** — `Get<Co>Query` (np. `GetMyLeadsQuery`), handler `Get<Co>Handler`, wynik jako DTO / read model.
- **Zdarzenie domenowe** — `<Agregat lub obszar><czasownik w czasie przeszłym>` (np. `LeadRegistered`).
  Zdarzenia sprawy backoffice mają prefiks `BackofficeOrder`, a zamówienia handlowca `SalesOrder` —
  samo `Order…` jest niejednoznaczne.
- **Zdarzenie integracyjne** (między kontekstami) — nazwa faktu z sufiksem `IntegrationEvent`, zgodnie z przykładami
  z `.github/instructions/50-event-driven-messaging.instructions.md` (np. `SalesOrderSubmittedIntegrationEvent`,
  `BackofficeOrderCompletedIntegrationEvent`).
- **Statusy** — w kodzie angielskie identyfikatory (`New`, `Qualified`), w UI polskie etykiety ze słownika `doc/01` §6.

## Konteksty (bounded contexts)

Te same nazwy obowiązują w słowniku (§1), w `copilot-instructions.md` (sekcja „Architecture rules”) i w pakiecie DDD.

| Nazwa kanoniczna | Zakazane aliasy |
|---|---|
| `Customer Management` | — |
| `Sales Pipeline` | Lead & Pipeline |
| `Sales Activities` | Sales Activity (jako nazwa kontekstu) |
| `Order Capture` | Sales Order Capture |
| `Order Backoffice` | Backoffice Order Processing |
| `Reporting & KPI` | Reporting & Analytics |
| `Identity & Access` | Identity and Access |
| `Integrations` | Integration Context, Integration / ACL |

`Lead Management` to nazwa **epiku** backlogu, nie kontekstu (leady należą do `Sales Pipeline`).
`Administration` to epik i **obszar wspierający** spoza ośmiu kontekstów słownika — konfiguracja słowników
(CRM-036, Q-19); zarządzanie użytkownikami (CRM-035) należy do `Identity & Access`.

## Agregaty, encje i value objecty

| Nazwa kanoniczna | Uwagi | Zakazane aliasy |
|---|---|---|
| `BackofficeOrderCase` | sprawa backoffice; osobny agregat niż `SalesOrder` | OrderProcess, OrderProcessId |
| `SalesActivity` | jeden agregat z typami: kontakt, `SalesNote`, `FollowUp` (słownik §7, T-04) | Note (jako agregat), FollowUpTask |
| `Opportunity` | szansa sprzedaży | Deal |
| `PipelineStage` | value object etapu szansy (słownik §6.2) | OpportunityStage, agregat Pipeline |
| `FollowUpStatus` | status follow-upu | TaskStatus (koliduje z `System.Threading.Tasks.TaskStatus` przy `ImplicitUsings`) |
| `TaxIdentifier` | NIP | TaxId |
| `EmailAddress` | w kodzie obecnie `SDC.CRM.Domain.Common.Email` (T-07) | — |
| `SalesOrderStatus`, `BackofficeOrderStatus` | status zamówienia handlowca / sprawy backoffice | ProcessStatus |
| `OrderStatus` | status prezentowany użytkownikowi (read model `GetSalesOrderStatusQuery`) | — |
| `CaseHistoryEntry` | historia statusów, przypisań i zwrotów sprawy | StatusHistoryEntry (w sprawie backoffice) |

Usunięte z modelu (brak w backlogu): BackofficeTask, InvoiceRequest, agregat Pipeline, UpdateCustomerData / CustomerUpdated.

## Komendy

| Nazwa kanoniczna | Źródło | Zakazane aliasy |
|---|---|---|
| `RegisterLead` | kod (`RegisterLeadCommand`) | CreateLead |
| `AssignLeadToSalesperson` | backlog CRM-004 | AssignLead |
| `CreateOpportunityFromLead` | backlog CRM-014 | CreateOpportunity |
| `ChangeOpportunityStage` | backlog CRM-015 | MoveOpportunityToStage |
| `WinOpportunity` / `LoseOpportunity` | backlog CRM-016/017 | MarkOpportunityAsWon, MarkOpportunityAsLost |
| `LogSalesActivity` | backlog CRM-011 | RegisterPhoneCall, RegisterMeeting, RegisterEmailActivity, RegisterContact |
| `AddSalesNote` | backlog CRM-010 | AddCustomerNote, AddNote |
| `CreateOrderFromOpportunity` | backlog CRM-018 | CreateSalesOrder, CreateSalesOrderFromOpportunityCommand |
| `SubmitOrderToBackoffice` | backlog CRM-020 | SubmitSalesOrder |
| `AssignBackofficeOrder` | backlog CRM-023 | AssignOrderProcess |
| `ReturnOrderToSales` | backlog CRM-026 | RequestMissingInformation, ProvideMissingInfo |
| `CompleteOrder` | backlog CRM-027 | CompleteOrderProcess, CompleteBackofficeOrder, CompleteBackofficeOrderCommand |
| `CancelOrder` | słownik §5 (status „Anulowane”), Q-07 | RejectOrderProcess |

Metody agregatów z `.github/instructions/30-ddd-aggregates.instructions.md` (np. `opportunity.MarkAsWon()`,
`backofficeCase.Complete(completedAt)`) to nazwy metod domenowych, nie komend — są zgodne z modelem.

## Zdarzenia

| Nazwa kanoniczna | Zakazane aliasy |
|---|---|
| `LeadRegistered` | LeadCreated |
| `SalesActivityLogged` | ActivityRegistered |
| `SalesNoteAdded` | NoteAdded |
| `SalesOrderSubmittedToBackoffice` | SalesOrderSubmitted, OrderSubmittedToBackoffice |
| `BackofficeOrderCaseOpened` | OrderProcessingStarted |
| `BackofficeOrderAssigned` | OrderAssigned |
| `BackofficeOrderBlocked` | OrderBlocked |
| `BackofficeOrderReturnedToSales` | OrderReturnedToSales, MissingInformationRequested, MissingInformationProvided |
| `BackofficeOrderCompleted` | OrderCompleted, OrderAcceptedForFulfillment |
| `BackofficeOrderCancelled` | OrderCancelled, OrderRejected |

## Role

Wyłącznie role z `doc/02` §2 i `Backend/src/SDC.CRM.Api/Authorization/CrmRoles.cs`:
`Salesperson`, `SalesManager`, `BackofficeUser`, `BackofficeManager`, `Admin`.
Nie wprowadzaj aktorów spoza macierzy uprawnień (np. „zarząd”, „realizacja/operations”) — patrz Q-14.

