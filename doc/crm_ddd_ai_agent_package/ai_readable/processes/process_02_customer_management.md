# Proces 2 — Zarządzanie klientem

**Cel:** jedna karta klienta z osobami kontaktowymi, używana przez sprzedaż i zamówienia przez `CustomerId`.
**Konteksty:** Customer Management, Sales Pipeline, Order Capture, Reporting & KPI.
**Agregaty:** `Customer`. **Backlog:** CRM-001, CRM-007, CRM-008, CRM-009.
Wersja maszynowa: [`processes.json`](processes.json) (`process_02_customer_management`).

Oznaczenia: ✔ zaimplementowane, ◐ częściowo, ○ planowane, ❓ do decyzji.

## Kroki

| Krok | Typ | Opis | Kontekst / agregat | Komenda | Zdarzenia | Stan | Story |
|---|---|---|---|---|---|---|---|
| S1 | start | Handlowiec ma dane klienta: wprowadza je ręcznie albo przy kwalifikacji leada (Q-01) | Customer Management | — | — | — | CRM-007 |
| S2 | akcja | System ostrzega o potencjalnym duplikacie (Q-02) | Customer Management / `Customer` | — | — | ○ | CRM-001, CRM-007 |
| S3 | decyzja | Czy klient już istnieje? | — | — | — | — | — |
| S4 | akcja | Użycie istniejącego klienta — powiązanie przez `CustomerId`, bez nadpisywania jego danych | Customer Management | — | — | ○ | CRM-007 |
| S5 | komenda | Utworzenie karty klienta: nazwa, telefon, e-mail, opcjonalnie NIP | Customer Management / `Customer` | `CreateCustomer` | `CustomerCreated` | ○ | CRM-007 |
| S6 | komenda | Dodanie osoby kontaktowej (wiele osób, jedna może być główna) | Customer Management / `Customer` | `AddContactPerson` | `ContactPersonAdded` | ○ | CRM-008 |
| S7 | koniec | Klient dostępny w procesach: `CustomerId` w szansie ([proces 1](process_01_lead_pipeline.md)), snapshot w zamówieniu ([proces 4](process_04_sales_finalization_order_capture.md)) | Customer Management | — | — | — | CRM-009 |

**Read modele:** Customer 360 (`GetCustomer360Query`, CRM-009) ○ — dane klienta, osoby kontaktowe, leady, szanse,
kontakty, notatki i zamówienia, od najnowszych.

## Reguły biznesowe

- Klient musi mieć nazwę; telefon, e-mail i NIP (`TaxIdentifier`) są danymi kontaktowymi klienta.
- Duplikat jest tylko ostrzeżeniem — handlowiec wybiera istniejącego klienta albo świadomie tworzy nowego (Q-02).
- Użycie istniejącego klienta nie nadpisuje jego danych; zmiana danych klienta to osobna decyzja (poza backlogiem MVP).
- Inne konteksty przechowują tylko `CustomerId`; `Order Capture` pobiera snapshot zapytaniem synchronicznym.
- Widoczność klientów innych handlowców — `doc/02` §4.1 (Q-02, Q-13).

**Otwarte pytania:** Q-01, Q-02, Q-13 ([`open_questions.md`](../open_questions.md)).

## Diagram

<!-- diagram: 03_process_02_customer_management -->
```mermaid
flowchart TB
  classDef actor fill:#f1f5f9,stroke:#475569,color:#0f172a
  classDef step fill:#ffffff,stroke:#64748b,color:#0f172a
  classDef cmd fill:#dbeafe,stroke:#1e40af,color:#0f172a
  classDef evt fill:#ffedd5,stroke:#c2410c,color:#0f172a
  classDef rm fill:#dcfce7,stroke:#15803d,color:#0f172a
  classDef q fill:#fee2e2,stroke:#b91c1c,stroke-dasharray:5 3,color:#7f1d1d
  classDef dec fill:#ffffff,stroke:#334155,color:#0f172a

  S1(["S1 · Handlowiec: dane klienta<br/>ręcznie lub przy kwalifikacji leada"]):::actor
  Q1{{"Q-01: kiedy powstaje Customer z leada?"}}:::q
  S2["S2 · Ostrzeżenie o potencjalnym duplikacie<br/>CRM-001, CRM-007"]:::step
  Q2{{"Q-02: kryteria duplikatu<br/>NIP, nazwa, e-mail, telefon?"}}:::q
  S3{"S3 · Klient<br/>już istnieje?"}:::dec
  S4["S4 · Użyj istniejącego klienta<br/>powiązanie przez CustomerId<br/>bez nadpisywania danych"]:::step
  S5["S5 · <b>CreateCustomer</b><br/>nazwa, telefon, e-mail, opcjonalnie NIP<br/>CRM-007"]:::cmd
  E5("CustomerCreated"):::evt
  S6["S6 · <b>AddContactPerson</b><br/>wiele osób, jedna główna · CRM-008"]:::cmd
  E6("ContactPersonAdded"):::evt
  S7(["S7 · Klient dostępny w procesach<br/>CustomerId w Opportunity — proces 1<br/>snapshot w SalesOrder — proces 4"]):::actor
  R1[["Customer 360 — GetCustomer360Query<br/>CRM-009"]]:::rm

  Q1 -.- S1
  S1 --> S2 --> S3
  Q2 -.- S2
  S3 -- "tak" --> S4
  S3 -- "nie" --> S5 --> E5 --> S6
  S4 -- "nowa osoba kontaktowa" --> S6
  S4 --> S7
  S6 --> E6 --> S7
  E5 & E6 -. "projekcja" .-> R1

  subgraph LEGEND["Legenda"]
    direction LR
    L1["komenda"]:::cmd
    L2("zdarzenie domenowe"):::evt
    L3["krok bez komendy domenowej"]:::step
    L4[["read model"]]:::rm
    L5{{"pytanie Q-xx"}}:::q
  end
```

Plik PNG: [`diagrams/png/03_process_02_customer_management.png`](../../diagrams/png/03_process_02_customer_management.png).

