# Użytkownicy, role i uprawnienia systemu CRM

## Cel dokumentu

Ten dokument opisuje typy użytkowników systemu CRM, ich odpowiedzialności oraz podstawowe uprawnienia.

---

# 1. Typy użytkowników

## 1.1. Handlowiec

### Opis

Handlowiec jest podstawowym użytkownikiem systemu.  
Odpowiada za rejestrowanie leadów, kontakt z klientami, prowadzenie pipeline, dodawanie notatek i wprowadzanie zamówień po finalizacji sprzedaży.

### Główne cele

- Rejestrować nowe leady.
- Obsługiwać przypisane leady.
- Prowadzić szanse sprzedaży.
- Dodawać kontakty, notatki i follow-upy.
- Tworzyć zamówienia.
- Sprawdzać status zamówień przekazanych do backoffice.

### Typowe akcje

- Dodanie leada.
- Edycja własnego leada.
- Dodanie notatki.
- Zaplanowanie follow-upu.
- Zmiana etapu szansy sprzedaży.
- Oznaczenie szansy jako wygranej lub przegranej.
- Utworzenie zamówienia.
- Przekazanie zamówienia do backoffice.
- Uzupełnienie zamówienia zwróconego przez backoffice.

---

## 1.2. Menedżer sprzedaży

### Opis

Menedżer sprzedaży zarządza pracą handlowców i odpowiada za kontrolę pipeline oraz wyników sprzedaży.

### Główne cele

- Monitorować pipeline zespołu.
- Przypisywać leady do handlowców.
- Analizować aktywność handlowców.
- Kontrolować sprzedaż wygraną i przegraną.
- Reagować na zaległe follow-upy i nieobsłużone leady.

### Typowe akcje

- Podgląd leadów zespołu.
- Przypisywanie leadów.
- Podgląd pipeline.
- Podgląd raportów sprzedażowych.
- Podgląd aktywności handlowców.
- Eksport raportów.

---

## 1.3. Pracownik backoffice

### Opis

Pracownik backoffice obsługuje zamówienia przekazane przez sprzedaż.

### Główne cele

- Odbierać zamówienia z kolejki.
- Weryfikować kompletność i poprawność zamówień.
- Zmieniać statusy zamówień.
- Dodawać komentarze.
- Zwracać zamówienia do handlowców, jeżeli wymagają poprawek.
- Zamykać zrealizowane zamówienia.

### Typowe akcje

- Podgląd kolejki zamówień.
- Przypisanie zamówienia do siebie.
- Zmiana statusu zamówienia.
- Dodanie komentarza.
- Zablokowanie zamówienia.
- Zwrot zamówienia do handlowca.
- Oznaczenie zamówienia jako zrealizowane.

---

## 1.4. Menedżer backoffice

### Opis

Menedżer backoffice zarządza pracą zespołu backoffice i kontroluje realizację zamówień.

### Główne cele

- Monitorować obciążenie zespołu.
- Przypisywać zamówienia pracownikom.
- Kontrolować opóźnienia i blokady.
- Analizować czas realizacji zamówień.
- Zarządzać priorytetami obsługi.

### Typowe akcje

- Podgląd wszystkich zamówień backoffice.
- Przypisanie zamówienia do pracownika.
- Zmiana osoby obsługującej.
- Podgląd raportu backoffice.
- Analiza zamówień zablokowanych.
- Eksport raportów.

---

## 1.5. Administrator

### Opis

Administrator zarządza kontami, rolami i konfiguracją systemu.

### Główne cele

- Zarządzać użytkownikami.
- Przypisywać role.
- Konfigurować podstawowe słowniki.
- Kontrolować dostęp do systemu.
- Zarządzać ustawieniami systemowymi.

### Typowe akcje

- Dodanie użytkownika.
- Dezaktywacja użytkownika.
- Przypisanie roli.
- Konfiguracja statusów i słowników.
- Podgląd audytu.
- Zarządzanie integracjami.

---

# 2. Role systemowe

| Rola | Opis | Główny obszar pracy |
|---|---|---|
| Salesperson | Handlowiec obsługujący leady i klientów. | Sprzedaż |
| SalesManager | Menedżer zarządzający sprzedażą. | Pipeline i raporty |
| BackofficeUser | Pracownik realizujący zamówienia. | Obsługa zamówień |
| BackofficeManager | Menedżer backoffice. | Zarządzanie kolejką i obciążeniem |
| Admin | Administrator systemu. | Konfiguracja i użytkownicy |

---

# 3. Macierz uprawnień MVP

| Funkcja | Handlowiec | Menedżer sprzedaży | Backoffice | Menedżer backoffice | Admin |
|---|---:|---:|---:|---:|---:|
| Logowanie do systemu | ✅ | ✅ | ✅ | ✅ | ✅ |
| Dodanie leada | ✅ | ✅ | ❌ | ❌ | ✅ |
| Edycja własnego leada | ✅ | ✅ | ❌ | ❌ | ✅ |
| Edycja leada innego handlowca | ❌ | ✅ | ❌ | ❌ | ✅ |
| Przypisanie leada | ❌ | ✅ | ❌ | ❌ | ✅ |
| Podgląd pipeline własnego | ✅ | ✅ | ❌ | ❌ | ✅ |
| Podgląd pipeline zespołu | ❌ | ✅ | ❌ | ❌ | ✅ |
| Dodanie notatki sprzedażowej | ✅ | ✅ | ❌ | ❌ | ✅ |
| Zaplanowanie follow-upu | ✅ | ✅ | ❌ | ❌ | ✅ |
| Utworzenie zamówienia | ✅ | ✅ | ❌ | ❌ | ✅ |
| Przekazanie zamówienia do backoffice | ✅ | ✅ | ❌ | ❌ | ✅ |
| Podgląd statusu własnego zamówienia | ✅ | ✅ | ✅ | ✅ | ✅ |
| Podgląd kolejki backoffice | ❌ | ❌ | ✅ | ✅ | ✅ |
| Zmiana statusu zamówienia | ❌ | ❌ | ✅ | ✅ | ✅ |
| Zwrot zamówienia do handlowca | ❌ | ❌ | ✅ | ✅ | ✅ |
| Przypisanie zamówienia do pracownika | ❌ | ❌ | ❌ | ✅ | ✅ |
| Zamknięcie zamówienia | ❌ | ❌ | ✅ | ✅ | ✅ |
| Podgląd raportów sprzedażowych | Ograniczony | ✅ | ❌ | ❌ | ✅ |
| Podgląd raportów backoffice | ❌ | ❌ | Ograniczony | ✅ | ✅ |
| Zarządzanie użytkownikami | ❌ | ❌ | ❌ | ❌ | ✅ |
| Konfiguracja systemu | ❌ | ❌ | ❌ | ❌ | ✅ |
| Podgląd audytu | ❌ | Ograniczony | Ograniczony | Ograniczony | ✅ |

---

# 4. Widoczność danych

## 4.1. Handlowiec

Domyślna widoczność:

- własne leady,
- własne szanse sprzedaży,
- własne zadania,
- klienci przypisani do handlowca,
- zamówienia utworzone przez handlowca.

Otwarte pytanie biznesowe:

- Czy handlowiec może widzieć klientów innych handlowców?
- Czy handlowiec może sprawdzić, że klient już istnieje, ale bez dostępu do pełnych danych?
- Jak obsłużyć zastępstwa i urlopy?

## 4.2. Menedżer sprzedaży

Domyślna widoczność:

- leady zespołu,
- szanse zespołu,
- klienci przypisani do zespołu,
- aktywności handlowców,
- raporty sprzedażowe.

Otwarte pytanie biznesowe:

- Czy menedżer widzi tylko swój zespół, czy wszystkich handlowców?
- Czy istnieje hierarchia zespołów?

## 4.3. Backoffice

Domyślna widoczność:

- zamówienia przekazane do backoffice,
- dane wymagane do realizacji zamówienia,
- komentarze backoffice,
- statusy i historia obsługi.

Ograniczenie:

- backoffice nie musi mieć pełnego dostępu do całego pipeline sprzedażowego.

## 4.4. Administrator

Domyślna widoczność:

- użytkownicy,
- role,
- konfiguracja,
- słowniki,
- audyt,
- techniczne i administracyjne ustawienia systemu.

---

# 5. Persony biznesowe

## Persona: Handlowiec terenowy

- Pracuje szybko.
- Często dodaje notatki po rozmowie.
- Potrzebuje listy zadań i prostego formularza leada.
- Nie chce wypełniać zbyt wielu pól na początku.
- Najważniejsze dla niego: szybka rejestracja kontaktu i follow-up.

## Persona: Menedżer sprzedaży

- Potrzebuje kontroli pipeline.
- Chce wiedzieć, które leady są zaniedbane.
- Analizuje aktywność i skuteczność handlowców.
- Potrzebuje raportów i filtrów.

## Persona: Pracownik backoffice

- Pracuje na kolejce zamówień.
- Potrzebuje czytelnego statusu i danych do realizacji.
- Chce szybko zwrócić zamówienie, jeżeli dane są niekompletne.
- Potrzebuje historii komentarzy i zmian.

## Persona: Administrator

- Zarządza dostępem.
- Konfiguruje podstawowe dane.
- Pomaga rozwiązywać problemy użytkowników.
- Potrzebuje audytu i kontroli nad systemem.
