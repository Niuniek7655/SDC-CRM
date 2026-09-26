# Proces 5 — Obsługa zamówienia przez backoffice

**Cel:** od przekazanego zamówienia do zamkniętej sprawy backoffice — z kolejką, przydziałem, weryfikacją,
zwrotem do handlowca, blokadą i zakończeniem.
**Konteksty:** Order Backoffice, Order Capture, Sales Activities, Integrations, Reporting & KPI.
**Agregaty:** `BackofficeOrderCase`. **Backlog:** CRM-020, CRM-022–027.
Wersja maszynowa: [`processes.json`](processes.json) (`process_05_backoffice_order_processing`).
Statusy i dozwolone przejścia: [`order_status_lifecycle.md`](../order_status_lifecycle.md) (diagram 09, Q-06).

Oznaczenia: ✔ zaimplementowane, ◐ częściowo, ○ planowane, ❓ do decyzji.

## Kroki

| Krok | Typ | Opis | Kontekst / agregat | Komenda / zapytanie | Zdarzenia | Stan | Story |
|---|---|---|---|---|---|---|---|
| S1 | polityka | Po `SalesOrderSubmittedIntegrationEvent` otwórz sprawę (`New`) albo wznów istniejącą po ponownym przekazaniu (Q-08) | Order Backoffice / `BackofficeOrderCase` | `OpenBackofficeOrderCase` | `BackofficeOrderCaseOpened` | ○ | CRM-020, CRM-022 |
| S2 | zapytanie | Kolejka backoffice: klient, handlowiec, data przekazania, status; filtrowanie i sortowanie | Order Backoffice | `GetBackofficeQueueQuery` | — | ○ | CRM-022 |
| S3 | komenda | Przypisanie pracownika (menedżer) albo przypisanie do siebie (pracownik) | Order Backoffice / `BackofficeOrderCase` | `AssignBackofficeOrder` | `BackofficeOrderAssigned` | ○ | CRM-023 |
| S4 | komenda | Rozpoczęcie weryfikacji: `InVerification` | Order Backoffice / `BackofficeOrderCase` | `ChangeBackofficeOrderStatus` | `BackofficeOrderStatusChanged` | ○ | CRM-024 |
| S5 | decyzja | Czy dane są kompletne i poprawne? | — | — | — | — | — |
| S6 | komenda | Zwrot do handlowca z komentarzem; publikacja `BackofficeOrderReturnedToSalesIntegrationEvent` ([proces 4](process_04_sales_finalization_order_capture.md)) | Order Backoffice / `BackofficeOrderCase` | `ReturnOrderToSales` | `BackofficeOrderReturnedToSales` | ○ | CRM-026 |
| S7 | komenda | Oczekiwanie na informacje bez zwrotu do handlowca: `AwaitingInformation` | Order Backoffice / `BackofficeOrderCase` | `ChangeBackofficeOrderStatus` | `BackofficeOrderStatusChanged` | ○ | CRM-024 |
| S8 | komenda | Realizacja: `InFulfillment` | Order Backoffice / `BackofficeOrderCase` | `ChangeBackofficeOrderStatus` | `BackofficeOrderStatusChanged` | ○ | CRM-024 |
| S9 | decyzja | Czy jest przeszkoda w realizacji? | — | — | — | — | — |
| S10 | komenda | Zablokowanie z powodem (`BlockingReason`); po usunięciu przeszkody powrót do realizacji | Order Backoffice / `BackofficeOrderCase` | `ChangeBackofficeOrderStatus` | `BackofficeOrderBlocked` | ○ | CRM-024 |
| S11 | komenda | Zakończenie: `Completed` z datą zakończenia; publikacja `BackofficeOrderCompletedIntegrationEvent` | Order Backoffice / `BackofficeOrderCase` | `CompleteOrder` | `BackofficeOrderCompleted` | ○ | CRM-027 |
| S12 | koniec | Sprawa zamknięta; dalej [proces 6](process_06_integration_invoice_payment_reporting.md) | Order Backoffice | — | — | — | CRM-027 |
| S13 | komenda | Komentarz wewnętrzny albo widoczny dla sprzedaży — w dowolnym momencie obsługi | Order Backoffice / `BackofficeOrderCase` | `AddBackofficeComment` | `BackofficeCommentAdded` | ○ | CRM-025 |
| S14 | komenda | Anulowanie zamówienia i publikacja `BackofficeOrderCancelledIntegrationEvent` — kto i z jakich statusów, do decyzji | Order Backoffice / `BackofficeOrderCase` | `CancelOrder` | `BackofficeOrderCancelled` | ❓ | — |

**Read modele:** Kolejka backoffice (`GetBackofficeQueueQuery`, CRM-022) ○; `BackofficeReport` (CRM-033) ○;
Status zamówienia dla handlowca (`GetSalesOrderStatusQuery`, CRM-021) ○.

## Reguły biznesowe

- Przejścia statusów tylko zgodnie z dozwolonym workflow (Q-06); każda zmiana statusu i przypisania trafia do historii.
- Zablokowanie wymaga powodu; zwrot do handlowca wymaga komentarza; zakończenie zapisuje datę zakończenia.
- Komentarze mają autora i datę, mogą być wewnętrzne albo widoczne dla sprzedaży i nie znikają po zmianie statusu.
- Backoffice nie zmienia danych `SalesOrder` ani pipeline — zwraca zamówienie, a poprawkę wykonuje handlowiec.
- Przypisanie pracownikowi: `BackofficeManager`; `BackofficeUser` może przypisać zamówienie do siebie (`doc/02`).

**Otwarte pytania:** Q-06, Q-07, Q-08, Q-09, Q-14 ([`open_questions.md`](../open_questions.md)).

## Diagram

<!-- diagram: 06_process_05_backoffice_order_processing -->
```mermaid
flowchart TB
  classDef cmd fill:#dbeafe,stroke:#1e40af,color:#0f172a
  classDef evt fill:#ffedd5,stroke:#c2410c,color:#0f172a
  classDef ie fill:#fff7ed,stroke:#c2410c,stroke-dasharray:4 3,color:#0f172a
  classDef pol fill:#ede9fe,stroke:#6d28d9,color:#0f172a
  classDef rm fill:#dcfce7,stroke:#15803d,color:#0f172a
  classDef q fill:#fee2e2,stroke:#b91c1c,stroke-dasharray:5 3,color:#7f1d1d
  classDef dec fill:#ffffff,stroke:#334155,color:#0f172a
  classDef actor fill:#f1f5f9,stroke:#475569,color:#0f172a

  IE1("SalesOrderSubmittedIntegrationEvent<br/>proces 4"):::ie
  S1[/"S1 · Polityka: <b>OpenBackofficeOrderCase</b><br/>nowa sprawa lub wznowienie · CRM-020"/]:::pol
  Q8{{"Q-08: wznowić tę samą sprawę?<br/>czy przypisanie zostaje?"}}:::q
  E1("BackofficeOrderCaseOpened · New"):::evt
  S2[["S2 · Kolejka backoffice<br/>GetBackofficeQueueQuery · CRM-022"]]:::rm
  S3["S3 · <b>AssignBackofficeOrder</b><br/>BackofficeManager; BackofficeUser — do siebie<br/>CRM-023"]:::cmd
  E3("BackofficeOrderAssigned"):::evt
  S4["S4 · <b>ChangeBackofficeOrderStatus</b><br/>→ InVerification · CRM-024"]:::cmd
  E4("BackofficeOrderStatusChanged"):::evt
  S5{"S5 · Dane kompletne<br/>i poprawne?"}:::dec
  S6["S6 · <b>ReturnOrderToSales</b><br/>komentarz wymagany · CRM-026"]:::cmd
  E6("BackofficeOrderReturnedToSales"):::evt
  IE6("BackofficeOrderReturnedToSalesIntegrationEvent<br/>proces 4: poprawa i ponowne przekazanie"):::ie
  S7["S7 · <b>ChangeBackofficeOrderStatus</b><br/>→ AwaitingInformation"]:::cmd
  S8["S8 · <b>ChangeBackofficeOrderStatus</b><br/>→ InFulfillment"]:::cmd
  S9{"S9 · Przeszkoda<br/>w realizacji?"}:::dec
  S10["S10 · <b>ChangeBackofficeOrderStatus</b><br/>→ Blocked, BlockingReason wymagany"]:::cmd
  E10("BackofficeOrderBlocked"):::evt
  S11["S11 · <b>CompleteOrder</b><br/>data zakończenia · CRM-027"]:::cmd
  E11("BackofficeOrderCompleted"):::evt
  IE11("BackofficeOrderCompletedIntegrationEvent"):::ie
  S12(["S12 · Sprawa zamknięta — proces 6"]):::actor
  S13["S13 · <b>AddBackofficeComment</b><br/>wewnętrzny lub dla sprzedaży<br/>w dowolnym momencie · CRM-025"]:::cmd
  S14["S14 · <b>CancelOrder</b> → BackofficeOrderCancelled<br/>Q-07: kto i z jakich statusów?"]:::q
  Q6{{"Q-06: dozwolone przejścia<br/>statusów — diagram 09"}}:::q
  R1[["BackofficeReport · CRM-033<br/>Status zamówienia · CRM-021"]]:::rm

  IE1 --> S1 --> E1 --> S2 --> S3 --> E3 --> S4 --> E4 --> S5
  Q8 -.- S1
  Q6 -.- S4
  S5 -- "braki do poprawy<br/>przez handlowca" --> S6 --> E6 --> IE6
  S5 -- "brak informacji" --> S7
  S7 -- "informacja uzupełniona" --> S4
  S5 -- "tak" --> S8 --> S9
  S9 -- "tak" --> S10 --> E10
  E10 -- "odblokowanie" --> S8
  S9 -- "nie" --> S11 --> E11 --> IE11 --> S12
  S2 -. "w dowolnym momencie" .-> S13
  S2 -. "do decyzji" .-> S14
  E4 & E6 & E10 & E11 -. "projekcje" .-> R1

  subgraph LEGEND["Legenda"]
    direction LR
    L1["komenda"]:::cmd
    L2("zdarzenie domenowe"):::evt
    L3("zdarzenie integracyjne"):::ie
    L4[/"polityka"/]:::pol
    L5[["read model / zapytanie"]]:::rm
    L6{{"pytanie Q-xx"}}:::q
  end
```

Plik PNG: [`diagrams/png/06_process_05_backoffice_order_processing.png`](../../diagrams/png/06_process_05_backoffice_order_processing.png).

