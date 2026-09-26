# Proces 4 — Finalizacja sprzedaży i wprowadzanie zamówienia

**Cel:** zamówienie handlowca od wygranej szansy do przekazania do backoffice, z pętlą poprawek po zwrocie.
**Konteksty:** Order Capture, Sales Pipeline, Customer Management, Order Backoffice, Reporting & KPI.
**Agregaty:** `SalesOrder`. **Backlog:** CRM-016, CRM-018–021, CRM-026.
Wersja maszynowa: [`processes.json`](processes.json) (`process_04_sales_finalization_order_capture`).
Statusy: [`order_status_lifecycle.md`](../order_status_lifecycle.md) (diagram 08).

Oznaczenia: ✔ zaimplementowane, ◐ częściowo, ○ planowane, ❓ do decyzji.

## Kroki

| Krok | Typ | Opis | Kontekst / agregat | Komenda | Zdarzenia | Stan | Story |
|---|---|---|---|---|---|---|---|
| S1 | start | Szansa jest wygrana (`OpportunityWon`, [proces 1](process_01_lead_pipeline.md)); handlowiec otwiera ją | Sales Pipeline | — | — | — | CRM-016 |
| S2 | komenda | Utworzenie zamówienia w statusie `Draft`; handler sprawdza zapytaniem, czy szansa jest wygrana (Q-04), i pobiera snapshot klienta | Order Capture / `SalesOrder` | `CreateOrderFromOpportunity` | `SalesOrderCreated` | ○ | CRM-018 |
| S3 | komenda | Edycja szkicu: pozycje i warunki; zapis bez przekazania | Order Capture / `SalesOrder` | `AddOrderLine` | — | ○ | CRM-018, CRM-019 |
| S4 | decyzja | Czy zamówienie jest kompletne? Lista braków widoczna przed przekazaniem (Q-05) | Order Capture / `SalesOrder` | — | — | — | CRM-019 |
| S5 | komenda | Przekazanie do backoffice; edycja zablokowana; publikacja `SalesOrderSubmittedIntegrationEvent` | Order Capture / `SalesOrder` | `SubmitOrderToBackoffice` | `SalesOrderSubmittedToBackoffice` | ○ | CRM-020 |
| S6 | koniec | Zamówienie w obsłudze backoffice ([proces 5](process_05_backoffice_order_processing.md)) | Order Backoffice | — | — | — | CRM-022 |
| S7 | polityka | Po `BackofficeOrderReturnedToSalesIntegrationEvent` zamówienie przechodzi w `ReturnedToSales` i znów jest edytowalne | Order Capture / `SalesOrder` | `ReopenSalesOrderAfterReturn` | — | ○ | CRM-026 |

Wywołania synchroniczne w S2: `Sales Pipeline` (czy szansa jest wygrana, dane szansy) i `Customer Management`
(snapshot klienta → `OrderCustomerSnapshot`).

**Read modele:** Status zamówienia (`GetSalesOrderStatusQuery`, CRM-021) ○; `SalespersonDashboard` — zamówienia
zwrócone do poprawy (CRM-031) ○.

## Reguły biznesowe

- Zamówienie tworzy handlowiec z wygranej szansy — nie powstaje automatycznie po `OpportunityWon` (CRM-016, CRM-018, Q-04).
- Zamówienie jest powiązane z klientem i szansą; dane klienta są wstępnie uzupełnione ze snapshotu.
- Szkic można zapisać bez przekazania do backoffice.
- Niekompletnego zamówienia nie można przekazać; komunikaty walidacji są zrozumiałe biznesowo (CRM-019, Q-05).
- Po przekazaniu handlowiec nie może dowolnie edytować zamówienia; edycja wraca tylko po zwrocie z backoffice.
- Ponowne przekazanie po poprawie wznawia obsługę w backoffice (Q-08).

**Otwarte pytania:** Q-04, Q-05, Q-07, Q-08 ([`open_questions.md`](../open_questions.md)).

## Diagram

<!-- diagram: 05_process_04_sales_finalization_order_capture -->
```mermaid
flowchart TB
  classDef actor fill:#f1f5f9,stroke:#475569,color:#0f172a
  classDef step fill:#ffffff,stroke:#64748b,color:#0f172a
  classDef cmd fill:#dbeafe,stroke:#1e40af,color:#0f172a
  classDef evt fill:#ffedd5,stroke:#c2410c,color:#0f172a
  classDef ie fill:#fff7ed,stroke:#c2410c,stroke-dasharray:4 3,color:#0f172a
  classDef pol fill:#ede9fe,stroke:#6d28d9,color:#0f172a
  classDef rm fill:#dcfce7,stroke:#15803d,color:#0f172a
  classDef q fill:#fee2e2,stroke:#b91c1c,stroke-dasharray:5 3,color:#7f1d1d
  classDef dec fill:#ffffff,stroke:#334155,color:#0f172a

  E0("OpportunityWon — proces 1"):::evt
  S1(["S1 · Handlowiec otwiera wygraną szansę"]):::actor
  S2["S2 · <b>CreateOrderFromOpportunity</b><br/>status Draft · CRM-018"]:::cmd
  Q4{{"Q-04: zamówienie bez wygranej szansy?"}}:::q
  SQ1["zapytanie do Sales Pipeline:<br/>czy szansa wygrana, dane szansy"]:::step
  SQ2["zapytanie do Customer Management:<br/>snapshot klienta → OrderCustomerSnapshot"]:::step
  E2("SalesOrderCreated"):::evt
  S3["S3 · <b>AddOrderLine</b> i edycja szkicu<br/>zapis bez przekazania · CRM-018"]:::cmd
  S4{"S4 · Zamówienie kompletne?<br/>lista braków · CRM-019"}:::dec
  Q5{{"Q-05: jakie dane są wymagane?"}}:::q
  S5["S5 · <b>SubmitOrderToBackoffice</b><br/>edycja zablokowana · CRM-020"]:::cmd
  E5("SalesOrderSubmittedToBackoffice"):::evt
  IE5("SalesOrderSubmittedIntegrationEvent"):::ie
  S6(["S6 · Obsługa w backoffice — proces 5"]):::actor
  IE7("BackofficeOrderReturnedToSalesIntegrationEvent<br/>komentarz backoffice — proces 5"):::ie
  S7[/"S7 · Polityka: <b>ReopenSalesOrderAfterReturn</b><br/>status ReturnedToSales, edycja możliwa · CRM-026"/]:::pol
  R1[["Status zamówienia — GetSalesOrderStatusQuery · CRM-021<br/>SalespersonDashboard: zwrócone do poprawy · CRM-031"]]:::rm

  E0 --> S1 --> S2
  Q4 -.- S2
  S2 -- "zapytanie" --> SQ1
  S2 -- "zapytanie" --> SQ2
  S2 --> E2 --> S3 --> S4
  Q5 -.- S4
  S4 -- "nie" --> S3
  S4 -- "tak" --> S5 --> E5 --> IE5 --> S6
  IE7 --> S7 --> S3
  E5 & IE7 -. "projekcje" .-> R1

  subgraph LEGEND["Legenda"]
    direction LR
    L1["komenda"]:::cmd
    L2("zdarzenie domenowe"):::evt
    L3("zdarzenie integracyjne"):::ie
    L4[/"polityka"/]:::pol
    L5[["read model"]]:::rm
    L6{{"pytanie Q-xx"}}:::q
  end
```

Plik PNG: [`diagrams/png/05_process_04_sales_finalization_order_capture.png`](../../diagrams/png/05_process_04_sales_finalization_order_capture.png).

