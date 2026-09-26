# Prompt dla Cursor / GitHub Copilot / Claude

Wklej poniższy tekst jako kontekst zadania dotyczącego modelu domeny SDC-CRM.

```text
Pracujesz nad SDC-CRM — modularnym monolitem .NET 10 (Clean Architecture, DDD, CQRS, vertical slices)
z klientami Angular (Frontend/Web) i .NET MAUI (Frontend/Mobile).

Źródła prawdy (w tej kolejności): kod w Backend/src dla elementów zaimplementowanych, doc/01 (słownik),
doc/03 (backlog), doc/02 (role i uprawnienia), .github/copilot-instructions.md i .github/instructions/*,
a model zbiorczy w doc/crm_ddd_ai_agent_package.

Przed zmianą przeczytaj:
- doc/crm_ddd_ai_agent_package/AI_AGENT_CONTEXT.md — stan projektu i granice,
- ai_readable/bounded_contexts_and_aggregates.json — agregaty, komendy, reguły, stan implementacji,
- ai_readable/events_catalog.json — zdarzenia domenowe i integracyjne,
- ai_readable/processes/processes.json — kroki procesów,
- ai_readable/naming_decisions.md — nazwy kanoniczne i zakazane aliasy,
- ai_readable/open_questions.md — pytania biznesowe Q-xx i decyzje techniczne T-xx.

Zasady:
1. Nie mieszaj pipeline sprzedaży z obsługą backoffice: SalesOrder i BackofficeOrderCase to osobne agregaty.
2. Inne konteksty używają CustomerId i snapshotów (OrderCustomerSnapshot), nie wspólnej encji Customer.
3. Między kontekstami używaj zdarzeń integracyjnych (<fakt>IntegrationEvent) publikowanych przez warstwę aplikacji;
   agregat tylko rejestruje zdarzenia domenowe.
4. Reporting & KPI buduj jako read modele / projekcje; integracje zewnętrzne tylko przez ACL.
5. Konta i role pochodzą z SSO (SimpleIdServer); nie twórz agregatów User ani Role; zawsze autoryzuj w backendzie.
6. Używaj wyłącznie nazw kanonicznych (np. BackofficeOrderCase, RegisterLead, SalesOrderSubmittedToBackoffice);
   aliasy wymienione w naming_decisions.md są zakazane.
7. Dodawaj kod w istniejących projektach SDC.CRM.* w folderach obszaru i przypadku użycia
   (wzór: Application/Leads/RegisterLead); nie twórz Modules/ ani BuildingBlocks/ bez ADR (T-01).
8. Elementy oznaczone Q-xx realizuj według założenia roboczego z open_questions.md
   i dodaj komentarz "// TODO Business decision (Q-xx): ...". Nie wymyślaj nowych reguł biznesowych.
9. Pracuj w TDD (TUnit + NSubstitute), zaczynając od testów domeny i handlera.
10. Po zmianie modelu zaktualizuj .md i .json razem, zrenderuj diagramy
    (tools/render-diagrams.ps1) i uruchom tools/validate-docs.ps1.
```


