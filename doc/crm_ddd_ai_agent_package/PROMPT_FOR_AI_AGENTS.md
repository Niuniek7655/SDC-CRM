# Prompt dla Cursor / GitHub Copilot / Claude AI

Użyj poniższego kontekstu przy implementacji CRM zgodnie z DDD i modularnym monolitem.

## Zadanie

Projektuj system CRM na podstawie bounded contextów, agregatów, encji, value objectów i procesów opisanych w tej paczce. Nie traktuj diagramów jako prostego CRUD. Zachowaj granice domenowe.

## Priorytety

1. Nie mieszaj pipeline sprzedażowego z procesem backoffice.
2. Nie twórz jednej globalnej encji `Customer` używanej mutowalnie przez wszystkie moduły.
3. Inne contexty mają używać `CustomerId` i snapshotów, np. `OrderCustomerSnapshot`.
4. Używaj eventów integracyjnych między bounded contextami.
5. Reporting buduj jako projekcje/read modele aktualizowane zdarzeniami.
6. W module Backoffice osobno modeluj `OrderProcess`, a w module Orders osobno `SalesOrder`.

## Foldery referencyjne

- `ai_readable/bounded_contexts_and_aggregates.json`
- `ai_readable/events_catalog.json`
- `ai_readable/processes/processes.json`
- `ai_readable/context_map.mmd`

## Główna architektura

Modularny monolit `.NET` z modułami:

```text
Customers
Sales
Activities
Orders
Backoffice
Integrations
Reporting
IdentityAccess
```

Każdy moduł może mieć warstwy:

```text
Domain
Application
Infrastructure
```

Eventy między modułami powinny przechodzić przez internal event bus/outbox.
