# Otwarte pytania biznesowe i decyzje techniczne

Elementów oznaczonych `Q-xx` nie implementuj „na wyczucie”. Jeżeli story wymaga takiego elementu, zaimplementuj
**założenie robocze** (najprostsze zachowanie wspierające główny workflow) i dodaj w kodzie komentarz:

```csharp
// TODO Business decision (Q-07): Can backoffice cancel an order?
```

Po rozstrzygnięciu zaktualizuj ten plik, model (`.md` + `.json`) i diagramy.

## Pytania biznesowe

| ID | Pytanie | Założenie robocze | Dotyczy |
|---|---|---|---|
| Q-01 | Czy lead może istnieć bez klienta i kiedy powstaje `Customer` — przy rejestracji leada, przy kwalifikacji czy ręcznie? | Lead przechowuje minimalne dane firmy i osoby kontaktowej (jak obecny kod). Najpóźniej przy `CreateOpportunityFromLead` handlowiec wybiera istniejącego klienta albo tworzy nowego z danych leada, bo szansa musi mieć `CustomerId` (CRM-014). | CRM-001, CRM-005, CRM-007, CRM-014 |
| Q-02 | Po czym rozpoznajemy potencjalny duplikat klienta (NIP, nazwa, e-mail, telefon)? Czy handlowiec widzi, że klient istnieje, bez dostępu do jego danych (`doc/02` §4.1)? | Tylko ostrzeżenie (bez blokady) przy zgodnym `TaxIdentifier` albo adresie e-mail; bez ujawniania danych klientów innych handlowców. | CRM-001, CRM-007 |
| Q-03 | Jakie dane leada są wymagane? Backlog: temat/nazwa, priorytet, „telefon albo e-mail”. Kod: nazwa firmy, osoba kontaktowa i wymagany e-mail. Czy status „W kontakcie” (`doc/01` §6.1) jest potrzebny w MVP? | Obowiązuje backlog — kod do dostosowania w ramach CRM-001 (In Progress). Status `InContact` nie jest implementowany do czasu decyzji. | CRM-001, CRM-002 |
| Q-04 | Czy zamówienie można utworzyć bez wygranej szansy? | Nie — `CreateOrderFromOpportunity` wymaga szansy w stanie wygranym (CRM-016, CRM-018). | CRM-018 |
| Q-05 | Jakie dane są wymagane do przekazania zamówienia do backoffice i jaki jest zakres pozycji (`SalesOrderLine`) oraz warunków? | Lista pól do ustalenia; reguła kompletności żyje w agregacie `SalesOrder` i zwraca listę braków (CRM-019). | CRM-018, CRM-019, CRM-020 |
| Q-06 | Jakie przejścia statusów `BackofficeOrderCase` są dozwolone? | Propozycja w `order_status_lifecycle.md` (diagram 09) — do potwierdzenia. | CRM-024 |
| Q-07 | Czy backoffice może anulować zamówienie? Kto jeszcze (np. handlowiec dla szkicu) i z jakich statusów? | Brak anulowania do czasu decyzji; status „Anulowane” istnieje w słowniku, komenda `CancelOrder` nie jest implementowana. | słownik §5, §6.3 |
| Q-08 | Ponowne przekazanie po zwrocie: ta sama sprawa backoffice (wznowienie z historią) czy nowa? Czy przypisany pracownik zostaje? | Ta sama sprawa (`BackofficeOrderCaseId` bez zmian), status wraca do `New`, historia zachowana; przypisanie do decyzji. | CRM-026 |
| Q-09 | Czy zwrot zamówienia do handlowca ma automatycznie tworzyć `FollowUp`? | Nie — handlowiec widzi zwrócone zamówienia na dashboardzie (CRM-031) i w statusie zamówienia (CRM-021). | CRM-021, CRM-026, CRM-031 |
| Q-10 | Integracja ERP/fakturowania: moment wysyłki (po `BackofficeOrderCompleted` czy po osobnym zatwierdzeniu do fakturowania), kierunek (czy wracają informacje o fakturze i płatności), limit ponowień oraz kto widzi błędy i może ponowić wysyłkę (macierz uprawnień nie obejmuje integracji)? | Jednokierunkowa wysyłka po `BackofficeOrderCompleted`; limit ponowień konfigurowalny; błędy i ponowienie dla `BackofficeManager` i `Admin`. | CRM-037, CRM-038 |
| Q-11 | Czy integracje e-mail, kalendarz i płatności są w zakresie MVP? | Poza MVP (brak stories w backlogu; brief: „płatności jeśli potrzebne”). | brief `doc/CRM.md` |
| Q-12 | Zarządzanie użytkownikami i rolami (CRM-035): w panelu SSO czy w CRM przez API administracyjne dostawcy tożsamości? | Panel i skrypty SimpleIdServer (`Integrations/Sso`) — stan obecny; CRM nie przechowuje kont ani ról. | CRM-029, CRM-035 |
| Q-13 | Czy menedżer sprzedaży widzi tylko swój zespół? Czy istnieje hierarchia zespołów (`doc/02` §4.2)? | Jeden zespół — menedżer widzi dane wszystkich handlowców; pojęcie zespołu dopiero po decyzji. | CRM-004, CRM-032 |
| Q-14 | Kim jest „realizacja” z briefu („komunikacja z realizacją”): zespół wewnętrzny, system zewnętrzny (ERP)? | Poza systemem; backoffice odnotowuje postęp statusami `InFulfillment` i `Completed`. | CRM-024, CRM-027 |
| Q-15 | Które raporty są wymagane w MVP? | Najpierw story Must Have; dashboardy CRM-031–033 (Should Have) po nich, eksport CRM-034 (Could Have) na końcu. | CRM-031–034 |
| Q-16 | Które procesy mają być dostępne w aplikacji mobilnej w MVP i czy mają działać offline? | Mobile obsługuje szybkie zadania handlowca (moje leady, rejestracja leada, notatka, follow-up) w trybie online z czytelnym komunikatem offline — jak obecnie. | CRM-001, CRM-002, CRM-010, CRM-012 |
| Q-17 | Kto może edytować lub usuwać notatki (CRM-010: „nie można usunąć bez odpowiednich uprawnień”)? | Brak edycji i usuwania w MVP; notatek nigdy nie usuwa się fizycznie. | CRM-010 |
| Q-18 | Kto może przywrócić wygraną szansę do zwykłego etapu (CRM-016: „bez specjalnych uprawnień”)? | Niedozwolone do czasu decyzji. | CRM-016 |
| Q-19 | Czy etapy pipeline, źródła, priorytety i powody przegranej mają być konfigurowalne (CRM-036)? | Stałe wartości ze słownika (`doc/01` §3, §6.2) do czasu realizacji CRM-036. | CRM-015, CRM-036 |

## Decyzje techniczne (kandydaci na ADR)

| ID | Temat | Stan obecny | Zalecenie do czasu ADR |
|---|---|---|---|
| T-01 | Podział modułów w kodzie | Jeden zestaw projektów warstwowych `SDC.CRM.{Api,Application,Domain,Infrastructure}` z folderami per obszar (`Leads`). | Kontynuuj ten wzorzec (folder kontekstu/agregatu w każdej warstwie, np. `SalesOrders/SubmitOrderToBackoffice/`). Nie twórz `Modules/` ani `BuildingBlocks/` bez ADR. |
| T-02 | Publikacja zdarzeń | Agregaty rejestrują zdarzenia w `Entity.DomainEvents`, ale nic ich nie publikuje (`UnitOfWork` wywołuje tylko `SaveChangesAsync`). Kontener RabbitMQ jest dostępny, nieużywany. | Dispatcher zdarzeń po zapisie + broker in-memory za abstrakcją (`50-event-driven-messaging`); outbox przy przejściu na RabbitMQ. Zdarzenia integracyjne publikuje warstwa aplikacji, nie agregat. |
| T-03 | Typy identyfikatorów | `Guid` (`Entity.Id`); właściciel leada = UUID v3 wyliczany z `sub` tokena (`CurrentUser.DeriveDeterministicGuid`). | Silne typy (`LeadId`, `CustomerId`, …) przy nowych agregatach; decyzja wspólna dla całego modelu. |
| T-04 | `SalesActivity` jako jeden agregat | Słownik §7 i backlog: jeden agregat (kontakt, notatka, zadanie). | Jeden agregat z typem; osobne agregaty `SalesNote` / `FollowUp` tylko po ADR. |
| T-05 | Dziennik audytu (CRM-030) | Brak. | Zapis audytu zasilany zdarzeniami audytowanymi (`events_catalog.md`); Event Sourcing tylko dla `BackofficeOrderCase`, jeśli uzasadni to ADR (`70-event-sourcing`). |
| T-06 | Read modele | `GetMyLeadsQuery` czyta bezpośrednio przez `ILeadRepository`. | Proste listy — zapytania bezpośrednie; dashboardy i widoki łączące moduły — projekcje ze zdarzeń po wdrożeniu T-02. |
| T-07 | Value objecty leada | `Email` jako VO; telefon, źródło i powód odrzucenia jako `string`. | Nazwa `EmailAddress` oraz VO `PhoneNumber`, `LeadSource`, `RejectionReason` przy najbliższej zmianie agregatu `Lead` (z migracją, jeśli zmienia się schemat). |

