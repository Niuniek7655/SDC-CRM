# CRM DDD — paczka schematów i materiałów AI-friendly

Ta paczka zawiera:

1. Oryginalne schematy PNG wygenerowane w rozmowie.
2. Poprzedni schemat główny DDD w formie niezmienionej graficznie oraz w wersji źródłowej DOT/SVG.
3. Wersje tekstowe przygotowane dla agentów AI, takich jak Cursor, GitHub Copilot, Claude AI albo ChatGPT.
4. Pliki Mermaid (`.mmd`) i Markdown (`.md`) do łatwego wklejenia do repozytorium lub promptu.
5. JSON/YAML z bounded contextami, agregatami, encjami, value objectami, komendami i zdarzeniami.

## Najważniejsze pliki dla agentów AI

- `AI_AGENT_CONTEXT.md` — najprostszy punkt startowy dla agenta AI.
- `ai_readable/bounded_contexts_and_aggregates.md` — opis bounded contextów, agregatów, encji i value objectów.
- `ai_readable/bounded_contexts_and_aggregates.json` — struktura domeny w formacie maszynowym.
- `ai_readable/events_catalog.md` — katalog zdarzeń i komunikacji.
- `ai_readable/context_map.mmd` — Mermaid: mapa contextów i komunikacji.
- `ai_readable/processes/*.md` — procesy biznesowe w wersji tekstowej.
- `ai_readable/processes/*.mmd` — procesy biznesowe jako Mermaid flowchart.

## Najważniejsze obrazy

- `diagrams/png/00_context_map_original_communication.png`
- `diagrams/png/01_context_map_with_entities_value_objects.png`
- `diagrams/png/02_process_01_lead_pipeline.png`
- `diagrams/png/03_process_02_customer_management.png`
- `diagrams/png/04_process_03_sales_activity.png`
- `diagrams/png/05_process_04_sales_finalization_order_capture.png`
- `diagrams/png/06_process_05_backoffice_order_processing.png`
- `diagrams/png/07_process_06_integration_invoice_payment_reporting.png`

## Zasady interpretacji

- Duże obszary to bounded contexty.
- Agregaty są właścicielami reguł biznesowych i spójności transakcyjnej.
- Encje i value objecty należą do agregatów, a nie do całego systemu globalnie.
- Komunikacja synchroniczna powinna być używana głównie do komend lokalnych, zapytań/snapshotów i autoryzacji.
- Komunikacja między bounded contextami powinna bazować głównie na zdarzeniach integracyjnych.
- Reporting & Analytics jest contextem odczytowym/projekcyjnym.

## Ważna granica architektoniczna

Sprzedaż nie zarządza statusem realizacji backoffice. Backoffice nie zarządza pipeline sprzedażowym.

```text
Lead / Opportunity / Kontakt / Oferta  !=  Weryfikacja / Przydział / Braki / Realizacja / Zamknięcie
```
