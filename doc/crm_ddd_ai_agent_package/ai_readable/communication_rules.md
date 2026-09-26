# Zasady komunikacji

## Stan obecny (2026-09-26)

- Backend działa jako jeden proces; klienci (web, mobile) wołają REST API z tokenem Bearer.
- Agregaty rejestrują zdarzenia w `Entity.DomainEvents` (`LeadRegistered`, `LeadQualified`, `LeadRejected`),
  ale **nic ich nie publikuje** — `UnitOfWork` wywołuje tylko `SaveChangesAsync`. Brak dispatchera, brokera i outboxa (T-02).
- Kontener RabbitMQ jest w `docker-compose.yml`, ale aplikacja go nie używa.
- Każde żądanie ma identyfikator korelacji `X-Correlation-ID` (`CorrelationIdMiddleware`), który trafia do logów
  i śladów OpenTelemetry — tak samo ma być przenoszony w komunikatach po wdrożeniu brokera.

## Docelowo

| Mechanizm | Kiedy | Przykłady |
|---|---|---|
| Komenda synchroniczna | zmiana stanu **w obrębie** jednego kontekstu, na żądanie użytkownika | `RegisterLead`, `SubmitOrderToBackoffice`, `ReturnOrderToSales` |
| Zapytanie synchroniczne | odczyt danych innego kontekstu bez zmiany jego stanu: snapshot, weryfikacja warunku, widok łączony | snapshot klienta dla `CreateOrderFromOpportunity`, „czy szansa wygrana”, `GetCustomer360Query`, `GetSalesOrderStatusQuery` |
| Zdarzenie domenowe | fakt zarejestrowany przez agregat; obsługiwany w tym samym kontekście (polityka) albo zamieniany na zdarzenie integracyjne | `LeadQualified` → polityka `CreateOpportunityFromLead` |
| Zdarzenie integracyjne | zmiana procesu **między** kontekstami, asynchronicznie | `SalesOrderSubmittedIntegrationEvent`, `BackofficeOrderReturnedToSalesIntegrationEvent`, `BackofficeOrderCompletedIntegrationEvent` |
| Projekcja | aktualizacja read modeli i audytu ze zdarzeń | dashboardy CRM-031–033, `AuditLog` (CRM-030) |
| Autoryzacja | każde żądanie; role z tokena SSO | `ICurrentUser`, polityki `CrmPolicies` |

## Reguły

1. Agregat tylko **rejestruje** zdarzenia domenowe. Zdarzenia integracyjne publikuje warstwa aplikacji po zapisie
   transakcji (docelowo przez outbox), nigdy agregat.
2. Najpierw broker in-memory za abstrakcją; RabbitMQ później, bez zmian w domenie i aplikacji
   (`.github/instructions/50-event-driven-messaging.instructions.md`). Typy RabbitMQ nie pojawiają się w domenie ani aplikacji.
3. Kontrakty zdarzeń integracyjnych są stabilne i niezależne od implementacji; konsumenci są idempotentni.
4. Komunikat niesie identyfikator korelacji i przyczynowości (`correlation id`, `causation id`), bez danych wrażliwych klientów.
5. Jedna transakcja zmienia jeden agregat. Reakcję na zdarzenie innego agregatu (także w tym samym kontekście)
   wykonuje osobna komenda polityki — np. `LeadQualified` → `CreateOpportunityFromLead`.
6. Zapytania między kontekstami idą przez publiczne API modułu (kontrakty/DTO), nigdy przez encje EF Core innego modułu.
7. Agregaty przechowują tylko identyfikatory innych agregatów (`CustomerId`, `LeadId`, `OpportunityId`, `SalesOrderId`,
   `BackofficeOrderCaseId`); snapshot danych — tylko gdy jest biznesowo wymagany (`OrderCustomerSnapshot`, `SubmittedOrderSnapshot`).
8. Wynik kontaktu handlowca nie zmienia pipeline automatycznie — handlowiec wykonuje komendę w `Sales Pipeline`.

## Przepływy między kontekstami

| Zdarzenie integracyjne | Publikuje | Odbiorca → reakcja |
|---|---|---|
| `SalesOrderSubmittedIntegrationEvent` | Order Capture | Order Backoffice → `OpenBackofficeOrderCase` (lub wznowienie sprawy, Q-08); Reporting & KPI → projekcje |
| `BackofficeOrderReturnedToSalesIntegrationEvent` | Order Backoffice | Order Capture → `ReopenSalesOrderAfterReturn`; Reporting & KPI → projekcje; Sales Activities → `FollowUp` tylko po decyzji Q-09 |
| `BackofficeOrderCompletedIntegrationEvent` | Order Backoffice | Integrations → `ExportOrderToErp` (Could Have, Q-10); Reporting & KPI → projekcje |
| `BackofficeOrderCancelledIntegrationEvent` | Order Backoffice | Order Capture, Reporting & KPI — po decyzji Q-07 |

Pełny katalog: [`events_catalog.md`](events_catalog.md).

