# Słownik języka wszechobecnego — CRM sprzedaży i backoffice

## Cel dokumentu

Ten dokument opisuje język domenowy dla systemu CRM.  
Ma służyć jako wspólny słownik dla klienta biznesowego, analityka, project managera, frontend developera, backend developera i testera.

W podejściu DDD nazwy używane w kodzie, interfejsie użytkownika, dokumentacji i rozmowach biznesowych powinny być możliwie spójne.

---

# 1. Konteksty domenowe

## 1.1. Customer Management

Kontekst odpowiedzialny za dane klientów, osoby kontaktowe i widok relacji z klientem.

Typowe pytania biznesowe:

- Kim jest klient?
- Jakie mamy dane kontaktowe?
- Kto jest osobą kontaktową?
- Jaka jest historia kontaktów z klientem?
- Jakie leady, szanse i zamówienia są powiązane z klientem?

## 1.2. Sales Pipeline

Kontekst odpowiedzialny za prowadzenie sprzedaży od leada do wygranej lub przegranej szansy.

Typowe pytania biznesowe:

- Na jakim etapie jest sprzedaż?
- Kto odpowiada za temat?
- Jaka jest wartość potencjalnej sprzedaży?
- Jaki jest priorytet?
- Dlaczego sprzedaż została przegrana?

## 1.3. Sales Activities

Kontekst odpowiedzialny za codzienną aktywność handlowców: notatki, kontakty i follow-upy.

Typowe pytania biznesowe:

- Kiedy był ostatni kontakt z klientem?
- Co ustalono podczas rozmowy?
- Kiedy trzeba ponownie skontaktować się z klientem?
- Jak aktywnie handlowiec pracuje na swoich leadach?

## 1.4. Order Capture

Kontekst odpowiedzialny za utworzenie zamówienia przez handlowca po finalizacji sprzedaży.

Typowe pytania biznesowe:

- Czy sprzedaż została wygrana?
- Czy zamówienie zawiera komplet danych?
- Czy zamówienie można przekazać do backoffice?
- Kto utworzył zamówienie?

## 1.5. Order Backoffice

Kontekst odpowiedzialny za obsługę zamówienia po stronie backoffice.

Typowe pytania biznesowe:

- Kto obsługuje zamówienie?
- Jaki jest aktualny status realizacji?
- Czy zamówienie jest zablokowane?
- Czy zamówienie wymaga poprawy przez handlowca?
- Czy zamówienie zostało zrealizowane?

## 1.6. Reporting & KPI

Kontekst odpowiedzialny za raportowanie sprzedaży, pipeline, aktywności i obsługi zamówień.

Typowe pytania biznesowe:

- Ile leadów jest aktywnych?
- Jaka jest wartość pipeline?
- Ile zamówień jest w realizacji?
- Które zamówienia są opóźnione?
- Jaką aktywność mają handlowcy?

## 1.7. Identity & Access

Kontekst odpowiedzialny za użytkowników, role i uprawnienia.

Typowe pytania biznesowe:

- Kto może zalogować się do systemu?
- Kto widzi klientów?
- Kto może edytować leady?
- Kto może zmieniać status zamówienia?
- Kto może zarządzać użytkownikami?

## 1.8. Integrations

Kontekst odpowiedzialny za komunikację z zewnętrznymi systemami, np. ERP, fakturowaniem, e-mailem, kalendarzem lub płatnościami.

---

# 2. Pojęcia podstawowe

| Pojęcie | Definicja biznesowa | Przykład / uwagi |
|---|---|---|
| CRM | System wspierający sprzedaż, obsługę klientów, pipeline i backoffice. | Główna aplikacja projektu. |
| Klient | Firma lub osoba, z którą firma prowadzi albo może prowadzić relację handlową. | Może mieć wiele osób kontaktowych. |
| Potencjalny klient | Podmiot, który nie jest jeszcze pełnoprawnym klientem, ale pojawia się w kontekście leada. | Może zostać przekształcony w klienta. |
| Osoba kontaktowa | Osoba po stronie klienta, z którą kontaktuje się handlowiec. | Może być głównym kontaktem. |
| Handlowiec | Użytkownik odpowiedzialny za obsługę leadów, kontakt z klientem i wprowadzanie zamówień. | Około 20 użytkowników w systemie. |
| Menedżer sprzedaży | Użytkownik zarządzający pracą handlowców i pipeline. | Widzi raporty zespołu. |
| Backoffice | Zespół obsługujący zamówienia po ich przekazaniu przez sprzedaż. | Pracuje na kolejce zamówień. |
| Administrator | Użytkownik zarządzający konfiguracją systemu, użytkownikami i uprawnieniami. | Nie musi uczestniczyć w sprzedaży. |

---

# 3. Pojęcia związane ze sprzedażą

| Pojęcie | Definicja biznesowa | Reguły / uwagi |
|---|---|---|
| Lead | Potencjalny temat sprzedażowy wymagający kwalifikacji. | Ma właściciela, status, źródło i priorytet. |
| Źródło leada | Informacja, skąd pochodzi lead. | Np. telefon, formularz, polecenie, kampania, targi. |
| Priorytet leada | Ważność lub pilność obsługi leada. | Np. niski, normalny, wysoki. |
| Właściciel leada | Handlowiec odpowiedzialny za obsługę leada. | Domyślnie osoba tworząca lead. |
| Kwalifikacja leada | Decyzja, czy lead jest wart dalszej pracy sprzedażowej. | Może zakończyć się utworzeniem szansy sprzedaży. |
| Lead odrzucony | Lead, który nie przechodzi kwalifikacji. | Wymaga powodu odrzucenia. |
| Szansa sprzedaży / Opportunity | Zakwalifikowany temat sprzedażowy prowadzony w pipeline. | Ma etap, wartość i prawdopodobieństwo. |
| Pipeline | Uporządkowany proces sprzedażowy składający się z etapów. | Pozwala śledzić postęp sprzedaży. |
| Etap sprzedaży | Aktualny stan szansy sprzedaży w pipeline. | Np. Nowy, Oferta, Negocjacje, Wygrany, Przegrany. |
| Wartość szansy | Szacowana wartość potencjalnej sprzedaży. | Używana w raportach i prognozach. |
| Prawdopodobieństwo wygranej | Szacowana szansa finalizacji sprzedaży. | Może być ustawiane ręcznie lub wynikać z etapu. |
| Wygrana szansa | Szansa zakończona sukcesem. | Może prowadzić do utworzenia zamówienia. |
| Przegrana szansa | Szansa zakończona niepowodzeniem. | Wymaga powodu przegranej. |
| Powód przegranej | Uzasadnienie, dlaczego sprzedaż nie została sfinalizowana. | Np. cena, konkurencja, brak decyzji, brak budżetu. |

---

# 4. Pojęcia związane z aktywnościami handlowca

| Pojęcie | Definicja biznesowa | Reguły / uwagi |
|---|---|---|
| Aktywność sprzedażowa | Każde zarejestrowane działanie handlowca wobec klienta lub leada. | Może być telefonem, e-mailem, spotkaniem itd. |
| Kontakt | Konkretna interakcja z klientem. | Powinien mieć typ, datę i autora. |
| Notatka | Opis ustaleń, obserwacji lub informacji po kontakcie. | Powiązana z klientem, leadem lub szansą. |
| Follow-up | Zaplanowany kolejny kontakt lub zadanie. | Ma termin i właściciela. |
| Zadanie zaległe | Follow-up, którego termin minął i nie został oznaczony jako wykonany. | Powinno być widoczne na dashboardzie handlowca. |
| Historia kontaktów | Chronologiczna lista kontaktów, notatek i aktywności. | Widoczna na karcie klienta lub leada. |

---

# 5. Pojęcia związane z zamówieniami

| Pojęcie | Definicja biznesowa | Reguły / uwagi |
|---|---|---|
| Zamówienie | Formalne zgłoszenie sprzedaży do realizacji. | Tworzone przez handlowca. |
| Zamówienie robocze | Zamówienie zapisane, ale jeszcze nieprzekazane do backoffice. | Handlowiec może je edytować. |
| Kompletność zamówienia | Spełnienie minimalnych wymagań danych potrzebnych do realizacji. | Niekompletne zamówienie nie powinno trafić do backoffice. |
| Przekazanie do backoffice | Moment, w którym handlowiec kończy uzupełnianie zamówienia i przekazuje je do obsługi. | Po przekazaniu edycja powinna być ograniczona. |
| Sprawa backoffice | Proces obsługi zamówienia przez backoffice. | Może mieć przypisanego pracownika i status. |
| Kolejka backoffice | Lista zamówień oczekujących na obsługę lub będących w obsłudze. | Główne miejsce pracy backoffice. |
| Pracownik obsługujący | Osoba z backoffice przypisana do zamówienia. | Odpowiada za realizację. |
| Status zamówienia | Aktualny etap obsługi zamówienia. | Np. Nowe, W weryfikacji, W realizacji, Zrealizowane. |
| Zamówienie zablokowane | Zamówienie, którego nie można dalej realizować bez wyjaśnienia. | Wymaga powodu blokady. |
| Zwrot do handlowca | Cofnięcie zamówienia do sprzedaży z prośbą o uzupełnienie lub poprawę. | Wymaga komentarza. |
| Zamówienie zrealizowane | Zamówienie zakończone przez backoffice. | Powinno mieć datę zamknięcia. |
| Zamówienie anulowane | Zamówienie, które nie będzie realizowane. | Wymaga powodu anulowania. |

---

# 6. Statusy referencyjne

## 6.1. Statusy leada

| Status | Znaczenie |
|---|---|
| Nowy | Lead został utworzony i wymaga pierwszej obsługi. |
| W kontakcie | Handlowiec rozpoczął kontakt z klientem. |
| Zakwalifikowany | Lead nadaje się do prowadzenia jako szansa sprzedaży. |
| Odrzucony | Lead nie będzie dalej obsługiwany. |

## 6.2. Etapy szansy sprzedaży

| Etap | Znaczenie |
|---|---|
| Nowa szansa | Szansa została utworzona po kwalifikacji leada. |
| Kontakt nawiązany | Handlowiec skontaktował się z klientem. |
| Oferta wysłana | Klient otrzymał ofertę. |
| Negocjacje | Trwają ustalenia warunków. |
| Wygrana | Sprzedaż zakończona sukcesem. |
| Przegrana | Sprzedaż zakończona niepowodzeniem. |

## 6.3. Statusy zamówienia

| Status | Znaczenie |
|---|---|
| Robocze | Zamówienie jest przygotowywane przez handlowca. |
| Przekazane do backoffice | Zamówienie zostało wysłane do obsługi. |
| Nowe w backoffice | Zamówienie czeka w kolejce. |
| W weryfikacji | Backoffice sprawdza dane zamówienia. |
| W realizacji | Zamówienie jest realizowane. |
| Oczekuje na informacje | Potrzebne są dodatkowe informacje. |
| Zablokowane | Realizacja nie może być kontynuowana. |
| Zwrócone do handlowca | Handlowiec musi uzupełnić dane. |
| Zrealizowane | Zamówienie zostało zakończone. |
| Anulowane | Zamówienie nie będzie realizowane. |

---

# 7. Agregaty domenowe

| Agregat | Kontekst | Odpowiedzialność |
|---|---|---|
| Customer | Customer Management | Pilnuje danych klienta i osób kontaktowych. |
| Lead | Sales Pipeline | Pilnuje życia leada od utworzenia do kwalifikacji lub odrzucenia. |
| Opportunity | Sales Pipeline | Pilnuje etapów sprzedaży, wartości, wygranej i przegranej. |
| SalesActivity | Sales Activities | Reprezentuje kontakt, notatkę lub zadanie. |
| SalesOrder | Order Capture | Pilnuje kompletności zamówienia przed przekazaniem do backoffice. |
| BackofficeOrderCase | Order Backoffice | Pilnuje obsługi zamówienia, statusów, przypisania i zamknięcia. |
| User | Identity & Access | Reprezentuje użytkownika systemu. |
| Role | Identity & Access | Reprezentuje poziom uprawnień użytkownika. |

---

# 8. Zdarzenia domenowe

| Zdarzenie | Znaczenie |
|---|---|
| CustomerCreated | Utworzono klienta. |
| ContactPersonAdded | Dodano osobę kontaktową. |
| LeadCreated | Utworzono lead. |
| LeadAssigned | Przypisano lead do handlowca. |
| LeadQualified | Lead został zakwalifikowany. |
| LeadRejected | Lead został odrzucony. |
| OpportunityCreated | Utworzono szansę sprzedaży. |
| OpportunityStageChanged | Zmieniono etap sprzedaży. |
| OpportunityWon | Szansa została wygrana. |
| OpportunityLost | Szansa została przegrana. |
| SalesActivityLogged | Zarejestrowano aktywność sprzedażową. |
| SalesNoteAdded | Dodano notatkę. |
| FollowUpScheduled | Zaplanowano kolejny kontakt. |
| SalesOrderCreated | Utworzono zamówienie. |
| OrderSubmittedToBackoffice | Zamówienie przekazano do backoffice. |
| BackofficeOrderAssigned | Zamówienie przypisano do pracownika backoffice. |
| BackofficeOrderStatusChanged | Zmieniono status obsługi zamówienia. |
| OrderReturnedToSales | Zamówienie zwrócono do handlowca. |
| OrderBlocked | Zamówienie zostało zablokowane. |
| OrderCompleted | Zamówienie zostało zrealizowane. |
| OrderCancelled | Zamówienie zostało anulowane. |
