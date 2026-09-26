# Diagramy

Pliki w `png/` są **generowane** z bloków Mermaid osadzonych w dokumentach `ai_readable/` — nie edytuj ich ręcznie.
Każdy blok jest poprzedzony znacznikiem `<!-- diagram: NAZWA -->`, a `NAZWA` jest nazwą pliku PNG.
Skróty źródeł zapisuje `diagrams.manifest.json`; `tools/validate-docs.ps1` wykrywa na tej podstawie nieaktualne PNG.

| Plik | Źródło | Zawartość |
|---|---|---|
| [`00_system_context.png`](png/00_system_context.png) | [`system_context.md`](../ai_readable/system_context.md) | klienci, backend, SSO, baza, obserwowalność, broker, ERP — ze stanem implementacji |
| [`01_context_map_with_entities_value_objects.png`](png/01_context_map_with_entities_value_objects.png) | [`context_map.md`](../ai_readable/context_map.md) | bounded contexty, agregaty, encje, VO, relacje synchroniczne i zdarzenia |
| [`02_process_01_lead_pipeline.png`](png/02_process_01_lead_pipeline.png) | [`process_01_lead_pipeline.md`](../ai_readable/processes/process_01_lead_pipeline.md) | lead → kwalifikacja → szansa → wygrana/przegrana |
| [`03_process_02_customer_management.png`](png/03_process_02_customer_management.png) | [`process_02_customer_management.md`](../ai_readable/processes/process_02_customer_management.md) | karta klienta, duplikaty, osoby kontaktowe |
| [`04_process_03_sales_activity.png`](png/04_process_03_sales_activity.png) | [`process_03_sales_activity.md`](../ai_readable/processes/process_03_sales_activity.md) | kontakty, notatki, follow-upy |
| [`05_process_04_sales_finalization_order_capture.png`](png/05_process_04_sales_finalization_order_capture.png) | [`process_04_sales_finalization_order_capture.md`](../ai_readable/processes/process_04_sales_finalization_order_capture.md) | zamówienie z wygranej szansy, kompletność, przekazanie, pętla zwrotu |
| [`06_process_05_backoffice_order_processing.png`](png/06_process_05_backoffice_order_processing.png) | [`process_05_backoffice_order_processing.md`](../ai_readable/processes/process_05_backoffice_order_processing.md) | kolejka, przydział, weryfikacja, zwrot, blokada, zakończenie |
| [`07_process_06_integration_invoice_payment_reporting.png`](png/07_process_06_integration_invoice_payment_reporting.png) | [`process_06_integration_invoice_payment_reporting.md`](../ai_readable/processes/process_06_integration_invoice_payment_reporting.md) | eksport do ERP z ponowieniami, raporty, audyt |
| [`08_sales_order_lifecycle.png`](png/08_sales_order_lifecycle.png) | [`order_status_lifecycle.md`](../ai_readable/order_status_lifecycle.md) | statusy `SalesOrder` |
| [`09_backoffice_order_case_lifecycle.png`](png/09_backoffice_order_case_lifecycle.png) | [`order_status_lifecycle.md`](../ai_readable/order_status_lifecycle.md) | statusy `BackofficeOrderCase` (propozycja — Q-06) |

## Legenda

| Element | Wygląd |
|---|---|
| Komenda | niebieski prostokąt |
| Zdarzenie domenowe | pomarańczowy zaokrąglony prostokąt |
| Zdarzenie integracyjne | jasnopomarańczowy, przerywana ramka |
| Polityka (reakcja automatyczna) | fioletowy równoległobok |
| Read model / zapytanie | zielony prostokąt z bocznymi liniami |
| Agregat | żółty prostokąt |
| System zewnętrzny | różowy prostokąt |
| Pytanie biznesowe `Q-xx` | czerwony sześciokąt z przerywaną ramką |
| Zaimplementowane | gruba zielona ramka |
| Częściowo zaimplementowane | gruba pomarańczowa przerywana ramka |

Na mapie kontekstów linia ciągła oznacza wywołanie synchroniczne (komenda/zapytanie), a przerywana — zdarzenie.

## Generowanie

```powershell
# z katalogu głównego repozytorium; renderuje tylko zmienione diagramy
./doc/crm_ddd_ai_agent_package/tools/render-diagrams.ps1

# wszystkie diagramy albo wybrane
./doc/crm_ddd_ai_agent_package/tools/render-diagrams.ps1 -Force
./doc/crm_ddd_ai_agent_package/tools/render-diagrams.ps1 -Name 02_process_01_lead_pipeline
```

Skrypt używa `npx @mermaid-js/mermaid-cli` w przypiętej wersji i konfiguracji `tools/mermaid-config.json`.
Jeżeli w systemie jest Chrome lub Edge (albo ustawiono `PUPPETEER_EXECUTABLE_PATH`), nie pobiera własnej przeglądarki.
PNG bez źródła są usuwane z `png/`.

