# Backlog user stories — CRM sprzedaży i backoffice

## Cel dokumentu

Ten dokument zawiera listę user stories do realizacji projektu CRM.  
Każda story ma miejsce na oznaczenie statusu realizacji.

## Legenda statusów

Dla każdej story można uzupełnić pole `Status`.

Dozwolone statusy:

- `Backlog`
- `Ready`
- `In Progress`
- `Blocked`
- `Review`
- `Done`

Alternatywnie można zaznaczyć checkbox przy statusie:

```md
- [x] Done
- [ ] In Progress
```

---

# EPIC 1 — Lead Management

## CRM-001 — Rejestracja nowego leada

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Lead

### User Story

Jako handlowiec  
chcę zarejestrować nowego leada z podstawowymi danymi klienta i osoby kontaktowej  
aby rozpocząć proces sprzedaży.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można dodać temat/nazwę leada.
- Można dodać źródło leada.
- Można ustawić priorytet.
- Można podać podstawowe dane klienta.
- Można podać dane osoby kontaktowej.
- System przypisuje zalogowanego handlowca jako właściciela.
- Nowy lead otrzymuje status `Nowy`.
- Lead pojawia się na liście moich leadów.

### Reguły biznesowe

- Lead musi mieć nazwę/temat.
- Lead musi mieć przynajmniej jeden kontakt: telefon albo e-mail.
- Lead powinien mieć właściciela.
- System powinien ostrzec o potencjalnym duplikacie klienta.

### Komendy domenowe

- `CreateLead`

### Zdarzenia domenowe

- `LeadCreated`

---

## CRM-002 — Lista moich leadów

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Lead

### User Story

Jako handlowiec  
chcę widzieć listę moich leadów  
aby wiedzieć, którymi tematami sprzedażowymi mam się zająć.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Widzę tylko leady przypisane do mnie, o ile nie mam szerszych uprawnień.
- Lista pokazuje nazwę leada, klienta, priorytet, status i datę utworzenia.
- Mogę filtrować po statusie.
- Mogę sortować po dacie utworzenia i priorytecie.
- Mogę przejść do szczegółów leada.

---

## CRM-003 — Szczegóły leada

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Lead

### User Story

Jako handlowiec  
chcę wejść w szczegóły leada  
aby zobaczyć dane klienta, osobę kontaktową, status i historię aktywności.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Widzę dane leada.
- Widzę dane klienta lub potencjalnego klienta.
- Widzę osobę kontaktową.
- Widzę aktualny status.
- Widzę właściciela leada.
- Widzę historię aktywności.
- Mogę dodać notatkę lub kontakt z poziomu szczegółów.

---

## CRM-004 — Przypisanie leada do handlowca

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Menedżer sprzedaży  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Lead

### User Story

Jako menedżer sprzedaży  
chcę przypisać lead do handlowca  
aby zapewnić odpowiedzialność za jego obsługę.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Menedżer może zmienić właściciela leada.
- Nowy właściciel widzi lead na swojej liście.
- Poprzedni właściciel traci dostęp, jeżeli nie ma uprawnień zespołowych.
- Zmiana właściciela jest widoczna w historii.

### Komendy domenowe

- `AssignLeadToSalesperson`

### Zdarzenia domenowe

- `LeadAssigned`

---

## CRM-005 — Kwalifikacja leada

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Lead / Opportunity

### User Story

Jako handlowiec  
chcę zakwalifikować lead jako szansę sprzedaży  
aby prowadzić temat dalej w pipeline.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Mogę oznaczyć lead jako zakwalifikowany.
- Po kwalifikacji powstaje szansa sprzedaży.
- Szansa sprzedaży jest powiązana z leadem.
- Szansa otrzymuje pierwszy etap pipeline.
- Historia leada pokazuje informację o kwalifikacji.

### Komendy domenowe

- `QualifyLead`

### Zdarzenia domenowe

- `LeadQualified`
- `OpportunityCreated`

---

## CRM-006 — Odrzucenie leada

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Lead

### User Story

Jako handlowiec  
chcę odrzucić lead z podaniem powodu  
aby zakończyć obsługę nieperspektywicznego tematu.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Mogę oznaczyć lead jako odrzucony.
- Muszę podać powód odrzucenia.
- Odrzucony lead nie pojawia się jako aktywny.
- Powód odrzucenia jest widoczny w historii.

### Komendy domenowe

- `RejectLead`

### Zdarzenia domenowe

- `LeadRejected`

---

# EPIC 2 — Customer Management

## CRM-007 — Utworzenie klienta

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Customer Management  
**Agregat:** Customer

### User Story

Jako handlowiec  
chcę utworzyć kartę klienta  
aby zapisywać dane kontaktowe i historię relacji.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można podać nazwę klienta.
- Można podać telefon, e-mail i opcjonalnie NIP.
- System waliduje wymagane pola.
- System ostrzega o potencjalnym duplikacie.
- Klient jest widoczny na liście klientów zgodnie z uprawnieniami.

### Komendy domenowe

- `CreateCustomer`

### Zdarzenia domenowe

- `CustomerCreated`

---

## CRM-008 — Dodanie osoby kontaktowej

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Customer Management  
**Agregat:** Customer

### User Story

Jako handlowiec  
chcę dodać osobę kontaktową do klienta  
aby wiedzieć, z kim prowadzić rozmowy.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Osoba kontaktowa ma imię i nazwisko albo nazwę.
- Można dodać telefon i e-mail.
- Można oznaczyć osobę jako główny kontakt.
- Klient może mieć wiele osób kontaktowych.
- Dane kontaktowe są widoczne na karcie klienta.

### Komendy domenowe

- `AddContactPerson`

### Zdarzenia domenowe

- `ContactPersonAdded`

---

## CRM-009 — Widok 360 klienta

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec / Menedżer sprzedaży  
**Kontekst DDD:** Customer Management  
**Agregat:** Customer

### User Story

Jako handlowiec  
chcę widzieć pełny widok klienta  
aby znać jego historię, aktywności, szanse i zamówienia.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Widzę dane klienta.
- Widzę osoby kontaktowe.
- Widzę leady i szanse sprzedaży.
- Widzę kontakty i notatki.
- Widzę zamówienia klienta.
- Elementy historii są posortowane od najnowszych.

---

# EPIC 3 — Sales Activities

## CRM-010 — Dodanie notatki sprzedażowej

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Activities  
**Agregat:** SalesActivity

### User Story

Jako handlowiec  
chcę dodać notatkę po rozmowie  
aby zachować ustalenia z klientem.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Notatka ma treść, autora i datę.
- Notatka może być powiązana z klientem, leadem albo szansą.
- Notatka jest widoczna w historii.
- Notatki nie można usunąć bez odpowiednich uprawnień.

### Komendy domenowe

- `AddSalesNote`

### Zdarzenia domenowe

- `SalesNoteAdded`

---

## CRM-011 — Zarejestrowanie kontaktu z klientem

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Activities  
**Agregat:** SalesActivity

### User Story

Jako handlowiec  
chcę zarejestrować wykonany kontakt z klientem  
aby system pokazywał historię relacji.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Kontakt ma typ: telefon, e-mail, spotkanie lub inny.
- Kontakt ma datę i autora.
- Kontakt może mieć notatkę.
- Kontakt jest widoczny na karcie klienta.
- Kontakt jest widoczny w aktywności handlowca.

### Komendy domenowe

- `LogSalesActivity`

### Zdarzenia domenowe

- `SalesActivityLogged`

---

## CRM-012 — Zaplanowanie follow-upu

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Activities  
**Agregat:** SalesActivity

### User Story

Jako handlowiec  
chcę zaplanować kolejny kontakt z klientem  
aby nie zapomnieć o kontynuacji sprzedaży.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można podać termin follow-upu.
- Można dodać opis zadania.
- Follow-up jest przypisany do właściciela.
- Follow-up pojawia się na liście zadań handlowca.
- Zadanie po terminie jest oznaczone jako zaległe.

### Komendy domenowe

- `ScheduleFollowUp`

### Zdarzenia domenowe

- `FollowUpScheduled`

---

## CRM-013 — Lista moich zadań

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Activities  
**Agregat:** SalesActivity

### User Story

Jako handlowiec  
chcę widzieć listę moich follow-upów  
aby zarządzać codzienną pracą.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Widzę zadania dzisiejsze.
- Widzę zadania przyszłe.
- Widzę zadania zaległe.
- Mogę oznaczyć zadanie jako wykonane.
- Mogę przejść z zadania do klienta, leada albo szansy.

---

# EPIC 4 — Sales Pipeline

## CRM-014 — Utworzenie szansy sprzedaży

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Opportunity

### User Story

Jako handlowiec  
chcę utworzyć szansę sprzedaży z zakwalifikowanego leada  
aby prowadzić temat w pipeline.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Szansa jest powiązana z leadem.
- Szansa jest powiązana z klientem.
- Szansa ma właściciela.
- Szansa ma pierwszy etap pipeline.
- Szansa może mieć wartość i prawdopodobieństwo.

### Komendy domenowe

- `CreateOpportunityFromLead`

### Zdarzenia domenowe

- `OpportunityCreated`

---

## CRM-015 — Zmiana etapu szansy sprzedaży

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Opportunity

### User Story

Jako handlowiec  
chcę zmieniać etap szansy sprzedaży  
aby odzwierciedlać aktualny stan rozmów z klientem.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można zmienić etap zgodnie z workflow pipeline.
- Zmiana etapu jest zapisywana w historii.
- Aktualny etap jest widoczny na szczegółach szansy.
- Zmiana etapu aktualizuje raport pipeline.

### Komendy domenowe

- `ChangeOpportunityStage`

### Zdarzenia domenowe

- `OpportunityStageChanged`

---

## CRM-016 — Oznaczenie szansy jako wygranej

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Opportunity

### User Story

Jako handlowiec  
chcę oznaczyć szansę sprzedaży jako wygraną  
aby móc utworzyć zamówienie.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można oznaczyć szansę jako wygraną.
- Wygrana szansa nie może wrócić do zwykłego etapu bez specjalnych uprawnień.
- Po wygraniu pojawia się możliwość utworzenia zamówienia.
- Zdarzenie wygranej trafia do historii.

### Komendy domenowe

- `WinOpportunity`

### Zdarzenia domenowe

- `OpportunityWon`

---

## CRM-017 — Oznaczenie szansy jako przegranej

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Sales Pipeline  
**Agregat:** Opportunity

### User Story

Jako handlowiec  
chcę oznaczyć szansę jako przegraną z podaniem powodu  
aby zamknąć temat i umożliwić analizę przyczyn utraty sprzedaży.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można oznaczyć szansę jako przegraną.
- Powód przegranej jest wymagany.
- Przegrana szansa nie pojawia się jako aktywna.
- Powód przegranej jest widoczny w raporcie.

### Komendy domenowe

- `LoseOpportunity`

### Zdarzenia domenowe

- `OpportunityLost`

---

# EPIC 5 — Order Capture

## CRM-018 — Utworzenie zamówienia z wygranej szansy

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Order Capture  
**Agregat:** SalesOrder

### User Story

Jako handlowiec  
chcę utworzyć zamówienie z wygranej szansy sprzedaży  
aby przekazać sprzedaż do realizacji.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Zamówienie jest powiązane z klientem.
- Zamówienie jest powiązane z wygraną szansą.
- Zamówienie ma status roboczy.
- Dane klienta są wstępnie uzupełnione.
- Zamówienie można zapisać bez przekazywania do backoffice.

### Komendy domenowe

- `CreateOrderFromOpportunity`

### Zdarzenia domenowe

- `SalesOrderCreated`

---

## CRM-019 — Walidacja kompletności zamówienia

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Order Capture  
**Agregat:** SalesOrder

### User Story

Jako handlowiec  
chcę wiedzieć, czy zamówienie jest kompletne  
aby uniknąć zwrotu z backoffice.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- System pokazuje brakujące wymagane pola.
- Nie można przekazać niekompletnego zamówienia.
- Komunikaty walidacyjne są zrozumiałe biznesowo.
- Lista błędów jest widoczna przed przekazaniem do backoffice.

---

## CRM-020 — Przekazanie zamówienia do backoffice

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Order Capture / Order Backoffice  
**Agregat:** SalesOrder / BackofficeOrderCase

### User Story

Jako handlowiec  
chcę przekazać kompletne zamówienie do backoffice  
aby rozpocząć proces realizacji.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Zamówienie musi być kompletne.
- Zamówienie zmienia status na przekazane do backoffice.
- Powstaje sprawa backoffice albo wpis w kolejce backoffice.
- Handlowiec nie może dowolnie edytować przekazanego zamówienia.
- Backoffice widzi zamówienie na swojej kolejce.

### Komendy domenowe

- `SubmitOrderToBackoffice`

### Zdarzenia domenowe

- `OrderSubmittedToBackoffice`

---

## CRM-021 — Podgląd statusu zamówienia przez handlowca

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Order Capture / Order Backoffice  
**Agregat:** SalesOrder / BackofficeOrderCase

### User Story

Jako handlowiec  
chcę widzieć status zamówienia przekazanego do backoffice  
aby móc poinformować klienta o postępie realizacji.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Widzę aktualny status zamówienia.
- Widzę datę ostatniej zmiany.
- Widzę komentarze widoczne dla sprzedaży.
- Widzę informację, czy zamówienie wymaga mojej reakcji.

---

# EPIC 6 — Order Backoffice

## CRM-022 — Kolejka nowych zamówień backoffice

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Pracownik backoffice  
**Kontekst DDD:** Order Backoffice  
**Agregat:** BackofficeOrderCase

### User Story

Jako pracownik backoffice  
chcę widzieć kolejkę nowych zamówień  
aby rozpocząć ich obsługę.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Lista pokazuje zamówienia przekazane do backoffice.
- Widoczny jest klient, handlowiec, data przekazania i status.
- Można filtrować po statusie.
- Można sortować po dacie przekazania.
- Można wejść w szczegóły zamówienia.

---

## CRM-023 — Przypisanie zamówienia do pracownika backoffice

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Menedżer backoffice  
**Kontekst DDD:** Order Backoffice  
**Agregat:** BackofficeOrderCase

### User Story

Jako menedżer backoffice  
chcę przypisać zamówienie do pracownika  
aby zapewnić odpowiedzialność za obsługę.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można przypisać zamówienie do pracownika backoffice.
- Przypisana osoba widzi zamówienie na swojej liście.
- Zmiana przypisania jest zapisana w historii.
- Można zmienić przypisaną osobę.

### Komendy domenowe

- `AssignBackofficeOrder`

### Zdarzenia domenowe

- `BackofficeOrderAssigned`

---

## CRM-024 — Zmiana statusu zamówienia przez backoffice

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Pracownik backoffice  
**Kontekst DDD:** Order Backoffice  
**Agregat:** BackofficeOrderCase

### User Story

Jako pracownik backoffice  
chcę zmienić status zamówienia  
aby odzwierciedlić aktualny etap obsługi.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można zmienić status zgodnie z workflow.
- Każda zmiana statusu trafia do historii.
- Przy statusie `Zablokowane` wymagany jest powód.
- Przy statusie `Zrealizowane` zapisywana jest data zakończenia.

### Komendy domenowe

- `ChangeBackofficeOrderStatus`

### Zdarzenia domenowe

- `BackofficeOrderStatusChanged`

---

## CRM-025 — Dodanie komentarza backoffice

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Pracownik backoffice  
**Kontekst DDD:** Order Backoffice  
**Agregat:** BackofficeOrderCase

### User Story

Jako pracownik backoffice  
chcę dodać komentarz do zamówienia  
aby udokumentować postęp lub problem w obsłudze.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Komentarz ma autora i datę.
- Komentarz może być wewnętrzny albo widoczny dla sprzedaży.
- Komentarz jest widoczny w historii zamówienia.
- Komentarz nie znika po zmianie statusu.

---

## CRM-026 — Zwrot zamówienia do handlowca

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Pracownik backoffice  
**Kontekst DDD:** Order Backoffice  
**Agregat:** BackofficeOrderCase

### User Story

Jako pracownik backoffice  
chcę zwrócić zamówienie do handlowca z komentarzem  
aby handlowiec mógł uzupełnić brakujące lub błędne dane.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Zwrot wymaga komentarza.
- Handlowiec widzi zamówienie wymagające reakcji.
- Po poprawie handlowiec może ponownie przekazać zamówienie.
- Historia zwrotów jest widoczna.

### Komendy domenowe

- `ReturnOrderToSales`

### Zdarzenia domenowe

- `OrderReturnedToSales`

---

## CRM-027 — Zamknięcie zamówienia jako zrealizowanego

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Pracownik backoffice  
**Kontekst DDD:** Order Backoffice  
**Agregat:** BackofficeOrderCase

### User Story

Jako pracownik backoffice  
chcę oznaczyć zamówienie jako zrealizowane  
aby zakończyć proces obsługi.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Zamówienie otrzymuje status `Zrealizowane`.
- Zapisywana jest data zakończenia.
- Zamówienie nie pojawia się jako aktywne.
- Dane trafiają do raportów.

### Komendy domenowe

- `CompleteOrder`

### Zdarzenia domenowe

- `OrderCompleted`

---

# EPIC 7 — Identity & Access

## CRM-028 — Logowanie użytkownika

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Każdy użytkownik  
**Kontekst DDD:** Identity & Access  
**Agregat:** User

### User Story

Jako użytkownik systemu  
chcę zalogować się do CRM  
aby korzystać z funkcji zgodnych z moją rolą.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Użytkownik może się zalogować.
- Użytkownik ma przypisaną rolę.
- Użytkownik bez konta nie ma dostępu do systemu.
- Po zalogowaniu użytkownik widzi funkcje zgodne z rolą.

---

## CRM-029 — Uprawnienia według ról

**Status:** Backlog  
**Priorytet:** Must Have  
**Rola:** Administrator  
**Kontekst DDD:** Identity & Access  
**Agregat:** Role

### User Story

Jako administrator  
chcę kontrolować uprawnienia według ról  
aby użytkownicy mieli dostęp tylko do właściwych funkcji.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Handlowiec widzi funkcje sprzedażowe.
- Menedżer sprzedaży widzi raporty i pipeline zespołu.
- Backoffice widzi kolejkę zamówień.
- Administrator może zarządzać użytkownikami.
- Dostęp do akcji jest sprawdzany po stronie backendu.

---

## CRM-030 — Dziennik audytu

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Administrator  
**Kontekst DDD:** Identity & Access  
**Agregat:** AuditLog

### User Story

Jako administrator  
chcę widzieć historię istotnych zmian  
aby móc sprawdzić, kto i kiedy zmienił dane.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Historia obejmuje zmianę statusów.
- Historia obejmuje przypisania.
- Historia obejmuje edycję kluczowych danych.
- Wpis zawiera użytkownika, datę, typ operacji i identyfikator obiektu.

---

# EPIC 8 — Reporting & KPI

## CRM-031 — Dashboard handlowca

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Handlowiec  
**Kontekst DDD:** Reporting & KPI  
**Model:** Read Model

### User Story

Jako handlowiec  
chcę widzieć moje zadania, leady i aktywne szanse  
aby wiedzieć, czym powinienem się zająć.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Widzę aktywne leady.
- Widzę aktywne szanse.
- Widzę dzisiejsze follow-upy.
- Widzę zaległe follow-upy.
- Widzę zamówienia zwrócone do poprawy.

---

## CRM-032 — Dashboard menedżera sprzedaży

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Menedżer sprzedaży  
**Kontekst DDD:** Reporting & KPI  
**Model:** Read Model

### User Story

Jako menedżer sprzedaży  
chcę widzieć pipeline zespołu  
aby zarządzać wynikiem sprzedaży.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Widzę liczbę szans na etapach.
- Widzę wartość pipeline.
- Widzę wygrane i przegrane szanse.
- Widzę aktywność handlowców.
- Mogę filtrować po handlowcu i okresie.

---

## CRM-033 — Raport backoffice

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Menedżer backoffice  
**Kontekst DDD:** Reporting & KPI  
**Model:** Read Model

### User Story

Jako menedżer backoffice  
chcę widzieć status obsługi zamówień  
aby kontrolować obciążenie i opóźnienia.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Widzę liczbę zamówień według statusów.
- Widzę zamówienia zablokowane.
- Widzę zamówienia zwrócone do sprzedaży.
- Widzę średni czas obsługi.
- Mogę filtrować po pracowniku i statusie.

---

## CRM-034 — Eksport raportów do CSV/XLSX

**Status:** Backlog  
**Priorytet:** Could Have  
**Rola:** Menedżer sprzedaży / Menedżer backoffice  
**Kontekst DDD:** Reporting & KPI  
**Model:** Read Model

### User Story

Jako menedżer  
chcę wyeksportować dane raportowe  
aby analizować je poza systemem.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Eksport jest dostępny dla uprawnionych użytkowników.
- Eksport respektuje filtry.
- Eksport respektuje uprawnienia użytkownika.
- Plik zawiera nagłówki kolumn i dane zgodne z widokiem raportu.

---

# EPIC 9 — Administration

## CRM-035 — Zarządzanie użytkownikami

**Status:** Backlog  
**Priorytet:** Should Have  
**Rola:** Administrator  
**Kontekst DDD:** Identity & Access  
**Agregat:** User

### User Story

Jako administrator  
chcę dodawać, edytować i dezaktywować użytkowników  
aby kontrolować dostęp do systemu.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można dodać użytkownika.
- Można przypisać rolę.
- Można dezaktywować użytkownika.
- Dezaktywowany użytkownik nie może się zalogować.
- Zmiana użytkownika jest widoczna w audycie.

---

## CRM-036 — Konfiguracja słowników systemowych

**Status:** Backlog  
**Priorytet:** Could Have  
**Rola:** Administrator  
**Kontekst DDD:** Administration  
**Agregat:** Configuration

### User Story

Jako administrator  
chcę zarządzać podstawowymi słownikami systemowymi  
aby dopasować system do procesu firmy.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Można zarządzać źródłami leadów.
- Można zarządzać powodami przegranej.
- Można zarządzać priorytetami.
- Można zarządzać podstawowymi statusami, jeżeli są konfigurowalne.
- Nie można usunąć wartości używanej przez istniejące dane bez migracji lub dezaktywacji.

---

# EPIC 10 — Integrations

## CRM-037 — Definicja kontraktu integracji ERP/fakturowanie

**Status:** Backlog  
**Priorytet:** Could Have  
**Rola:** Product Owner / Administrator  
**Kontekst DDD:** Integrations  
**Model:** Anti-Corruption Layer

### User Story

Jako właściciel biznesowy  
chcę zdefiniować dane przekazywane do ERP lub fakturowania  
aby zamówienia mogły być obsługiwane w zewnętrznym systemie.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- Określono, które dane zamówienia są wymagane.
- Określono moment wysyłki danych.
- Określono reakcję na błąd integracji.
- Określono, czy integracja jest jedno- czy dwukierunkowa.

---

## CRM-038 — Wysłanie zamówienia do ERP/fakturowania

**Status:** Backlog  
**Priorytet:** Could Have  
**Rola:** Backoffice / System  
**Kontekst DDD:** Integrations  
**Model:** Anti-Corruption Layer

### User Story

Jako backoffice  
chcę przekazać zatwierdzone lub zrealizowane zamówienie do ERP  
aby uruchomić dalszy proces realizacyjny lub księgowy.

### Status realizacji

- [x] Backlog
- [ ] Ready
- [ ] In Progress
- [ ] Blocked
- [ ] Review
- [ ] Done

### Kryteria akceptacji

- System wysyła dane zamówienia do systemu zewnętrznego.
- System zapisuje status integracji.
- Błąd integracji jest widoczny dla uprawnionych użytkowników.
- Można ponowić wysyłkę po błędzie.

### Zdarzenia wejściowe

- `OrderCompleted`
- `OrderApprovedForInvoicing`

---

# Widok zbiorczy backlogu

| ID | Epic | Story | Priorytet | Status |
|---|---|---|---|---|
| CRM-001 | Lead Management | Rejestracja nowego leada | Must Have | Backlog |
| CRM-002 | Lead Management | Lista moich leadów | Must Have | Backlog |
| CRM-003 | Lead Management | Szczegóły leada | Must Have | Backlog |
| CRM-004 | Lead Management | Przypisanie leada do handlowca | Should Have | Backlog |
| CRM-005 | Lead Management | Kwalifikacja leada | Must Have | Backlog |
| CRM-006 | Lead Management | Odrzucenie leada | Should Have | Backlog |
| CRM-007 | Customer Management | Utworzenie klienta | Must Have | Backlog |
| CRM-008 | Customer Management | Dodanie osoby kontaktowej | Must Have | Backlog |
| CRM-009 | Customer Management | Widok 360 klienta | Must Have | Backlog |
| CRM-010 | Sales Activities | Dodanie notatki sprzedażowej | Must Have | Backlog |
| CRM-011 | Sales Activities | Zarejestrowanie kontaktu z klientem | Must Have | Backlog |
| CRM-012 | Sales Activities | Zaplanowanie follow-upu | Must Have | Backlog |
| CRM-013 | Sales Activities | Lista moich zadań | Must Have | Backlog |
| CRM-014 | Sales Pipeline | Utworzenie szansy sprzedaży | Must Have | Backlog |
| CRM-015 | Sales Pipeline | Zmiana etapu szansy sprzedaży | Must Have | Backlog |
| CRM-016 | Sales Pipeline | Oznaczenie szansy jako wygranej | Must Have | Backlog |
| CRM-017 | Sales Pipeline | Oznaczenie szansy jako przegranej | Should Have | Backlog |
| CRM-018 | Order Capture | Utworzenie zamówienia z wygranej szansy | Must Have | Backlog |
| CRM-019 | Order Capture | Walidacja kompletności zamówienia | Must Have | Backlog |
| CRM-020 | Order Capture | Przekazanie zamówienia do backoffice | Must Have | Backlog |
| CRM-021 | Order Capture | Podgląd statusu zamówienia przez handlowca | Should Have | Backlog |
| CRM-022 | Order Backoffice | Kolejka nowych zamówień backoffice | Must Have | Backlog |
| CRM-023 | Order Backoffice | Przypisanie zamówienia do pracownika backoffice | Must Have | Backlog |
| CRM-024 | Order Backoffice | Zmiana statusu zamówienia przez backoffice | Must Have | Backlog |
| CRM-025 | Order Backoffice | Dodanie komentarza backoffice | Should Have | Backlog |
| CRM-026 | Order Backoffice | Zwrot zamówienia do handlowca | Must Have | Backlog |
| CRM-027 | Order Backoffice | Zamknięcie zamówienia jako zrealizowanego | Must Have | Backlog |
| CRM-028 | Identity & Access | Logowanie użytkownika | Must Have | Backlog |
| CRM-029 | Identity & Access | Uprawnienia według ról | Must Have | Backlog |
| CRM-030 | Identity & Access | Dziennik audytu | Should Have | Backlog |
| CRM-031 | Reporting & KPI | Dashboard handlowca | Should Have | Backlog |
| CRM-032 | Reporting & KPI | Dashboard menedżera sprzedaży | Should Have | Backlog |
| CRM-033 | Reporting & KPI | Raport backoffice | Should Have | Backlog |
| CRM-034 | Reporting & KPI | Eksport raportów do CSV/XLSX | Could Have | Backlog |
| CRM-035 | Administration | Zarządzanie użytkownikami | Should Have | Backlog |
| CRM-036 | Administration | Konfiguracja słowników systemowych | Could Have | Backlog |
| CRM-037 | Integrations | Definicja kontraktu integracji ERP/fakturowanie | Could Have | Backlog |
| CRM-038 | Integrations | Wysłanie zamówienia do ERP/fakturowania | Could Have | Backlog |
