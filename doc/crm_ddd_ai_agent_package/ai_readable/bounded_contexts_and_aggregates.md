# Bounded contexty, agregaty, encje i value objecty

Model docelowy zgodny ze słownikiem (`doc/01`), backlogiem (`doc/03`) i rolami (`doc/02`), z zaznaczonym stanem
implementacji na 2026-09-26. Wersja maszynowa: [`bounded_contexts_and_aggregates.json`](bounded_contexts_and_aggregates.json),
mapa relacji: [`context_map.md`](context_map.md), zdarzenia: [`events_catalog.md`](events_catalog.md).

Oznaczenia: ✔ zaimplementowane, ◐ częściowo, ○ planowane (backlog), ❓ do decyzji ([`open_questions.md`](open_questions.md)).

## Reguły architektoniczne

1. `Customer Management` jest właścicielem `Customer`. Inne konteksty przechowują `CustomerId`, a snapshot danych
   klienta tylko tam, gdzie jest biznesowo potrzebny (`OrderCustomerSnapshot`).
2. `Sales Pipeline` jest właścicielem `Lead` i `Opportunity`. Backoffice nie zmienia pipeline sprzedaży.
3. `Order Capture` jest właścicielem `SalesOrder`, a `Order Backoffice` — `BackofficeOrderCase` i statusu realizacji.
   `SalesOrder` odpowiada na pytanie „co sprzedano i na jakich warunkach”, `BackofficeOrderCase` — „jak backoffice
   obsługuje zamówienie”. To dwa różne agregaty.
4. Zmiany stanu między kontekstami przenoszą zdarzenia integracyjne. Synchronicznie — tylko zapytania/snapshoty
   i autoryzacja ([`communication_rules.md`](communication_rules.md)).
5. Agregaty odwołują się do innych agregatów wyłącznie przez identyfikatory.
6. `Reporting & KPI` tylko czyta (read modele, projekcje); nie jest właścicielem żadnego procesu.
7. `Identity & Access`: konta i role utrzymuje dostawca tożsamości (SimpleIdServer); backend zawsze sam egzekwuje
   uprawnienia na podstawie ról z tokena.
8. Systemy zewnętrzne tylko przez warstwę antykorupcyjną (`Integrations`); ich DTO nie trafiają do domeny.
9. Rekordów wymagających historii (leady, szanse, zamówienia, sprawy backoffice, audyt) nie usuwa się fizycznie.

## Customer Management

**Odpowiedzialność:** źródło prawdy o kliencie i jego osobach kontaktowych; widok 360 klienta.
**Role:** `Salesperson`, `SalesManager`, `Admin`. **Backlog:** CRM-007, CRM-008, CRM-009.
**Folder w kodzie (proponowany):** `Customers`.

### Agregat `Customer` ○

| Element | Nazwa | Stan | Uwagi |
|---|---|---|---|
| Encja | `ContactPerson` | ○ | imię i nazwisko lub nazwa, telefon, e-mail, oznaczenie głównego kontaktu (CRM-008) |
| VO | `CustomerId` | ○ | T-03 |
| VO | `TaxIdentifier` | ○ | NIP, opcjonalny (CRM-007) |
| VO | `EmailAddress` | ○ | |
| VO | `PhoneNumber` | ○ | |

| Komenda | Role | Story | Zdarzenie | Stan |
|---|---|---|---|---|
| `CreateCustomer` | `Salesperson`, `SalesManager`, `Admin` | CRM-007 | `CustomerCreated` | ○ |
| `AddContactPerson` | `Salesperson`, `SalesManager`, `Admin` | CRM-008 | `ContactPersonAdded` | ○ |

**Zapytania:** `GetCustomer360Query` ○ (CRM-009) — dane klienta, osoby kontaktowe, leady, szanse, kontakty, notatki
i zamówienia, od najnowszych; łączy dane kilku kontekstów (T-06).

**Reguły:** klient musi mieć nazwę; może mieć wiele osób kontaktowych, jedna może być główna; system ostrzega
o potencjalnym duplikacie (Q-02); moment tworzenia klienta z leada — Q-01.

## Sales Pipeline

**Odpowiedzialność:** leady, kwalifikacja, szanse sprzedaży i etapy pipeline.
**Role:** `Salesperson` (własne dane), `SalesManager` (zespół — Q-13), `Admin`.
**Backlog:** CRM-001–006, CRM-014–017. **Folder w kodzie:** `Leads` (istnieje), `Opportunities` (proponowany).

### Agregat `Lead` ◐

Kod: `Backend/src/SDC.CRM.Domain/Leads/Lead.cs`. Obecne dane: `CompanyName`, `ContactName`, `ContactEmail`,
`ContactPhone`, `Source`, `Status`, `RejectionReason`, `AssignedSalespersonId`, `CreatedAtUtc`.

| Element | Nazwa | Stan | Uwagi |
|---|---|---|---|
| VO | `LeadStatus` | ✔ | `New`, `Qualified`, `Rejected`; `InContact` („W kontakcie”) — Q-03 |
| VO | `EmailAddress` | ✔ | w kodzie `Email` (T-07); obecnie wymagany — Q-03 |
| VO | `RejectionReason` | ◐ | `string` w agregacie (T-07) |
| VO | `LeadSource` | ◐ | `string`; słownik źródeł — CRM-036, Q-19 |
| VO | `PhoneNumber` | ◐ | `string` (T-07) |
| VO | `LeadPriority` | ○ | niski, normalny, wysoki (słownik §3, CRM-001) |
| VO | `LeadId` | ◐ | `Guid` (T-03) |

| Komenda | Role | Story | Zdarzenie | Stan |
|---|---|---|---|---|
| `RegisterLead` | `Salesperson`, `SalesManager`, `Admin` | CRM-001 | `LeadRegistered` | ✔ `POST /api/leads` |
| `AssignLeadToSalesperson` | `SalesManager`, `Admin` | CRM-004 | `LeadAssigned` | ○ |
| `QualifyLead` | `Salesperson`, `SalesManager`, `Admin` | CRM-005 | `LeadQualified` | ◐ tylko `Lead.Qualify()` |
| `RejectLead` | `Salesperson`, `SalesManager`, `Admin` | CRM-006 | `LeadRejected` | ◐ tylko `Lead.Reject(reason)` |

**Zapytania:** `GetMyLeadsQuery` ✔ (`GET /api/leads/mine`, CRM-002; bez filtrowania i sortowania),
`GetLeadDetailsQuery` ○ (CRM-003).

**Reguły:**

- ✔ lead ma nazwę firmy, osobę kontaktową i właściciela; właściciel pochodzi z tokena, nie z żądania,
- ✔ odrzucony lead nie może zostać zakwalifikowany; odrzucenie wymaga powodu,
- ○ temat/nazwa leada, priorytet, co najmniej telefon albo e-mail (kod wymaga e-maila) — Q-03,
- ○ ostrzeżenie o potencjalnym duplikacie klienta — Q-02,
- ○ zmiana właściciela widoczna w historii; poprzedni właściciel traci dostęp bez uprawnień zespołowych (CRM-004).

### Agregat `Opportunity` ○

| Element | Nazwa | Stan | Uwagi |
|---|---|---|---|
| Encja | `StageHistoryEntry` | ○ | historia zmian etapu (CRM-015) |
| VO | `OpportunityId` | ○ | T-03 |
| VO | `PipelineStage` | ○ | Nowa szansa, Kontakt nawiązany, Oferta wysłana, Negocjacje, Wygrana, Przegrana (słownik §6.2, Q-19) |
| VO | `Money` | ○ | wartość szansy |
| VO | `Probability` | ○ | ustawiane ręcznie lub wynikające z etapu (słownik §3) |
| VO | `LostReason` | ○ | np. cena, konkurencja, brak decyzji, brak budżetu |

Referencje: `LeadId`, `CustomerId`, właściciel (identyfikator handlowca).

| Komenda | Role | Story | Zdarzenie | Stan |
|---|---|---|---|---|
| `CreateOpportunityFromLead` | `Salesperson`, `SalesManager`, `Admin` | CRM-005, CRM-014 | `OpportunityCreated` | ○ |
| `ChangeOpportunityStage` | `Salesperson`, `SalesManager`, `Admin` | CRM-015 | `OpportunityStageChanged` | ○ |
| `WinOpportunity` | `Salesperson`, `SalesManager`, `Admin` | CRM-016 | `OpportunityWon` | ○ |
| `LoseOpportunity` | `Salesperson`, `SalesManager`, `Admin` | CRM-017 | `OpportunityLost` | ○ |

**Zapytania:** `GetOpportunityPipelineQuery` ○ (widok „Opportunity Pipeline”).

**Reguły:** szansa jest powiązana z leadem i klientem, ma właściciela i pierwszy etap pipeline; przegrana wymaga
`LostReason`; wygranej nie można przywrócić do zwykłego etapu bez specjalnych uprawnień (Q-18); po `LeadQualified`
powstaje szansa (polityka w obrębie kontekstu, CRM-005).

## Sales Activities

**Odpowiedzialność:** kontakty, notatki i follow-upy handlowca; historia relacji z klientem.
**Role:** `Salesperson`, `SalesManager` (podgląd aktywności handlowców), `Admin`. **Backlog:** CRM-010–013.
**Folder w kodzie (proponowany):** `SalesActivities`.

### Agregat `SalesActivity` ○

Jeden agregat z typem — kontakt, notatka (`SalesNote`) albo zaplanowany kontakt (`FollowUp`) — zgodnie ze słownikiem §7 (T-04).

| Element | Nazwa | Stan | Uwagi |
|---|---|---|---|
| VO | `SalesActivityId` | ○ | T-03 |
| VO | `SalesActivityType` | ○ | kontakt, `SalesNote`, `FollowUp` |
| VO | `ContactChannel` | ○ | telefon, e-mail, spotkanie, inny (CRM-011) |
| VO | `NoteContent` | ○ | treść notatki (CRM-010) |
| VO | `DueDate` | ○ | termin follow-upu (CRM-012) |
| VO | `FollowUpStatus` | ○ | zaplanowany, wykonany; „zaległy” wyliczany z terminu (CRM-013) |
| VO | `RelatedTo` | ○ | dokładnie jedno z: `CustomerId`, `LeadId`, `OpportunityId` |

| Komenda | Role | Story | Zdarzenie | Stan |
|---|---|---|---|---|
| `LogSalesActivity` | `Salesperson`, `SalesManager`, `Admin` | CRM-011 | `SalesActivityLogged` | ○ |
| `AddSalesNote` | `Salesperson`, `SalesManager`, `Admin` | CRM-010 | `SalesNoteAdded` | ○ |
| `ScheduleFollowUp` | `Salesperson`, `SalesManager`, `Admin` | CRM-012 | `FollowUpScheduled` | ○ |
| `CompleteFollowUp` | `Salesperson`, `SalesManager`, `Admin` | CRM-013 | `FollowUpCompleted` | ○ |

**Zapytania:** `GetMyFollowUpsQuery` ○ (CRM-013: dzisiejsze, przyszłe, zaległe); historia aktywności w
`GetLeadDetailsQuery` i `GetCustomer360Query`.

**Reguły:** kontakt ma typ, datę i autora; notatka ma treść, autora i datę; follow-up ma termin i właściciela;
notatek nie usuwa się fizycznie (Q-17). Wynik kontaktu nie zmienia pipeline automatycznie — handlowiec wykonuje
komendę w `Sales Pipeline`.

## Order Capture

**Odpowiedzialność:** zamówienie handlowca od szkicu do przekazania do backoffice oraz poprawki po zwrocie.
**Role:** `Salesperson`, `SalesManager`, `Admin`; podgląd statusu — wszystkie role (`doc/02` §3). **Backlog:** CRM-018–021.
**Folder w kodzie (proponowany):** `SalesOrders`.

### Agregat `SalesOrder` ○

| Element | Nazwa | Stan | Uwagi |
|---|---|---|---|
| Encja | `SalesOrderLine` | ○ | zakres danych pozycji — Q-05 |
| VO | `SalesOrderId` | ○ | T-03 |
| VO | `OrderNumber` | ○ | numer zamówienia |
| VO | `SalesOrderStatus` | ○ | `Draft` (Robocze), `SubmittedToBackoffice` (Przekazane do backoffice), `ReturnedToSales` (Zwrócone do handlowca) |
| VO | `OrderCustomerSnapshot` | ○ | dane klienta z chwili utworzenia (CRM-018) |
| VO | `Money` | ○ | wartości pozycji i zamówienia |

Referencje: `OpportunityId`, `CustomerId`, właściciel (identyfikator handlowca).

| Komenda | Role | Story | Zdarzenie | Stan |
|---|---|---|---|---|
| `CreateOrderFromOpportunity` | `Salesperson`, `SalesManager`, `Admin` | CRM-018 | `SalesOrderCreated` | ○ |
| `AddOrderLine` | `Salesperson`, `SalesManager`, `Admin` | CRM-018, CRM-019 | — | ○ edycja szkicu; pełna lista komend po Q-05 |
| `SubmitOrderToBackoffice` | `Salesperson`, `SalesManager`, `Admin` | CRM-020, CRM-026 | `SalesOrderSubmittedToBackoffice` | ○ |
| `ReopenSalesOrderAfterReturn` | polityka systemowa | CRM-026 | — | ○ reakcja na zwrot z backoffice |

**Polityka:** po `BackofficeOrderReturnedToSalesIntegrationEvent` komenda `ReopenSalesOrderAfterReturn` przełącza
zamówienie w `ReturnedToSales` i znów pozwala je edytować; ponowne `SubmitOrderToBackoffice` wznawia obsługę (Q-08).
Diagram stanów: [`order_status_lifecycle.md`](order_status_lifecycle.md).

**Zapytania:** `GetSalesOrderStatusQuery` ○ (CRM-021: status z backoffice, data ostatniej zmiany, komentarze widoczne
dla sprzedaży, informacja o wymaganej reakcji).

**Reguły:** zamówienie powstaje z wygranej szansy — sprawdzane w handlerze zapytaniem do `Sales Pipeline` (Q-04);
szkic można zapisać bez przekazania; niekompletnego zamówienia nie można przekazać, a lista braków jest widoczna
wcześniej (CRM-019, Q-05); po przekazaniu handlowiec nie może dowolnie edytować zamówienia — edycja jest możliwa
ponownie tylko po zwrocie.

## Order Backoffice

**Odpowiedzialność:** obsługa przekazanych zamówień: kolejka, przydział, weryfikacja, statusy realizacji, zwroty, zamknięcie.
**Role:** `BackofficeUser` (przypisanie do siebie, statusy, komentarze, zwrot, zamknięcie), `BackofficeManager`
(także przydział pracownikom), `Admin`. **Backlog:** CRM-022–027. **Folder w kodzie (proponowany):** `BackofficeOrders`.

### Agregat `BackofficeOrderCase` ○

| Element | Nazwa | Stan | Uwagi |
|---|---|---|---|
| Encja | `CaseHistoryEntry` | ○ | historia statusów, przypisań i zwrotów (CRM-023, CRM-024, CRM-026) |
| Encja | `BackofficeComment` | ○ | autor, data, widoczność: wewnętrzny albo dla sprzedaży (CRM-025) |
| VO | `BackofficeOrderCaseId` | ○ | T-03 |
| VO | `BackofficeOrderStatus` | ○ | `New`, `InVerification`, `InFulfillment`, `AwaitingInformation`, `Blocked`, `ReturnedToSales`, `Completed`, `Cancelled` (słownik §6.3) |
| VO | `BlockingReason` | ○ | wymagany przy `Blocked` |
| VO | `ReturnToSalesComment` | ○ | wymagany przy zwrocie |
| VO | `CompletionDate` | ○ | zapisywana przy `Completed` |
| VO | `CancellationReason` | ❓ | Q-07 |
| VO | `SubmittedOrderSnapshot` | ○ | dane przekazanego zamówienia potrzebne do obsługi |

Referencje: `SalesOrderId`, przypisany pracownik backoffice.

| Komenda | Role | Story | Zdarzenie | Stan |
|---|---|---|---|---|
| `OpenBackofficeOrderCase` | polityka systemowa | CRM-020, CRM-022 | `BackofficeOrderCaseOpened` | ○ |
| `AssignBackofficeOrder` | `BackofficeManager`, `Admin`; `BackofficeUser` — do siebie | CRM-023 | `BackofficeOrderAssigned` | ○ |
| `ChangeBackofficeOrderStatus` | `BackofficeUser`, `BackofficeManager`, `Admin` | CRM-024 | `BackofficeOrderStatusChanged`, `BackofficeOrderBlocked` | ○ |
| `AddBackofficeComment` | `BackofficeUser`, `BackofficeManager`, `Admin` | CRM-025 | `BackofficeCommentAdded` | ○ |
| `ReturnOrderToSales` | `BackofficeUser`, `BackofficeManager`, `Admin` | CRM-026 | `BackofficeOrderReturnedToSales` | ○ |
| `CompleteOrder` | `BackofficeUser`, `BackofficeManager`, `Admin` | CRM-027 | `BackofficeOrderCompleted` | ○ |
| `CancelOrder` | do decyzji | — | `BackofficeOrderCancelled` | ❓ Q-07 |

`ChangeBackofficeOrderStatus` emituje `BackofficeOrderBlocked` przy przejściu do `Blocked` (z `BlockingReason`),
a `BackofficeOrderStatusChanged` przy pozostałych przejściach. `OpenBackofficeOrderCase` przy ponownym przekazaniu
wznawia istniejącą sprawę (Q-08).

**Zapytania:** `GetBackofficeQueueQuery` ○ (CRM-022), `GetBackofficeOrderDetailsQuery` ○.

**Reguły:** przejścia statusów zgodne z dozwolonym workflow (Q-06, [`order_status_lifecycle.md`](order_status_lifecycle.md));
blokada wymaga powodu; zwrot wymaga komentarza; zakończenie zapisuje datę zakończenia; każda zmiana statusu
i przypisania trafia do historii; komentarze nie znikają po zmianie statusu; backoffice nie zmienia pipeline ani danych `SalesOrder`.

## Reporting & KPI

**Odpowiedzialność:** read modele i projekcje dla dashboardów i raportów. **Backlog:** CRM-031–034 (Q-15).

| Read model | Story | Zawartość | Role | Stan |
|---|---|---|---|---|
| `SalespersonDashboard` | CRM-031 | aktywne leady i szanse, dzisiejsze i zaległe follow-upy, zamówienia zwrócone do poprawy | `Salesperson`, `SalesManager`, `Admin` | ○ |
| `SalesManagerDashboard` | CRM-032 | szanse na etapach, wartość pipeline, wygrane i przegrane, aktywność handlowców; filtry: handlowiec, okres | `SalesManager`, `Admin` | ○ |
| `BackofficeReport` | CRM-033 | zamówienia według statusów, zablokowane, zwrócone, średni czas obsługi; filtry: pracownik, status | `BackofficeManager`, `Admin`; `BackofficeUser` — ograniczony | ○ |
| `ReportExport` | CRM-034 | eksport CSV/XLSX z zachowaniem filtrów i uprawnień | `SalesManager`, `BackofficeManager`, `Admin` | ○ (Could Have) |

Zasilanie read modeli zdarzeniami: [`events_catalog.md`](events_catalog.md#zasilanie-read-modeli). Do czasu wdrożenia
publikacji zdarzeń (T-02) proste widoki czytają dane modułów bezpośrednio (T-06).

## Identity & Access

**Odpowiedzialność:** tożsamość użytkownika, role, uprawnienia i audyt. **Backlog:** CRM-028 (Review),
CRM-029 (In Progress), CRM-030, CRM-035.

| Komponent | Stan | Kod / miejsce | Uwagi |
|---|---|---|---|
| Dostawca tożsamości | ✔ | `Integrations/Sso` | SimpleIdServer: konta, grupy i role — źródło prawdy dla pojęć `User` i `Role` ze słownika (Q-12) |
| `ICurrentUser` | ✔ | `Backend/src/SDC.CRM.Application/Abstractions/Identity/ICurrentUser.cs` | identyfikator (UUID wyliczony z `sub`), nazwa, e-mail, role z tokena |
| `CrmRoles` | ✔ | `Backend/src/SDC.CRM.Api/Authorization/CrmRoles.cs` | `Salesperson`, `SalesManager`, `BackofficeUser`, `BackofficeManager`, `Admin` |
| `CrmPolicies` | ◐ | `Backend/src/SDC.CRM.Api/Authorization/CrmPolicies.cs` | `leads:register`, `leads:view-own`; kolejne polityki per macierz `doc/02` §3 |
| `AuditLog` | ○ | — | CRM-030: użytkownik, czas, akcja, identyfikator obiektu; zasilany zdarzeniami audytowanymi (T-05) |

CRM nie ma własnych agregatów `User` ani `Role`. Zespoły sprzedaży — Q-13.

## Integrations

**Odpowiedzialność:** warstwa antykorupcyjna do ERP, fakturowania, e-maila, kalendarza i płatności.
**Backlog:** CRM-037, CRM-038 (Could Have), Q-10, Q-11. Obecnie w repozytorium istnieje tylko środowisko SSO
(`Integrations/Sso`), które należy do `Identity & Access`.

### Agregat `IntegrationJob` ○

| Element | Nazwa | Stan | Uwagi |
|---|---|---|---|
| Encja | `IntegrationAttempt` | ○ | pojedyncza próba wysyłki, wynik, błąd bez danych wrażliwych |
| VO | `IntegrationJobStatus` | ○ | `Pending`, `Succeeded`, `Failed` |
| VO | `ExternalSystemName` | ○ | np. ERP |

| Komenda | Role | Story | Zdarzenie | Stan |
|---|---|---|---|---|
| `ExportOrderToErp` | polityka systemowa | CRM-038 | `IntegrationJobSucceeded`, `IntegrationJobFailed` | ○ |
| `RetryIntegrationJob` | `BackofficeManager`, `Admin` (Q-10) | CRM-038 | `IntegrationJobSucceeded`, `IntegrationJobFailed` | ○ |

**Reguły:** liczba automatycznych ponowień jest ograniczona — po jej wyczerpaniu błąd jest widoczny dla uprawnionych
(CRM-038, Q-10); DTO systemów zewnętrznych nie trafiają do domeny.

### Agregat `ExternalSystemMapping` ○

Mapowanie identyfikatorów CRM na identyfikatory systemu zewnętrznego (VO `ExternalId`). Identyfikator CRM ma
co najwyżej jeden identyfikator w danym systemie zewnętrznym.

| Komenda | Role | Story | Zdarzenie | Stan |
|---|---|---|---|---|
| `RecordExternalId` | polityka systemowa | CRM-038 | — | ○ po udanej wysyłce |

Informacje zwrotne z ERP (faktura, płatność) oraz integracje e-mail i kalendarza nie są częścią modelu do czasu
decyzji (Q-10, Q-11).

## Obszar wspierający: Administration

Konfiguracja słowników systemowych — źródła leadów, powody przegranej, priorytety, statusy, jeśli będą konfigurowalne
(CRM-036, Could Have). Kandydat na agregat: `Configuration` ○. Do czasu decyzji Q-19 wartości są stałe (słownik §3, §6).
Wartości używanych przez istniejące dane nie wolno usuwać — tylko dezaktywować.

