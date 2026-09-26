# Proces 3 — Planowanie i rejestracja aktywności handlowca

**Cel:** kontakty, notatki i follow-upy powiązane z klientem, leadem albo szansą; lista zadań handlowca.
**Konteksty:** Sales Activities, Sales Pipeline, Customer Management, Reporting & KPI.
**Agregaty:** `SalesActivity` (typy: kontakt, `SalesNote`, `FollowUp`). **Backlog:** CRM-010–013.
Wersja maszynowa: [`processes.json`](processes.json) (`process_03_sales_activity`).

Oznaczenia: ✔ zaimplementowane, ◐ częściowo, ○ planowane, ❓ do decyzji.

## Kroki

| Krok | Typ | Opis | Kontekst / agregat | Komenda / zapytanie | Zdarzenia | Stan | Story |
|---|---|---|---|---|---|---|---|
| S1 | start | Handlowiec wybiera klienta, leada albo szansę (`RelatedTo`) | Sales Activities | — | — | — | — |
| S2 | decyzja | Co rejestruje: kontakt, notatkę czy kolejny kontakt? | — | — | — | — | — |
| S3 | komenda | Rejestracja kontaktu: typ (telefon, e-mail, spotkanie, inny), data, autor, opcjonalnie notatka | Sales Activities / `SalesActivity` | `LogSalesActivity` | `SalesActivityLogged` | ○ | CRM-011 |
| S4 | komenda | Dodanie notatki: treść, autor, data | Sales Activities / `SalesActivity` | `AddSalesNote` | `SalesNoteAdded` | ○ | CRM-010 |
| S5 | komenda | Zaplanowanie follow-upu: termin, opis, właściciel | Sales Activities / `SalesActivity` | `ScheduleFollowUp` | `FollowUpScheduled` | ○ | CRM-012 |
| S6 | zapytanie | Lista moich zadań: dzisiejsze, przyszłe, zaległe | Sales Activities | `GetMyFollowUpsQuery` | — | ○ | CRM-013 |
| S7 | komenda | Oznaczenie follow-upu jako wykonanego | Sales Activities / `SalesActivity` | `CompleteFollowUp` | `FollowUpCompleted` | ○ | CRM-013 |
| S8 | decyzja | Czy wynik zmienia pipeline? | — | — | — | — | — |
| S9 | akcja | Handlowiec wykonuje komendę w Sales Pipeline ([proces 1](process_01_lead_pipeline.md)) — bez automatycznej zmiany | Sales Pipeline | `QualifyLead`, `RejectLead`, `ChangeOpportunityStage` | — | ◐ | CRM-005, CRM-006, CRM-015 |
| S10 | koniec | Aktywność widoczna w historii leada i w Customer 360 | Sales Activities | — | — | — | CRM-003, CRM-009 |
| S11 | polityka | Po zwrocie zamówienia (`BackofficeOrderReturnedToSalesIntegrationEvent`) follow-up dla handlowca — tylko po decyzji Q-09 | Sales Activities | — | — | ❓ | CRM-026 |

**Read modele:** Moje zadania (`GetMyFollowUpsQuery`, CRM-013) ○; `SalespersonDashboard` (CRM-031) i
`SalesManagerDashboard` (CRM-032) ○; historia w `GetLeadDetailsQuery` (CRM-003) i `GetCustomer360Query` (CRM-009) ○.

## Reguły biznesowe

- Aktywność jest powiązana z dokładnie jednym obiektem: klientem, leadem albo szansą (`RelatedTo`).
- Kontakt ma typ, datę i autora; notatka ma treść, autora i datę; follow-up ma termin i właściciela.
- Follow-up po terminie i niewykonany jest zaległy — widoczny na liście zadań i dashboardzie.
- Notatek nie usuwa się fizycznie; edycja i usuwanie — Q-17.
- Wynik kontaktu nie zmienia pipeline automatycznie.

**Otwarte pytania:** Q-09, Q-16, Q-17 ([`open_questions.md`](../open_questions.md)).

## Diagram

<!-- diagram: 04_process_03_sales_activity -->
```mermaid
flowchart TB
  classDef actor fill:#f1f5f9,stroke:#475569,color:#0f172a
  classDef cmd fill:#dbeafe,stroke:#1e40af,color:#0f172a
  classDef evt fill:#ffedd5,stroke:#c2410c,color:#0f172a
  classDef ie fill:#fff7ed,stroke:#c2410c,stroke-dasharray:4 3,color:#0f172a
  classDef pol fill:#ede9fe,stroke:#6d28d9,color:#0f172a
  classDef rm fill:#dcfce7,stroke:#15803d,color:#0f172a
  classDef q fill:#fee2e2,stroke:#b91c1c,stroke-dasharray:5 3,color:#7f1d1d
  classDef dec fill:#ffffff,stroke:#334155,color:#0f172a

  S1(["S1 · Handlowiec wybiera klienta, leada lub szansę<br/>RelatedTo: CustomerId, LeadId albo OpportunityId"]):::actor
  S2{"S2 · Co<br/>rejestruje?"}:::dec
  S3["S3 · <b>LogSalesActivity</b><br/>telefon, e-mail, spotkanie, inny<br/>data, autor · CRM-011"]:::cmd
  E3("SalesActivityLogged"):::evt
  S4["S4 · <b>AddSalesNote</b><br/>treść, autor, data · CRM-010"]:::cmd
  E4("SalesNoteAdded"):::evt
  S5["S5 · <b>ScheduleFollowUp</b><br/>termin, opis, właściciel · CRM-012"]:::cmd
  E5("FollowUpScheduled"):::evt
  S6[["S6 · Moje zadania — GetMyFollowUpsQuery<br/>dzisiejsze, przyszłe, zaległe · CRM-013"]]:::rm
  S7["S7 · <b>CompleteFollowUp</b><br/>CRM-013"]:::cmd
  E7("FollowUpCompleted"):::evt
  S8{"S8 · Wynik zmienia<br/>pipeline?"}:::dec
  S9(["S9 · Handlowiec wykonuje komendę w Sales Pipeline<br/>QualifyLead, RejectLead, ChangeOpportunityStage<br/>proces 1 — bez automatu"]):::actor
  S10(["S10 · Historia: szczegóły leada, Customer 360"]):::actor
  IE("BackofficeOrderReturnedToSalesIntegrationEvent<br/>proces 5"):::ie
  S11[/"S11 · Polityka: follow-up po zwrocie zamówienia"/]:::pol
  Q9{{"Q-09: tworzyć FollowUp automatycznie?"}}:::q
  R1[["SalespersonDashboard · CRM-031<br/>SalesManagerDashboard · CRM-032"]]:::rm

  S1 --> S2
  S2 -- "kontakt" --> S3 --> E3 --> S8
  S2 -- "notatka" --> S4 --> E4 --> S10
  S2 -- "kolejny kontakt" --> S5 --> E5 --> S6 --> S7 --> E7 --> S8
  S8 -- "tak" --> S9 --> S10
  S8 -- "nie, zaplanuj kontakt" --> S5
  S8 -- "nie" --> S10
  IE -.-> S11 -. "po decyzji" .-> S5
  Q9 -.- S11
  E3 & E5 & E7 -. "projekcje" .-> R1

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

Plik PNG: [`diagrams/png/04_process_03_sales_activity.png`](../../diagrams/png/04_process_03_sales_activity.png).

