# Pakiet DDD CRM — model domeny dla ludzi i agentów AI

Model domeny SDC-CRM: bounded contexty, agregaty, zdarzenia, procesy i diagramy, zgodny ze słownikiem, backlogiem,
macierzą uprawnień i aktualnym kodem. Stan implementacji na **2026-09-26** (gałąź `main` po PR #4).

Oznaczenia używane w pakiecie: ✔ zaimplementowane, ◐ częściowo, ○ planowane (backlog), ❓ do decyzji (`Q-xx`).

## Hierarchia źródeł prawdy

Przy rozbieżnościach obowiązuje kolejność:

1. **Kod** w `Backend/src` — dla elementów już zaimplementowanych (np. `RegisterLead`, `LeadRegistered`).
2. **Słownik** [`doc/01-slownik-jezyka-wszechobecnego.md`](../01-slownik-jezyka-wszechobecnego.md) — język domeny, konteksty, statusy.
3. **Backlog** [`doc/03-backlog-user-stories.md`](../03-backlog-user-stories.md) — zakres, kryteria akceptacji, komendy i zdarzenia.
4. **Role** [`doc/02-uzytkownicy-role-uprawnienia.md`](../02-uzytkownicy-role-uprawnienia.md) — role i macierz uprawnień.
5. **Instrukcje** `.github/copilot-instructions.md` i `.github/instructions/*` — zasady architektury i kodu.
6. **Ten pakiet** — model zbiorczy zgodny z 1–5.

Warianty nazw występujące w źródłach 1–5 rozstrzyga [`ai_readable/naming_decisions.md`](ai_readable/naming_decisions.md).
Brief projektu: [`doc/CRM.md`](../CRM.md).

## Zawartość

| Plik | Zawartość |
|---|---|
| [`AI_AGENT_CONTEXT.md`](AI_AGENT_CONTEXT.md) | Najkrótszy kontekst dla agenta AI: stan projektu, granice, reguły. |
| [`PROMPT_FOR_AI_AGENTS.md`](PROMPT_FOR_AI_AGENTS.md) | Gotowy prompt do Cursor / Copilot / Claude. |
| [`ai_readable/system_context.md`](ai_readable/system_context.md) | Architektura i stan implementacji (diagram 00). |
| [`ai_readable/bounded_contexts_and_aggregates.md`](ai_readable/bounded_contexts_and_aggregates.md), [`.json`](ai_readable/bounded_contexts_and_aggregates.json) | Konteksty, agregaty, encje, VO, komendy, zapytania, reguły. |
| [`ai_readable/context_map.md`](ai_readable/context_map.md) | Mapa kontekstów i relacji (diagram 01). |
| [`ai_readable/communication_rules.md`](ai_readable/communication_rules.md) | Komunikacja synchroniczna i zdarzenia — stan obecny i docelowy. |
| [`ai_readable/events_catalog.md`](ai_readable/events_catalog.md), [`.json`](ai_readable/events_catalog.json) | Zdarzenia domenowe i integracyjne, audyt, zasilanie read modeli. |
| [`ai_readable/order_status_lifecycle.md`](ai_readable/order_status_lifecycle.md) | Statusy `SalesOrder` i `BackofficeOrderCase` (diagramy 08–09). |
| [`ai_readable/processes/`](ai_readable/processes/processes.json) | Sześć procesów biznesowych: `process_0N_*.md` i `processes.json` (diagramy 02–07). |
| [`ai_readable/naming_decisions.md`](ai_readable/naming_decisions.md) | Nazwy kanoniczne i zakazane aliasy. |
| [`ai_readable/open_questions.md`](ai_readable/open_questions.md) | Pytania biznesowe `Q-xx` i decyzje techniczne `T-xx`. |
| [`diagrams/`](diagrams/README.md) | Diagramy PNG generowane z bloków Mermaid oraz manifest źródeł. |
| `tools/` | `render-diagrams.ps1`, `validate-docs.ps1`, konfiguracja Mermaid. |

## Diagramy

| Nr | Diagram | Źródło |
|---|---|---|
| 00 | [Kontekst systemu i stan implementacji](diagrams/png/00_system_context.png) | `ai_readable/system_context.md` |
| 01 | [Mapa kontekstów z agregatami, encjami i VO](diagrams/png/01_context_map_with_entities_value_objects.png) | `ai_readable/context_map.md` |
| 02 | [Proces 1 — lead i pipeline](diagrams/png/02_process_01_lead_pipeline.png) | `ai_readable/processes/process_01_lead_pipeline.md` |
| 03 | [Proces 2 — zarządzanie klientem](diagrams/png/03_process_02_customer_management.png) | `ai_readable/processes/process_02_customer_management.md` |
| 04 | [Proces 3 — aktywności handlowca](diagrams/png/04_process_03_sales_activity.png) | `ai_readable/processes/process_03_sales_activity.md` |
| 05 | [Proces 4 — zamówienie handlowca](diagrams/png/05_process_04_sales_finalization_order_capture.png) | `ai_readable/processes/process_04_sales_finalization_order_capture.md` |
| 06 | [Proces 5 — obsługa w backoffice](diagrams/png/06_process_05_backoffice_order_processing.png) | `ai_readable/processes/process_05_backoffice_order_processing.md` |
| 07 | [Proces 6 — integracje, raporty, audyt](diagrams/png/07_process_06_integration_invoice_payment_reporting.png) | `ai_readable/processes/process_06_integration_invoice_payment_reporting.md` |
| 08 | [Statusy `SalesOrder`](diagrams/png/08_sales_order_lifecycle.png) | `ai_readable/order_status_lifecycle.md` |
| 09 | [Statusy `BackofficeOrderCase`](diagrams/png/09_backoffice_order_case_lifecycle.png) | `ai_readable/order_status_lifecycle.md` |

## Najważniejsza granica

Sprzedaż nie zarządza statusem realizacji, a backoffice nie zarządza pipeline sprzedaży:

```text
Lead / Opportunity / kontakt / oferta  !=  weryfikacja / przydział / braki / realizacja / zamknięcie
SalesOrder (Order Capture)             !=  BackofficeOrderCase (Order Backoffice)
```

## Jak aktualizować pakiet

1. Zmieniaj model w pliku `.md` i odpowiadającym mu `.json` w tym samym commicie.
2. Diagram edytuj w bloku ```` ```mermaid ```` poprzedzonym znacznikiem `<!-- diagram: NAZWA -->`, a potem
   wygeneruj PNG (wymaga Node.js; lokalny Chrome lub Edge zostanie użyty automatycznie):

   ```powershell
   ./doc/crm_ddd_ai_agent_package/tools/render-diagrams.ps1
   ```

3. Sprawdź spójność (JSON, odwołania do komend i zdarzeń, zakazane aliasy, linki, pytania `Q-xx`,
   aktualność PNG, zgodność ze słownikiem i backlogiem):

   ```powershell
   ./doc/crm_ddd_ai_agent_package/tools/validate-docs.ps1
   ```

4. Po decyzji biznesowej zaktualizuj `ai_readable/open_questions.md` i usuń odpowiadające jej komentarze `TODO` w kodzie.

