# AI Agent Context — CRM DDD

Szybki kontekst dla Cursor, GitHub Copilot, Claude i innych agentów pracujących nad SDC-CRM.
Zasady kodowania są w `.github/copilot-instructions.md` i `.github/instructions/*` — ten plik ich nie zastępuje.

## Cel systemu

CRM dla ok. 20 handlowców i działu backoffice (`doc/CRM.md`). Handlowcy prowadzą klientów, leady, szanse,
kontakty, notatki i follow-upy oraz tworzą zamówienia; backoffice weryfikuje i realizuje zamówienia.

```text
Lead -> Opportunity -> SalesOrder -> BackofficeOrderCase -> Completed / Cancelled / ReturnedToSales
```

## Stan projektu (2026-09-26)

Zaimplementowany jest pionowy wycinek **leada** i infrastruktura — szczegóły: [`ai_readable/system_context.md`](ai_readable/system_context.md).

- Backend .NET 10 (`Backend/src/SDC.CRM.{Api,Application,Domain,Infrastructure}`): agregat `Lead`
  (`Register`, `Qualify`, `Reject`), `POST /api/leads` (`RegisterLead`), `GET /api/leads/mine` (`GetMyLeadsQuery`),
  JWT + polityki ról, PostgreSQL z migracjami EF Core, OpenTelemetry, `X-Correlation-ID`.
- Web (Angular 22): logowanie SSO, „Moje leady”, „Nowy lead”. Mobile (.NET MAUI): logowanie SSO, „Moje leady”.
- SSO: SimpleIdServer (`Integrations/Sso`) — konta i role; lokalna infrastruktura w `docker-compose.yml`.
- Zdarzenia domenowe są rejestrowane w agregatach, ale **nie są publikowane** — brak brokera i outboxa (T-02).
- Pozostałe konteksty są zaprojektowane, ale niezaimplementowane.

## Bounded contexty

| Kontekst | Agregaty / elementy | Stan |
|---|---|---|
| Customer Management | `Customer` (+ `ContactPerson`) | ○ |
| Sales Pipeline | `Lead`, `Opportunity` | ◐ `Lead` częściowo |
| Sales Activities | `SalesActivity` (kontakt, `SalesNote`, `FollowUp`) | ○ |
| Order Capture | `SalesOrder` | ○ |
| Order Backoffice | `BackofficeOrderCase` | ○ |
| Reporting & KPI | read modele: `SalespersonDashboard`, `SalesManagerDashboard`, `BackofficeReport` | ○ |
| Identity & Access | SimpleIdServer, `ICurrentUser`, `CrmRoles`, `CrmPolicies`, `AuditLog` | ◐ |
| Integrations | `IntegrationJob`, `ExternalSystemMapping` (ACL) | ○ Could Have |

Szczegóły: [`ai_readable/bounded_contexts_and_aggregates.md`](ai_readable/bounded_contexts_and_aggregates.md),
mapa: [`ai_readable/context_map.md`](ai_readable/context_map.md).

## Najważniejsze granice

1. `SalesOrder` (co sprzedano) i `BackofficeOrderCase` (jak backoffice realizuje) to **dwa różne agregaty**.
   Sprzedaż nie zmienia statusu realizacji, backoffice nie zmienia pipeline ani danych `SalesOrder`.
2. `Customer` należy do Customer Management; inne konteksty trzymają `CustomerId` i snapshot tylko, gdy jest potrzebny.
3. Między kontekstami: zdarzenia integracyjne; synchronicznie tylko zapytania/snapshoty i autoryzacja.
4. Konta i role pochodzą z dostawcy tożsamości — CRM nie ma agregatów `User`/`Role`; backend zawsze egzekwuje uprawnienia.
5. Reporting tylko czyta; integracje wyłącznie przez ACL.

## Główny przepływ zdarzeń

```text
RegisterLead → LeadRegistered                                        ✔
QualifyLead → LeadQualified → (polityka) CreateOpportunityFromLead → OpportunityCreated
ChangeOpportunityStage → OpportunityStageChanged
WinOpportunity → OpportunityWon
(handlowiec) CreateOrderFromOpportunity → SalesOrderCreated
SubmitOrderToBackoffice → SalesOrderSubmittedToBackoffice ⇒ SalesOrderSubmittedIntegrationEvent
(polityka) OpenBackofficeOrderCase → BackofficeOrderCaseOpened
AssignBackofficeOrder → BackofficeOrderAssigned
ChangeBackofficeOrderStatus → BackofficeOrderStatusChanged | BackofficeOrderBlocked
ReturnOrderToSales → BackofficeOrderReturnedToSales ⇒ BackofficeOrderReturnedToSalesIntegrationEvent
    → (polityka) ReopenSalesOrderAfterReturn → poprawa → SubmitOrderToBackoffice (ponownie)
CompleteOrder → BackofficeOrderCompleted ⇒ BackofficeOrderCompletedIntegrationEvent
    → (Could Have) ExportOrderToErp
```

Katalog zdarzeń: [`ai_readable/events_catalog.md`](ai_readable/events_catalog.md),
statusy: [`ai_readable/order_status_lifecycle.md`](ai_readable/order_status_lifecycle.md).

## Reguły biznesowe do zachowania

- Odrzucony lead nie może zostać zakwalifikowany; odrzucenie wymaga powodu.
- Przegrana szansa wymaga powodu przegranej.
- Niekompletnego zamówienia nie można przekazać do backoffice; przekazanego nie można dowolnie edytować.
- Zablokowanie wymaga powodu; zwrot do handlowca wymaga komentarza; zakończenie zapisuje datę zakończenia.
- Statusy backoffice zmieniają się tylko zgodnie z dozwolonym workflow (Q-06).
- Istotne zmiany trafiają do audytu (użytkownik, czas, akcja, identyfikator obiektu).

## Jak dodawać kod

- Kolejność pracy: [`doc/04-plan-realizacji-pbi.md`](../04-plan-realizacji-pbi.md) — bierz następne PBI, którego zależności
  są ukończone, a pytania `Q-xx` rozstrzygnięte.

- Trzymaj się istniejącej struktury: projekty `SDC.CRM.*`, folder obszaru w każdej warstwie i folder przypadku użycia,
  np. `Backend/src/SDC.CRM.Application/Leads/RegisterLead/`. **Nie** twórz `Modules/` ani `BuildingBlocks/` bez ADR (T-01).
- Używaj wyłącznie nazw kanonicznych z [`ai_readable/naming_decisions.md`](ai_readable/naming_decisions.md)
  (np. `BackofficeOrderCase`, `RegisterLead`, `SalesOrderSubmittedToBackoffice`); aliasy wymienione w tym pliku są zakazane.
- Elementy `Q-xx` z [`ai_readable/open_questions.md`](ai_readable/open_questions.md) implementuj według założenia
  roboczego i oznaczaj `// TODO Business decision (Q-xx): ...`.
- TDD z TUnit i NSubstitute (`.github/instructions/99-tdd-tunit-nsubstitute.instructions.md`).


