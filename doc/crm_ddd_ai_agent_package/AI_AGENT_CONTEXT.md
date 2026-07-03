# AI Agent Context — CRM DDD

Ten plik jest przygotowany jako szybki kontekst dla Cursor, GitHub Copilot, Claude AI lub innego agenta AI pracującego nad implementacją CRM.

## Cel systemu

System CRM dla około 20 handlowców i działu backoffice. Handlowcy zapisują klientów, leady, aktywności, notatki i wprowadzają zamówienia po finalizacji sprzedaży. Backoffice obsługuje zamówienia, sprawdza kompletność danych, zarządza statusem realizacji i komunikuje się z realizacją.

## Główne bounded contexty

1. `Customer Management` — źródło prawdy dla danych klienta.
2. `Lead & Pipeline` — leady, opportunity, pipeline sprzedażowy.
3. `Sales Activity` — kontakty, follow-upy, aktywności i notatki.
4. `Sales Order Capture` — utworzenie i przekazanie zamówienia przez handlowca.
5. `Backoffice Order Processing` — obsługa i realizacja zamówienia.
6. `Integration Context` — ACL dla ERP, fakturowania, e-maila, kalendarza i płatności.
7. `Reporting & Analytics` — read modele/projekcje.
8. `Identity & Access` — użytkownicy, role, uprawnienia i zespoły.

## Najważniejsza zasada projektowa

`Sales Order` i `Order Process` to nie jest ten sam agregat.

- `SalesOrder` odpowiada na pytanie: co handlowiec sprzedał i na jakich warunkach?
- `OrderProcess` odpowiada na pytanie: jak backoffice obsługuje i realizuje to zamówienie?

## Preferowana komunikacja

- Wewnątrz bounded contextu: komendy aplikacyjne i transakcje lokalne.
- Między bounded contextami: integration events.
- Do pobierania danych referencyjnych: query/snapshot, np. `SalesOrder` pobiera snapshot danych `Customer`.
- Reporting: aktualizowany asynchronicznie na podstawie zdarzeń.

## Minimalny zestaw modułów dla modularnego monolitu .NET

```text
src/
  CRM.Api/
  Modules/
    Customers/
    Sales/
    Activities/
    Orders/
    Backoffice/
    Integrations/
    Reporting/
    IdentityAccess/
  BuildingBlocks/
    Domain/
    Application/
    Infrastructure/
    EventBus/
```

## Event flow główny

```text
LeadQualified
  -> OpportunityCreated
  -> OpportunityWon
  -> SalesOrderCreated
  -> SalesOrderSubmitted
  -> OrderProcessingStarted
  -> OrderAcceptedForFulfillment
  -> InvoiceRequested
  -> InvoiceIssued
  -> PaymentReceived
  -> OrderCompleted
```

## Pliki szczegółowe

- `ai_readable/bounded_contexts_and_aggregates.json`
- `ai_readable/bounded_contexts_and_aggregates.md`
- `ai_readable/events_catalog.md`
- `ai_readable/processes/`
