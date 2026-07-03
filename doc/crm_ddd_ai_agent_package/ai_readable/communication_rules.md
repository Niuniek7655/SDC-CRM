# Reguły komunikacji

## Komendy i zapytania synchroniczne

Stosuj dla:

- komend wewnątrz tego samego bounded contextu,
- pobrania snapshotu danych klienta do `SalesOrder`,
- walidacji użytkownika, roli lub uprawnienia,
- pobierania konfiguracji pipeline,
- odczytu statusu na potrzeby UI.

## Zdarzenia integracyjne asynchroniczne

Stosuj dla zmiany procesu między bounded contextami, np.:

- `OpportunityWon` → `Sales Order Capture` tworzy szkic zamówienia,
- `SalesOrderSubmitted` → `Backoffice Order Processing` tworzy `OrderProcess`,
- `MissingInformationRequested` → `Sales Activity` tworzy `FollowUpTask`,
- `OrderAcceptedForFulfillment` → `Integration Context` tworzy `IntegrationJob`,
- `InvoiceIssued` i `PaymentReceived` → `Reporting & Analytics` aktualizuje projekcje.

## Referencje przez ID

Agregaty nie powinny przechowywać pełnych obiektów z innych contextów. Przechowuj:

- `CustomerId`, `LeadId`, `OpportunityId`, `SalesOrderId`, `OrderProcessId`,
- snapshoty historyczne tylko tam, gdzie są biznesowo wymagane, np. `OrderCustomerSnapshot`.

## Reporting

Reporting & Analytics nie jest właścicielem procesu biznesowego. To context odczytowy oparty o read modele i projekcje aktualizowane ze zdarzeń.
