# CRM — eksport analizy DDD i backlogu

Data wygenerowania: 2026-06-14

Ten katalog zawiera roboczy zestaw plików Markdown dla projektu CRM sprzedażowo-backoffice’owego.

## Pliki

Punkt wyjścia: `CRM.md` — pierwotny brief projektu (założenia bazowe: moduły, wymagania niefunkcjonalne).

1. `01-slownik-jezyka-wszechobecnego.md`  
   Słownik pojęć domenowych w podejściu DDD.

2. `02-uzytkownicy-role-uprawnienia.md`  
   Użytkownicy, role, odpowiedzialności i podstawowa macierz uprawnień.

3. `03-backlog-user-stories.md`  
   Lista epików i user stories z miejscem na oznaczanie statusu realizacji.

4. `crm_ddd_ai_agent_package/`  
   Model domeny DDD dla ludzi i agentów AI: bounded contexty, agregaty, zdarzenia, procesy, diagramy
   oraz aktualny stan implementacji. Punkt startowy: [crm_ddd_ai_agent_package/README.md](crm_ddd_ai_agent_package/README.md).

Nazwy komend, zdarzeń i statusów są wspólne dla słownika, backlogu i pakietu DDD. Spójność sprawdza
`crm_ddd_ai_agent_package/tools/validate-docs.ps1`, a diagramy generuje `crm_ddd_ai_agent_package/tools/render-diagrams.ps1`.

## Legenda statusów dla backlogu

Dla każdej story możesz uzupełnić status:

- `Backlog` — wymaganie zidentyfikowane, ale jeszcze niegotowe do realizacji.
- `Ready` — story gotowa do developmentu.
- `In Progress` — w trakcie realizacji.
- `Blocked` — zablokowane.
- `Review` — gotowe do review / testów / akceptacji.
- `Done` — zakończone i zaakceptowane.

## Sugerowana kolejność pracy

1. Zarejestrowanie nowego leada.
2. Lista moich leadów.
3. Szczegóły leada.
4. Notatki i kontakty.
5. Kwalifikacja leada do szansy sprzedaży.
6. Pipeline.
7. Utworzenie zamówienia.
8. Przekazanie zamówienia do backoffice.
9. Kolejka backoffice.
10. Statusy i obsługa zamówienia.
