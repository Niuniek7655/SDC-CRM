# 2. Scenariusze testowe — aplikacja web

Scenariusze ręczne dla aplikacji Angular (`Frontend/Web`). Zakres odpowiada temu, co jest zaimplementowane:
logowanie przez SSO, uprawnienia według ról, lista „Moje leady” i rejestracja leada (CRM-001, CRM-002, CRM-028, CRM-029).

Środowisko: [01-uruchomienie-srodowiska.md](01-uruchomienie-srodowiska.md) — kroki 1–5.
Narzędzie pomocnicze: DevTools przeglądarki (`F12`), karty **Network**, **Application** i **Console**.

## Zasady wykonywania

- Każdy scenariusz zaczynaj od stanu opisanego w „Warunkach wstępnych”.
- Do pracy jako inny użytkownik używaj **oddzielnego profilu lub innej przeglądarki** (ew. okna zwykłego i jednego incognito) albo najpierw się wyloguj — sesja SSO
  (ciasteczko na `localhost:5001`) jest wspólna dla kart i okien tego samego profilu, także między otwartymi oknami incognito.
  Potwierdź nazwę i różne `sub` tokenów; nie zapisuj tokenów w raporcie.
- Wynik zapisuj w tabeli podsumowania: ✅ zgodny, ❌ niezgodny (z opisem), ⏭ pominięty.
- Dane testowe nazywaj z prefiksem, np. `[TEST-WEB] Acme`, żeby łatwo je odróżnić w bazie.

## Podsumowanie

| ID | Scenariusz | Użytkownik | Wynik |
|---|---|---|---|
| WEB-01 | Wejście bez sesji przekierowuje do SSO | — | |
| WEB-02 | Logowanie handlowca i pusta lista | `handlowiec` | |
| WEB-03 | Rejestracja leada ze wszystkimi polami | `handlowiec` | |
| WEB-04 | Rejestracja leada tylko z polami wymaganymi | `handlowiec` | |
| WEB-05 | Walidacja formularza po stronie przeglądarki | `handlowiec` | |
| WEB-06 | Reguła domenowa z backendu (nazwa firmy ze spacji) | `handlowiec` | |
| WEB-07 | Reguła domenowa z backendu (osoba kontaktowa ze spacji) | `handlowiec` | |
| WEB-08 | Anulowanie rejestracji | `handlowiec` | |
| WEB-09 | Kolejność listy — najnowszy lead na górze | `handlowiec` | |
| WEB-10 | Izolacja danych między użytkownikami | `kierownik.sprzedazy` | |
| WEB-11 | Administrator ma dostęp do leadów | `admin` | |
| WEB-12 | Rola backoffice — brak dostępu do leadów | `backoffice` | |
| WEB-13 | Rola backoffice — bezpośrednie wejście na adres formularza | `kierownik.backoffice` | |
| WEB-14 | Przechowywanie tokenów i odświeżenie strony | `handlowiec` | |
| WEB-15 | Wylogowanie | `handlowiec` | |
| WEB-16 | Backend niedostępny i ponowienie | `handlowiec` | |
| WEB-17 | Token tylko w żądaniach do API, identyfikator korelacji | `handlowiec` | |
| WEB-18 | Zbyt długi telefon lub źródło (znana luka) | `handlowiec` | |

## Scenariusze

### WEB-01 — Wejście bez sesji przekierowuje do SSO

- **Warunki wstępne:** nowe okno incognito, brak sesji SSO.
- **Kroki:**
  1. Otwórz http://localhost:4200.
- **Oczekiwany wynik:**
  - przeglądarka przechodzi na stronę logowania SSO pod adresem zaczynającym się od `http://localhost:5001/master`,
  - w adresie są parametry `client_id=sdc-crm-web`, `response_type=code` i `code_challenge` (PKCE).

### WEB-02 — Logowanie handlowca i pusta lista

- **Warunki wstępne:** WEB-01; użytkownik `handlowiec` nie ma jeszcze leadów (lub zignoruj krok 3 oczekiwań).
- **Kroki:**
  1. Zaloguj się jako `handlowiec` / `Test123!`.
- **Oczekiwany wynik:**
  1. powrót do http://localhost:4200, przekierowanie na `/leads`, tytuł karty „Moje leady”,
  2. w nagłówku: „SDC CRM”, link „Leady”, nazwa użytkownika i przycisk „Wyloguj”,
  3. pusta lista: „Nie masz jeszcze żadnych leadów.” i przycisk „Zarejestruj pierwszego leada”,
  4. w karcie Network żądanie `GET /api/leads/mine` ze statusem `200`.

### WEB-03 — Rejestracja leada ze wszystkimi polami

- **Warunki wstępne:** zalogowany `handlowiec`, strona „Moje leady”.
- **Kroki:**
  1. Kliknij „+ Nowy lead” (adres `/leads/new`, nagłówek „Nowy lead”).
  2. Wpisz: Nazwa firmy `[TEST-WEB] Acme Sp. z o.o.`, Osoba kontaktowa `Jan Kowalski`,
     E-mail `jan.kowalski@acme.test`, Telefon `+48 600 100 200`, Źródło `Targi`.
  3. Kliknij „Zapisz leada”.
- **Oczekiwany wynik:**
  1. przycisk na chwilę zmienia się na „Zapisywanie…” i jest nieaktywny (brak podwójnego wysłania),
  2. `POST /api/leads` → `201`, odpowiedź `{"id":"<guid>"}`,
  3. powrót na listę; nowy wiersz: Firma, Osoba kontaktowa, E-mail, Status **„Nowy”**, data utworzenia,
  4. (opcjonalnie) w bazie rekord z `Status = New`, telefonem i źródłem —
     zapytanie w [04-testy-endpointow-api.md](04-testy-endpointow-api.md#weryfikacja-w-bazie-danych).

### WEB-04 — Rejestracja leada tylko z polami wymaganymi

- **Kroki:** zarejestruj leada z Nazwą firmy, Osobą kontaktową i E-mailem; Telefon i Źródło zostaw puste.
- **Oczekiwany wynik:** `201`, lead na liście; w bazie `ContactPhone` i `Source` mają wartość `NULL`
  (puste pola nie są zapisywane jako pusty tekst).

### WEB-05 — Walidacja formularza po stronie przeglądarki

- **Kroki:**
  1. Na `/leads/new` kliknij „Zapisz leada” bez wypełniania pól.
  2. Wpisz E-mail `jan.kowalski` (bez `@`) i opuść pole.
- **Oczekiwany wynik:**
  1. komunikaty: „Nazwa firmy jest wymagana.”, „Osoba kontaktowa jest wymagana.”, „Podaj poprawny adres e-mail.”,
  2. w karcie Network **brak** żądania `POST /api/leads`,
  3. pola oznaczone `*` to Nazwa firmy, Osoba kontaktowa, E-mail.

### WEB-06 — Reguła domenowa z backendu (nazwa firmy ze spacji)

Formularz uznaje tekst złożony z samych spacji za wypełniony, więc żądanie trafia do backendu, który egzekwuje regułę domenową.

- **Kroki:**
  1. Nazwa firmy: trzy spacje; Osoba kontaktowa `Jan Kowalski`; E-mail `jan@acme.test`.
  2. Kliknij „Zapisz leada”.
- **Oczekiwany wynik:**
  1. `POST /api/leads` → `400`, treść `ProblemDetails` z `"detail": "A lead must have a company name."`,
  2. komunikat **„A lead must have a company name.”** nad formularzem (komunikaty domeny są po angielsku),
  3. formularz zachowuje wpisane dane, przycisk wraca do „Zapisz leada”,
  4. lead nie pojawia się na liście.

### WEB-07 — Reguła domenowa z backendu (osoba kontaktowa ze spacji)

- **Kroki:** jak WEB-06, ale Nazwa firmy `[TEST-WEB] Beta`, Osoba kontaktowa: trzy spacje.
- **Oczekiwany wynik:** `400`, komunikat „A lead must have a contact name.”, dane w formularzu zachowane.

### WEB-08 — Anulowanie rejestracji

- **Kroki:** na `/leads/new` wpisz dowolne dane i kliknij „Anuluj”.
- **Oczekiwany wynik:** powrót na `/leads`, brak żądania `POST /api/leads`, lista bez zmian.

### WEB-09 — Kolejność listy

- **Kroki:** zarejestruj kolejno leady `[TEST-WEB] Pierwszy` i `[TEST-WEB] Drugi`.
- **Oczekiwany wynik:** `[TEST-WEB] Drugi` jest pierwszym wierszem — lista jest sortowana od najnowszego.

### WEB-10 — Izolacja danych między użytkownikami

- **Warunki wstępne:** `handlowiec` ma co najmniej jednego leada (WEB-03).
- **Kroki:**
  1. W oddzielnym profilu lub innej przeglądarce zaloguj się jako `kierownik.sprzedazy`.
  2. Zarejestruj leada `[TEST-WEB] Leada kierownika`.
  3. Wróć do okna `handlowiec` i odśwież listę (`F5`).
- **Oczekiwany wynik:**
  1. `kierownik.sprzedazy` widzi tylko swojego leada — nie widzi leadów handlowca
     (widok zespołu dla kierownika nie jest zaimplementowany, Q-13),
  2. `handlowiec` nie widzi leada kierownika,
  3. właściciel leada wynika z tokena, a nie z danych formularza.

### WEB-11 — Administrator ma dostęp do leadów

- **Kroki:** zaloguj się jako `admin` / `Test123!`.
- **Oczekiwany wynik:** widoczny link „Leady” i strona „Moje leady”; rejestracja leada działa (`201`).

### WEB-12 — Rola backoffice — brak dostępu do leadów

- **Kroki:** w oddzielnym profilu lub innej przeglądarce zaloguj się jako `backoffice` / `Test123!`.
- **Oczekiwany wynik:**
  1. po zalogowaniu przekierowanie na `/forbidden`: „Brak dostępu” i „Twoje konto nie ma uprawnień do tej sekcji.
     Skontaktuj się z administratorem.”,
  2. w nagłówku **brak** linku „Leady”, jest nazwa użytkownika i „Wyloguj”,
  3. w karcie Network brak żądań do `/api/leads` (blokuje je strażnik trasy).

### WEB-13 — Rola backoffice — bezpośrednie wejście na adres formularza

- **Kroki:** jako `kierownik.backoffice` wpisz w pasku adresu http://localhost:4200/leads/new.
- **Oczekiwany wynik:** przekierowanie na `/forbidden`. Backend również odrzuca takie żądanie (`403`) —
  sprawdzenie w [04-testy-endpointow-api.md](04-testy-endpointow-api.md) (API-40, API-41).

### WEB-14 — Przechowywanie tokenów i odświeżenie strony

- **Warunki wstępne:** zalogowany `handlowiec`.
- **Kroki:**
  1. DevTools → Application → Session Storage → `http://localhost:4200`.
  2. DevTools → Application → Local Storage → `http://localhost:4200`.
  3. Odśwież stronę (`F5`).
  4. Otwórz http://localhost:4200 w **nowej karcie** tego samego okna.
- **Oczekiwany wynik:**
  1. Session Storage zawiera m.in. `access_token`, `refresh_token`, `id_token`,
  2. Local Storage **nie** zawiera tokenów,
  3. po `F5` użytkownik pozostaje zalogowany,
  4. nowa karta przechodzi przez SSO, zwykle bez pytania o hasło (sesja SSO), i kończy na „Moje leady”.

### WEB-15 — Wylogowanie

- **Kroki:**
  1. Kliknij „Wyloguj”.
  2. Jeśli SSO wyświetli stronę potwierdzenia, potwierdź zakończenie sesji („Revoke session”).
  3. Ponownie otwórz http://localhost:4200.
- **Oczekiwany wynik:**
  1. powrót na http://localhost:4200, a następnie na stronę logowania SSO,
  2. Session Storage bez tokenów,
  3. ponowne wejście wymaga podania loginu i hasła.

### WEB-16 — Backend niedostępny i ponowienie

- **Kroki:**
  1. Zalogowany `handlowiec`; zatrzymaj API (`Ctrl+C`).
  2. Odśwież „Moje leady”.
  3. Uruchom API ponownie i kliknij „Spróbuj ponownie”.
- **Oczekiwany wynik:**
  1. komunikat „Nie udało się pobrać leadów. Sprawdź, czy backend jest uruchomiony.” i przycisk „Spróbuj ponownie”,
  2. użytkownik nie zostaje wylogowany,
  3. po ponowieniu lista się wczytuje.
- **Dodatkowo:** zatrzymaj API na formularzu „Nowy lead” i zapisz poprawne dane → komunikat
  „Nie udało się zapisać leada.”, dane w formularzu zachowane.

### WEB-17 — Token tylko w żądaniach do API, identyfikator korelacji

- **Kroki:** zalogowany `handlowiec`; karta Network; odśwież „Moje leady”.
- **Oczekiwany wynik:**
  1. `GET /api/leads/mine` ma nagłówek `Authorization: Bearer …`,
  2. żądania do `localhost:5001` (np. discovery, `token`) **nie** mają nagłówka `Authorization: Bearer`,
  3. odpowiedź API ma nagłówek `X-Correlation-ID` — wpisz go w Seq (`CorrelationId = '<wartość>'`)
     albo w Jaegerze (usługa `sdc-crm-api`, tag `correlation.id=<wartość>`), żeby znaleźć logi i ślad żądania.

### WEB-18 — Zbyt długi telefon lub źródło (znana luka)

Kolumny w bazie mają limity (Telefon 50, Źródło 100, Nazwa firmy i Osoba kontaktowa 200, E-mail 320 znaków).
Formularz ogranicza tylko Nazwę firmy i Osobę kontaktową, a domena nie sprawdza długości.

- **Kroki:** zarejestruj leada z Telefonem dłuższym niż 50 znaków (np. 60 cyfr).
- **Oczekiwany wynik biznesowy:** czytelny błąd walidacji (`400`) i zachowane dane formularza.
- **Prawdopodobny obecny wynik:** `500` z bazy danych i ogólny komunikat „Nie udało się zapisać leada.” —
  jeśli się potwierdzi, zgłoś jako błąd (brak walidacji długości w domenie / API).

## Testy automatyczne aplikacji web

Scenariusze ręczne uzupełniają testy jednostkowe (Karma + Jasmine, opis w
[Frontend/Web/README.md](../../Frontend/Web/README.md)):

```powershell
cd Frontend/Web
$env:CI = 'true'      # bez tego pierwsze uruchomienie Angular CLI może czekać na odpowiedź na pytanie
npm run test:ci
```

## Uzupełnienia regresyjne (WEB-19…WEB-25)

Dla każdego przypadku zapisz PASS/FAIL/SKIP, przeglądarkę, użytkownika, kod HTTP i identyfikator korelacji; bez tokenów i danych kontaktowych.

| ID | Kroki | Oczekiwany wynik / stan obecny |
|---|---|---|
| WEB-19 | Czysta sesja → bezpośrednio `/leads/new` → logowanie handlowca. Powtórz z query string. | Powrót do żądanego formularza. Znana luka: callback kieruje do listy; odnotuj FAIL, nie uznawaj WEB-01 za pokrycie tego przypadku. |
| WEB-20 | Otwarta lista i formularz → wygaśnięcie access tokena → kolejny GET/POST; następnie powtórz z unieważnionym refresh tokenem oraz wyłączonym SSO (API pozostaje włączone). | Udany refresh używa nowego tokena; nieudany ma czytelny stan i możliwość ponownego logowania. Sprawdź, czy formularz nie gubi wpisanych danych. F5 ze świeżym tokenem nie zastępuje tego testu. |
| WEB-21 | DevTools: opóźnij sieć. Zapisz formularz i przed odpowiedzią kliknij Anuluj, zmień trasę lub wyloguj się. Osobno ponów po utracie odpowiedzi mimo zapisu. | Brak nawigacji ze starego ekranu/sesji. Policz faktyczne POST oraz rekordy w bazie. Wynik niepewnego zapisu i duplikatów wymaga decyzji o idempotencji; nie zakładaj jej istnienia. |
| WEB-22 | Po wejściu na formularz odbierz rolę w SSO i uzyskaj świeży token albo doprowadź do 401. Wykonaj GET i POST. | Sprawdź komunikat, zachowanie formularza i odzyskanie sesji. Zakaz wejścia przez guard nie pokrywa odmowy API w otwartym ekranie. |
| WEB-23 | Każde pole: N−1/N/N+1 znaków (200/200/320/50/100), spacje wokół wartości, Unicode; po 400 popraw dane i ponów. | Poprawne dane są zachowane, błędne nie zapisane; UI pokazuje adekwatny komunikat. Znana luka: API nie waliduje maksymalnych długości, nazwa 201 znaków może pokazać błędny komunikat „wymagana”. |
| WEB-24 | Przerwij logowanie, odmów dostępu; powtórz callback z błędnym state/nonce w lokalnym środowisku. | Brak nowej uwierzytelnionej sesji z niepoprawnego callbacku, możliwość ponownego logowania, brak tokenów w logach. Nie publikuj URL callbacku zawierającego kod. |
| WEB-25 | Powtórz logowanie/refresh/logout w dwóch kartach tego samego konta oraz dwóch niezależnych profilach różnych kont. | Dane są zgodne z tokenem konkretnej karty. Zakończenie sesji SSO nie jest tym samym co usunięcie tokenów z każdej karty; zapisz zaobserwowane zachowanie i decyzję o globalnym logout. |

Automaty i jawnie uruchamiane znane regresje: [05-pokrycie-i-regresje.md](05-pokrycie-i-regresje.md).
