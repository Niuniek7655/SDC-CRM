# Proces 1 — Obsługa leada i pipeline sprzedażowego

**Cel:** od nowego tematu sprzedażowego do wygranej lub przegranej szansy.
**Konteksty:** Sales Pipeline, Sales Activities, Customer Management, Reporting & KPI.
**Agregaty:** `Lead`, `Opportunity`, `SalesActivity`. **Backlog:** CRM-001, CRM-002, CRM-004–006, CRM-010–012, CRM-014–018.
Wersja maszynowa: [`processes.json`](processes.json) (`process_01_lead_pipeline`).

Oznaczenia: ✔ zaimplementowane, ◐ częściowo, ○ planowane, ❓ do decyzji.

## Kroki

| Krok | Typ | Opis | Kontekst / agregat | Komenda | Zdarzenia | Stan | Story |
|---|---|---|---|---|---|---|---|
| S1 | start | Handlowiec ma nowy temat sprzedażowy (źródło: telefon, formularz, polecenie, kampania, targi) | Sales Pipeline | — | — | — | — |
| S2 | komenda | Rejestracja leada z minimalnymi danymi klienta i osoby kontaktowej; właściciel z tokena | Sales Pipeline / `Lead` | `RegisterLead` | `LeadRegistered` | ✔ | CRM-001 |
| S3 | komenda | Opcjonalnie: menedżer zmienia właściciela leada | Sales Pipeline / `Lead` | `AssignLeadToSalesperson` | `LeadAssigned` | ○ | CRM-004 |
| S4 | komenda | Pierwszy kontakt i notatka ([proces 3](process_03_sales_activity.md)) | Sales Activities / `SalesActivity` | `LogSalesActivity`, `AddSalesNote` | `SalesActivityLogged`, `SalesNoteAdded` | ○ | CRM-010, CRM-011 |
| S5 | decyzja | Czy lead jest wart dalszej pracy? | — | — | — | — | — |
| S6 | komenda | Odrzucenie z podaniem powodu | Sales Pipeline / `Lead` | `RejectLead` | `LeadRejected` | ◐ | CRM-006 |
| S7 | komenda | Kwalifikacja leada (odrzuconego nie można zakwalifikować) | Sales Pipeline / `Lead` | `QualifyLead` | `LeadQualified` | ◐ | CRM-005 |
| S8 | polityka | Po `LeadQualified` utwórz szansę sprzedaży | Sales Pipeline | — | — | ○ | CRM-005 |
| S9 | komenda | Utworzenie szansy: powiązanie z leadem i klientem (Q-01), pierwszy etap „Nowa szansa” | Sales Pipeline / `Opportunity` | `CreateOpportunityFromLead` | `OpportunityCreated` | ○ | CRM-014 |
| S10 | komenda | Zmiana etapu pipeline; każda zmiana w historii | Sales Pipeline / `Opportunity` | `ChangeOpportunityStage` | `OpportunityStageChanged` | ○ | CRM-015 |
| S11 | komenda | Zaplanowanie kolejnego kontaktu ([proces 3](process_03_sales_activity.md)) | Sales Activities / `SalesActivity` | `ScheduleFollowUp` | `FollowUpScheduled` | ○ | CRM-012 |
| S12 | decyzja | Wynik sprzedaży: w toku, przegrana czy wygrana? | — | — | — | — | — |
| S13 | komenda | Przegrana z powodem (`LostReason`) | Sales Pipeline / `Opportunity` | `LoseOpportunity` | `OpportunityLost` | ○ | CRM-017 |
| S14 | komenda | Wygrana | Sales Pipeline / `Opportunity` | `WinOpportunity` | `OpportunityWon` | ○ | CRM-016 |
| S15 | koniec | Handlowiec może utworzyć zamówienie ([proces 4](process_04_sales_finalization_order_capture.md)) | Order Capture | — | — | — | CRM-018 |

**Read modele:** Moje leady (`GetMyLeadsQuery`, CRM-002) ✔ — zapytanie bezpośrednie; `SalespersonDashboard` (CRM-031)
i `SalesManagerDashboard` (CRM-032) ○ — projekcje ze zdarzeń.

## Reguły biznesowe

- ✔ Lead ma nazwę firmy, osobę kontaktową i właściciela; właściciel pochodzi z tokena.
- ✔ Odrzucony lead nie może zostać zakwalifikowany; odrzucenie wymaga powodu.
- ○ Temat/nazwa leada, priorytet, co najmniej telefon albo e-mail; status „W kontakcie” — Q-03.
- ○ Szansa jest powiązana z leadem i klientem, ma właściciela i pierwszy etap; przegrana wymaga `LostReason`.
- ○ Wygranej szansy nie można przywrócić do zwykłego etapu bez specjalnych uprawnień (Q-18).
- Zamówienie **nie powstaje automatycznie** po wygranej — tworzy je handlowiec (CRM-016, CRM-018).

**Otwarte pytania:** Q-01, Q-02, Q-03, Q-13, Q-18, Q-19 ([`open_questions.md`](../open_questions.md)).

## Diagram

<!-- diagram: 02_process_01_lead_pipeline -->
```mermaid
flowchart TB
  classDef actor fill:#f1f5f9,stroke:#475569,color:#0f172a
  classDef cmd fill:#dbeafe,stroke:#1e40af,color:#0f172a
  classDef cmdDone fill:#dbeafe,stroke:#16a34a,stroke-width:3px,color:#0f172a
  classDef cmdPart fill:#dbeafe,stroke:#d97706,stroke-width:3px,stroke-dasharray:6 3,color:#0f172a
  classDef evt fill:#ffedd5,stroke:#c2410c,color:#0f172a
  classDef evtDone fill:#ffedd5,stroke:#16a34a,stroke-width:3px,color:#0f172a
  classDef evtPart fill:#ffedd5,stroke:#d97706,stroke-width:3px,stroke-dasharray:6 3,color:#0f172a
  classDef pol fill:#ede9fe,stroke:#6d28d9,color:#0f172a
  classDef rm fill:#dcfce7,stroke:#15803d,color:#0f172a
  classDef rmDone fill:#dcfce7,stroke:#16a34a,stroke-width:3px,color:#0f172a
  classDef q fill:#fee2e2,stroke:#b91c1c,stroke-dasharray:5 3,color:#7f1d1d
  classDef dec fill:#ffffff,stroke:#334155,color:#0f172a

  S1(["S1 · Handlowiec: nowy temat sprzedażowy<br/>źródło: telefon, formularz, polecenie, kampania, targi"]):::actor
  S2["S2 · <b>RegisterLead</b><br/>Lead · CRM-001 · POST /api/leads"]:::cmdDone
  E2("LeadRegistered"):::evtDone
  R1[["Moje leady — GetMyLeadsQuery<br/>CRM-002 · GET /api/leads/mine"]]:::rmDone
  S3["S3 · <b>AssignLeadToSalesperson</b><br/>SalesManager · CRM-004"]:::cmd
  E3("LeadAssigned"):::evt
  S4["S4 · <b>LogSalesActivity</b> / <b>AddSalesNote</b><br/>pierwszy kontakt · proces 3"]:::cmd
  S5{"S5 · Lead wart<br/>dalszej pracy?"}:::dec
  S6["S6 · <b>RejectLead</b><br/>wymagany powód · CRM-006"]:::cmdPart
  E6("LeadRejected"):::evtPart
  S7["S7 · <b>QualifyLead</b><br/>odrzuconego nie można · CRM-005"]:::cmdPart
  E7("LeadQualified"):::evtPart
  S8[/"S8 · Polityka: po kwalifikacji utwórz szansę"/]:::pol
  Q1{{"Q-01: klient z danych leada<br/>czy istniejący klient?"}}:::q
  S9["S9 · <b>CreateOpportunityFromLead</b><br/>CustomerId, etap Nowa szansa · CRM-014"]:::cmd
  E9("OpportunityCreated"):::evt
  S10["S10 · <b>ChangeOpportunityStage</b><br/>historia etapów · CRM-015"]:::cmd
  E10("OpportunityStageChanged"):::evt
  S11["S11 · <b>ScheduleFollowUp</b><br/>kolejny kontakt · proces 3"]:::cmd
  S12{"S12 · Wynik<br/>sprzedaży?"}:::dec
  S13["S13 · <b>LoseOpportunity</b><br/>wymagany LostReason · CRM-017"]:::cmd
  E13("OpportunityLost"):::evt
  S14["S14 · <b>WinOpportunity</b><br/>CRM-016"]:::cmd
  E14("OpportunityWon"):::evt
  S15(["S15 · Handlowiec tworzy zamówienie<br/>CreateOrderFromOpportunity · proces 4"]):::actor
  R2[["SalespersonDashboard · CRM-031<br/>SalesManagerDashboard · CRM-032"]]:::rm

  S1 --> S2 --> E2
  E2 --> R1
  E2 -- "opcjonalnie" --> S3 --> E3
  E2 --> S4 --> S5
  S5 -- "nie" --> S6 --> E6
  S5 -- "tak" --> S7 --> E7 --> S8 --> S9 --> E9
  Q1 -.- S9
  E9 --> S10 --> E10 --> S11 --> S12
  S12 -- "w toku" --> S10
  S12 -- "przegrana" --> S13 --> E13
  S12 -- "wygrana" --> S14 --> E14 --> S15
  E6 & E13 & E14 -. "projekcje" .-> R2

  subgraph LEGEND["Legenda"]
    direction LR
    L1["komenda"]:::cmd
    L2("zdarzenie domenowe"):::evt
    L3[/"polityka"/]:::pol
    L4[["read model"]]:::rm
    L5{{"pytanie Q-xx"}}:::q
    L6["zaimplementowane"]:::cmdDone
    L7["częściowo"]:::cmdPart
  end
```

Plik PNG: [`diagrams/png/02_process_01_lead_pipeline.png`](../../diagrams/png/02_process_01_lead_pipeline.png).

