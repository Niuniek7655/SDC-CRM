# 3. Scenariusze testowe — aplikacja mobilna (Android i Windows)

Scenariusze ręczne dla aplikacji .NET MAUI (`Frontend/Mobile`). Zakres odpowiada temu, co jest zaimplementowane:
logowanie przez SSO w przeglądarce systemowej, ekran główny z listą „moich leadów”, obsługa braku uprawnień,
braku sieci i wylogowanie (CRM-002, CRM-028, CRM-029).

**Rejestracji leada w aplikacji mobilnej jeszcze nie ma** (ekran do decyzji w Q-16). Leady do testów listy
tworzysz w aplikacji web ([02-scenariusze-web.md](02-scenariusze-web.md)) albo przez API
([04-testy-endpointow-api.md](04-testy-endpointow-api.md)).

Środowisko: [01-uruchomienie-srodowiska.md](01-uruchomienie-srodowiska.md) — kroki 1–4 oraz 6a (Android) lub 6b (Windows).

## Platformy

| | Android (emulator lub telefon) | Windows |
|---|---|---|
| Dostęp do SSO i API | przez `adb reverse tcp:5001` i `tcp:5080` | bezpośrednio `localhost` |
| Logowanie | przeglądarka systemowa urządzenia, powrót przez `com.sdc.crm.mobile://callback` | **niezweryfikowane** — patrz uwaga w kroku 6b dokumentu uruchomieniowego |
| Wyłączenie sieci | tryb samolotowy albo `adb shell svc wifi disable` i `adb shell svc data disable` | wyłączenie Wi-Fi / karty sieciowej |
| Tokeny | `SecureStorage` (Android Keystore) | `SecureStorage` (Windows) |

Scenariusze wykonaj na Androidzie. Na Windows wykonaj co najmniej MOB-01, MOB-02 i MOB-09; jeśli logowanie
na Windows działa, wykonaj pozostałe tak samo jak na Androidzie.

## Zasady wykonywania

- Wynik zapisuj w tabeli podsumowania: ✅ zgodny, ❌ niezgodny (z opisem i zrzutem ekranu), ⏭ pominięty.
- Zmiana użytkownika wymaga wylogowania w aplikacji (MOB-12) — wtedy kończy się także sesja SSO.
- Logi aplikacji (Debug): okno Debug w Riderze albo `adb logcat` na Androidzie.

## Podsumowanie

| ID | Scenariusz | Użytkownik | Android | Windows |
|---|---|---|---|---|
| MOB-01 | Ekran logowania przy pierwszym uruchomieniu | — | | |
| MOB-02 | Logowanie handlowca i ekran główny | `handlowiec` | | |
| MOB-03 | Lista pokazuje leady utworzone w web lub API | `handlowiec` | | |
| MOB-04 | Odświeżanie listy gestem przeciągnięcia | `handlowiec` | | |
| MOB-05 | Przerwanie logowania | — | | |
| MOB-06 | Błędne hasło w SSO | — | | |
| MOB-07 | Zapamiętana sesja po ponownym uruchomieniu | `handlowiec` | | |
| MOB-08 | Rola bez dostępu do leadów — użytkownik zostaje zalogowany | `backoffice` | | |
| MOB-09 | Brak sieci | `handlowiec` | | |
| MOB-10 | API niedostępne | `handlowiec` | | |
| MOB-11 | Nieważna sesja (401) — powrót do logowania bez przeglądarki | `handlowiec` | | |
| MOB-12 | Wylogowanie z zakończeniem sesji SSO | `handlowiec` | | |
| MOB-13 | Wylogowanie, gdy SSO jest niedostępne | `handlowiec` | | |
| MOB-14 | Izolacja danych po zmianie użytkownika | `kierownik.sprzedazy` | | |
| MOB-15 | Wznowienie aplikacji i obrót ekranu | `handlowiec` | | |

## Scenariusze

### MOB-01 — Ekran logowania przy pierwszym uruchomieniu

- **Warunki wstępne:** aplikacja świeżo zainstalowana albo po wylogowaniu.
- **Kroki:** uruchom aplikację „SDC CRM”.
- **Oczekiwany wynik:** ekran „SDC CRM” z tekstem „Zaloguj się przez SSO, aby kontynuować.” i przyciskiem
  „Zaloguj się”; brak paska nawigacji i komunikatów błędu.

### MOB-02 — Logowanie handlowca i ekran główny

- **Warunki wstępne:** MOB-01.
- **Kroki:**
  1. Stuknij „Zaloguj się”.
  2. W przeglądarce systemowej zaloguj się jako `handlowiec` / `Test123!`.
- **Oczekiwany wynik:**
  1. podczas logowania widoczny wskaźnik postępu, a przycisk jest nieaktywny,
  2. przeglądarka otwiera stronę logowania SSO (`http://localhost:5001/master/...`),
  3. po zalogowaniu przeglądarka zamyka się i aplikacja pokazuje ekran główny „SDC CRM”,
  4. powitanie „Witaj, &lt;nazwa użytkownika&gt;!”, „Status połączenia: **Online**” (zielony),
  5. lista leadów użytkownika albo tekst „Brak leadów do wyświetlenia.”,
  6. w pasku narzędzi pozycja „Wyloguj”.

### MOB-03 — Lista pokazuje leady utworzone w web lub API

- **Warunki wstępne:** `handlowiec` ma leady zarejestrowane w web (WEB-03) albo przez API (API-11).
- **Kroki:** zaloguj się w aplikacji mobilnej jako `handlowiec`.
- **Oczekiwany wynik:**
  1. te same leady co na liście w web, w tej samej kolejności (najnowszy na górze),
  2. każda pozycja: nazwa firmy (pogrubiona), osoba kontaktowa, e-mail, status **„Nowy”** (polska etykieta, nie `New`).

### MOB-04 — Odświeżanie listy gestem przeciągnięcia

- **Kroki:**
  1. Ekran główny `handlowiec` otwarty.
  2. W web zarejestruj nowego leada `[TEST-MOB] Odświeżanie` jako `handlowiec`.
  3. W aplikacji przeciągnij listę w dół.
- **Oczekiwany wynik:** widoczny wskaźnik odświeżania; nowy lead pojawia się na górze listy; brak duplikatów.

### MOB-05 — Przerwanie logowania

- **Kroki:** stuknij „Zaloguj się”, a w przeglądarce systemowej zamknij ją lub cofnij (przycisk „Wstecz”) bez logowania.
- **Oczekiwany wynik:** aplikacja wraca na ekran logowania z czerwonym komunikatem zaczynającym się od
  „Logowanie nie powiodło się”; przycisk „Zaloguj się” znów jest aktywny; aplikacja się nie zawiesza.

### MOB-06 — Błędne hasło w SSO

- **Kroki:** stuknij „Zaloguj się” i podaj login `handlowiec` z błędnym hasłem.
- **Oczekiwany wynik:** błąd wyświetla strona SSO w przeglądarce; po poprawieniu hasła logowanie kończy się jak w MOB-02.

### MOB-07 — Zapamiętana sesja po ponownym uruchomieniu

- **Warunki wstępne:** zalogowany `handlowiec` (bez wylogowania).
- **Kroki:** zamknij aplikację całkowicie (usuń z listy ostatnich aplikacji / zamknij okno) i uruchom ponownie.
- **Oczekiwany wynik:** aplikacja przechodzi od razu do ekranu głównego **bez** otwierania przeglądarki;
  lista leadów się wczytuje. Jeżeli token wygasał, aplikacja odświeża go w tle tokenem odświeżania.

### MOB-08 — Rola bez dostępu do leadów — użytkownik zostaje zalogowany

- **Warunki wstępne:** wylogowany (MOB-12).
- **Kroki:**
  1. Zaloguj się jako `backoffice` / `Test123!`.
  2. Przeciągnij listę w dół (odświeżenie).
- **Oczekiwany wynik:**
  1. ekran główny z powitaniem,
  2. czerwony komunikat „Brak uprawnień do listy leadów dla Twojej roli.”, lista pusta,
  3. po odświeżeniu ten sam komunikat — aplikacja **nie** wylogowuje użytkownika i nie wraca do ekranu logowania
     (API zwraca 403, nie 401).
- **Powtórz** dla `kierownik.backoffice`.

### MOB-09 — Brak sieci

- **Warunki wstępne:** zalogowany `handlowiec`, lista wczytana.
- **Kroki:**
  1. Wyłącz sieć (Android: tryb samolotowy; Windows: wyłącz Wi-Fi / kartę sieciową).
  2. Przeciągnij listę w dół.
  3. Włącz sieć i ponownie przeciągnij listę.
- **Oczekiwany wynik:**
  1. po wyłączeniu sieci „Status połączenia: **Offline**” (czerwony) — zmiana bez odświeżania ekranu,
  2. komunikat „Brak połączenia z siecią. Sprawdź swoje połączenie internetowe.”,
  3. wcześniej wczytana lista **pozostaje widoczna**, użytkownik nie zostaje wylogowany,
  4. po włączeniu sieci status „Online”, odświeżenie usuwa komunikat i wczytuje listę.

### MOB-10 — API niedostępne

- **Kroki:**
  1. Zalogowany `handlowiec`; zatrzymaj API (`Ctrl+C`).
  2. Przeciągnij listę w dół.
  3. Uruchom API i ponownie odśwież.
- **Oczekiwany wynik:**
  1. czerwony komunikat zaczynający się od „Nie udało się pobrać leadów:” z opisem błędu połączenia,
  2. status połączenia nadal „Online” (sieć działa, nie działa serwer), użytkownik pozostaje zalogowany,
  3. po uruchomieniu API lista wczytuje się, a komunikat znika.

### MOB-11 — Nieważna sesja (401) — powrót do logowania bez przeglądarki

Scenariusz opcjonalny: wymaga unieważnienia tokenów przez reset SSO (nowe klucze podpisu, użytkownicy zakładani ponownie).

- **Kroki:**
  1. Zalogowany `handlowiec` w aplikacji.
  2. Zresetuj SSO:
     ```powershell
     cd Integrations/Sso
     docker compose down -v
     ./manage-sso.ps1 start
     ```
  3. W aplikacji przeciągnij listę w dół.
- **Oczekiwany wynik:**
  1. API odrzuca stary token (`401`),
  2. aplikacja czyści lokalną sesję i przechodzi do ekranu logowania **bez** otwierania przeglądarki,
  3. ponowne logowanie działa jak w MOB-02.
- **Uwaga:** reset SSO wylogowuje też aplikację web i unieważnia tokeny używane w testach API.

### MOB-12 — Wylogowanie z zakończeniem sesji SSO

- **Kroki:**
  1. Zalogowany `handlowiec`; stuknij „Wyloguj”.
  2. W przeglądarce SSO potwierdź zakończenie sesji („Revoke session”).
  3. Stuknij „Zaloguj się”.
- **Oczekiwany wynik:**
  1. otwiera się przeglądarka ze stroną zakończenia sesji SSO,
  2. po potwierdzeniu powrót do aplikacji (`com.sdc.crm.mobile://signout`) na ekran logowania, lista wyczyszczona,
  3. kolejne logowanie wymaga podania loginu i hasła (sesja SSO zakończona).

### MOB-13 — Wylogowanie, gdy SSO jest niedostępne

- **Kroki:**
  1. Zalogowany `handlowiec`; zatrzymaj SSO: `docker stop sso-idserver`.
  2. Stuknij „Wyloguj”; jeśli otworzy się przeglądarka z błędem, zamknij ją.
  3. Uruchom SSO: `docker start sso-idserver`; uruchom aplikację ponownie.
- **Oczekiwany wynik:** aplikacja kończy na ekranie logowania mimo błędu SSO (lokalne tokeny są usuwane
  w pierwszej kolejności); po ponownym uruchomieniu aplikacja **nie** przechodzi automatycznie do ekranu głównego.

### MOB-14 — Izolacja danych po zmianie użytkownika

- **Warunki wstępne:** `handlowiec` i `kierownik.sprzedazy` mają różne leady (WEB-10).
- **Kroki:** wyloguj się (MOB-12) i zaloguj jako `kierownik.sprzedazy`.
- **Oczekiwany wynik:** zmienione powitanie; lista zawiera tylko leady kierownika — żadnych leadów handlowca
  z poprzedniej sesji.

### MOB-15 — Wznowienie aplikacji i obrót ekranu

- **Kroki:**
  1. Zalogowany `handlowiec`, lista wczytana.
  2. Przełącz aplikację w tło (przycisk „Home”) na kilka minut i wróć.
  3. Android: obróć ekran.
- **Oczekiwany wynik:** lista i status połączenia widoczne po powrocie; aplikacja nie wraca do logowania i nie
  zamyka się; po obrocie układ się dostosowuje, a dane pozostają.

## Testy automatyczne aplikacji mobilnej

Logika aplikacji (ViewModele, klient API, sesja) jest pokryta testami jednostkowymi w `SDC.CRM.Mobile.Tests`
(opis w [Frontend/Mobile/README.md](../../Frontend/Mobile/README.md)). Nie wymagają emulatora, SSO ani sieci:

```powershell
dotnet test --project Frontend/Mobile/tests/SDC.CRM.Mobile.Tests/SDC.CRM.Mobile.Tests.csproj
```

Kontrola kompilacji aplikacji dla obu platform (jak w CI):

```powershell
dotnet build Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj -f net10.0-android
dotnet build Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj -f net10.0-windows10.0.19041.0
```

