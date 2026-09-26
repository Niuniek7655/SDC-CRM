# Proces 6 — Integracja, fakturowanie, płatność i aktualizacja raportów

**Cel:** przekazanie zrealizowanego zamówienia do ERP/fakturowania przez warstwę antykorupcyjną oraz aktualizacja
raportów i audytu ze zdarzeń.
**Konteksty:** Integrations, Order Backoffice, Reporting & KPI, Identity & Access (audyt).
**Agregaty:** `IntegrationJob`, `ExternalSystemMapping`. **Backlog:** CRM-030–034, CRM-037, CRM-038.
Wersja maszynowa: [`processes.json`](processes.json) (`process_06_integration_invoice_payment_reporting`).

Integracja ERP/fakturowania ma priorytet **Could Have**. Informacje zwrotne o fakturze i płatności oraz integracje
e-mail, kalendarza i płatności nie są częścią modelu do czasu decyzji (Q-10, Q-11).

Oznaczenia: ✔ zaimplementowane, ◐ częściowo, ○ planowane, ❓ do decyzji.

## Kroki

| Krok | Typ | Opis | Kontekst / agregat | Komenda | Zdarzenia | Stan | Story |
|---|---|---|---|---|---|---|---|
| S1 | polityka | Po `BackofficeOrderCompletedIntegrationEvent` utwórz zadanie eksportu (`Pending`) — moment wysyłki do potwierdzenia (Q-10) | Integrations / `IntegrationJob` | `ExportOrderToErp` | — | ○ | CRM-038 |
| S2 | akcja | ACL: mapowanie kontraktu CRM na DTO systemu zewnętrznego | Integrations | — | — | ○ | CRM-037 |
| S3 | zewnętrzny | Wywołanie API ERP / fakturowania | Integrations | — | — | ○ | CRM-038 |
| S4 | decyzja | Czy wysyłka się udała? | — | — | — | — | — |
| S5 | akcja | Zadanie zakończone sukcesem | Integrations / `IntegrationJob` | — | `IntegrationJobSucceeded` | ○ | CRM-038 |
| S6 | komenda | Zapis identyfikatora zamówienia w systemie zewnętrznym | Integrations / `ExternalSystemMapping` | `RecordExternalId` | — | ○ | CRM-038 |
| S7 | akcja | Zapis nieudanej próby (bez danych wrażliwych) | Integrations / `IntegrationJob` | — | `IntegrationJobFailed` | ○ | CRM-038 |
| S8 | decyzja | Czy limit automatycznych ponowień został wyczerpany? (Q-10) | — | — | — | — | — |
| S9 | akcja | Automatyczne ponowienie po odstępie | Integrations / `IntegrationJob` | — | — | ○ | CRM-038 |
| S10 | akcja | Błąd integracji widoczny dla uprawnionych użytkowników | Integrations | — | — | ○ | CRM-038 |
| S11 | komenda | Ręczne ponowienie wysyłki | Integrations / `IntegrationJob` | `RetryIntegrationJob` | — | ○ | CRM-038 |
| S12 | polityka | Projekcje raportowe i audyt ze zdarzeń wszystkich kontekstów (T-02, T-05, T-06) | Reporting & KPI | — | — | ○ | CRM-030–034 |
| S13 | polityka | Informacje zwrotne z ERP (faktura, płatność), e-mail, kalendarz, płatności — do decyzji | Integrations | — | — | ❓ | — |

**Read modele:** `SalespersonDashboard` (CRM-031), `SalesManagerDashboard` (CRM-032), `BackofficeReport` (CRM-033),
eksport CSV/XLSX (`ReportExport`, CRM-034), `AuditLog` (CRM-030) — wszystkie ○. Zasilanie:
[`events_catalog.md`](../events_catalog.md#zasilanie-read-modeli).

## Reguły biznesowe

- Integracja działa wyłącznie przez warstwę antykorupcyjną; DTO systemów zewnętrznych nie trafiają do domeny.
- System zapisuje status każdej wysyłki; błąd jest widoczny dla uprawnionych i można ponowić wysyłkę (CRM-038).
- Automatyczne ponowienia mają limit — nie ma nieskończonej pętli; po limicie wymagana jest decyzja człowieka.
- Raporty i audyt nie wywołują komend; aktualizują się wyłącznie ze zdarzeń.
- Wpis audytu zawiera użytkownika, czas, akcję i identyfikator obiektu (CRM-030).

**Otwarte pytania:** Q-10, Q-11, Q-15 ([`open_questions.md`](../open_questions.md)).

## Diagram

<!-- diagram: 07_process_06_integration_invoice_payment_reporting -->
```mermaid
flowchart TB
  classDef step fill:#ffffff,stroke:#64748b,color:#0f172a
  classDef cmd fill:#dbeafe,stroke:#1e40af,color:#0f172a
  classDef evt fill:#ffedd5,stroke:#c2410c,color:#0f172a
  classDef ie fill:#fff7ed,stroke:#c2410c,stroke-dasharray:4 3,color:#0f172a
  classDef pol fill:#ede9fe,stroke:#6d28d9,color:#0f172a
  classDef rm fill:#dcfce7,stroke:#15803d,color:#0f172a
  classDef ext fill:#fce7f3,stroke:#be185d,color:#0f172a
  classDef q fill:#fee2e2,stroke:#b91c1c,stroke-dasharray:5 3,color:#7f1d1d
  classDef dec fill:#ffffff,stroke:#334155,color:#0f172a

  IE("BackofficeOrderCompletedIntegrationEvent<br/>proces 5"):::ie
  S1[/"S1 · Polityka: <b>ExportOrderToErp</b><br/>IntegrationJob: Pending · CRM-038 — Could Have"/]:::pol
  Q10{{"Q-10: wysyłka po zakończeniu czy po zatwierdzeniu<br/>do fakturowania? limit ponowień, kto ponawia?"}}:::q
  S2["S2 · ACL: kontrakt CRM → DTO ERP<br/>CRM-037"]:::step
  S3["S3 · ERP / fakturowanie"]:::ext
  S4{"S4 · Wysyłka<br/>udana?"}:::dec
  S5("S5 · IntegrationJobSucceeded"):::evt
  S6["S6 · <b>RecordExternalId</b><br/>ExternalSystemMapping"]:::cmd
  S7("S7 · IntegrationJobFailed"):::evt
  S8{"S8 · Limit ponowień<br/>wyczerpany?"}:::dec
  S9["S9 · Automatyczne ponowienie po odstępie"]:::step
  S10[["S10 · Błąd widoczny dla uprawnionych<br/>CRM-038"]]:::rm
  S11["S11 · <b>RetryIntegrationJob</b><br/>BackofficeManager, Admin"]:::cmd
  EV("Zdarzenia domenowe<br/>procesów 1–5"):::evt
  subgraph REP["S12 · Projekcje: Reporting & KPI i audyt"]
    direction LR
    R1[["SalespersonDashboard<br/>CRM-031"]]:::rm
    R2[["SalesManagerDashboard<br/>CRM-032"]]:::rm
    R3[["BackofficeReport<br/>CRM-033"]]:::rm
    R4[["Eksport CSV/XLSX<br/>CRM-034"]]:::rm
    R5[["AuditLog<br/>CRM-030"]]:::rm
  end
  S13{{"S13 · Q-10, Q-11: faktura i płatność z ERP,<br/>e-mail, kalendarz, płatności"}}:::q

  IE --> S1 --> S2 --> S3 --> S4
  Q10 -.- S1
  S4 -- "tak" --> S5 --> S6
  S4 -- "nie" --> S7 --> S8
  S8 -- "nie" --> S9 --> S2
  S8 -- "tak" --> S10 --> S11 --> S2
  S3 -.- S13
  EV -. "projekcje i audyt" .-> REP

  subgraph LEGEND["Legenda"]
    direction LR
    L1["komenda"]:::cmd
    L2("zdarzenie"):::evt
    L3[/"polityka"/]:::pol
    L4[["read model"]]:::rm
    L5["system zewnętrzny"]:::ext
    L6{{"pytanie Q-xx"}}:::q
  end
```

Plik PNG: [`diagrams/png/07_process_06_integration_invoice_payment_reporting.png`](../../diagrams/png/07_process_06_integration_invoice_payment_reporting.png).

