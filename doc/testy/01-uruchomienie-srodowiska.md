# 1. Uruchomienie środowiska: backend, web i mobile

Instrukcja krok po kroku, co i w jakiej kolejności uruchomić, aby przetestować SDC-CRM lokalnie.
Wszystkie polecenia wykonuj w PowerShell z **katalogu głównego repozytorium**, chyba że krok mówi inaczej.

## Jak to działa razem

```text
Przeglądarka / aplikacja mobilna
        │  1. logowanie (Authorization Code + PKCE)
        ▼
SSO - SimpleIdServer  http://localhost:5001/master     (Integrations/Sso, Docker)
        │  2. token dostępowy (JWT z claimem role)
        ▼
Backend API          http://localhost:5080             (Backend/src/SDC.CRM.Api, dotnet run)
        │  3. odczyt / zapis
        ▼
PostgreSQL           localhost:5432 (baza appdb)       (docker-compose.yml w katalogu głównym)
        │  4. ślady, metryki, logi (OTLP)
        ▼
OpenTelemetry Collector -> Seq / Jaeger / Grafana      (docker-compose.yml w katalogu głównym)
```

Wystawcą tokenów jest **`http://localhost:5001/master`** i API akceptuje wyłącznie tokeny z tym wystawcą.
Dlatego każdy klient (przeglądarka, emulator Androida, Windows) musi widzieć SSO i API pod adresem `localhost`
— nie używaj `127.0.0.1` ani adresu IP komputera.

## Wymagania

| Do czego | Narzędzie | Sprawdzenie |
|---|---|---|
| wszystko | Docker Desktop (uruchomiony) | `docker version` |
| backend, mobile | .NET SDK 10 | `dotnet --version` |
| backend | narzędzie `dotnet-ef` (lokalne, z `dotnet-tools.json`) | `dotnet tool restore` |
| web | Node.js 24 (plik `Frontend/Web/.nvmrc`), npm 10+ | `node --version`, `npm --version` |
| web (testy jednostkowe) | Google Chrome | — |
| mobile Android | workload `android` / `maui-android`, Android SDK, emulator lub telefon, Java 17 | `dotnet workload list`, `adb devices` |
| mobile Windows | workload `maui-windows`, Windows 10 1809+ lub Windows 11 | `dotnet workload list` |

## Porty i adresy

| Usługa | Adres | Dane logowania |
|---|---|---|
| SSO (logowanie, metadane OIDC) | http://localhost:5001/master | użytkownicy testowi poniżej |
| Panel administracyjny SSO | http://localhost:5002/master/clients | `administrator` / `password` |
| Backend API | http://localhost:5080 | token Bearer z SSO |
| Dokument OpenAPI (tylko Development) | http://localhost:5080/openapi/v1.json | — |
| Aplikacja web | http://localhost:4200 | użytkownicy testowi poniżej |
| PostgreSQL (CRM) | localhost:5432, baza `appdb` | `app` / `app` |
| Seq (logi i ślady) | http://localhost:5341 | — |
| Jaeger (ślady) | http://localhost:16686 | — |
| Grafana | http://localhost:3000 | `admin` / `admin` |

### Użytkownicy testowi

Tworzy ich skrypt `Integrations/Sso/register-sdc-crm-clients.ps1` (uruchamiany przez `manage-sso.ps1 start`).
Hasło wszystkich: **`Test123!`**.

| Login | Rola | Dostęp do leadów (API, web, mobile) |
|---|---|---|
| `handlowiec` | `Salesperson` | tak — rejestracja i lista własnych leadów |
| `kierownik.sprzedazy` | `SalesManager` | tak — rejestracja i lista **własnych** leadów (widok zespołu nie istnieje, Q-13) |
| `admin` | `Admin` | tak — rejestracja i lista własnych leadów |
| `backoffice` | `BackofficeUser` | nie — API 403, web „Brak dostępu”, mobile „Brak uprawnień…” |
| `kierownik.backoffice` | `BackofficeManager` | nie — jak wyżej |

## Krok 1. SSO (SimpleIdServer)

```powershell
cd Integrations/Sso
./manage-sso.ps1 start     # uruchamia kontenery, czeka na IdServer i rejestruje klientów, role oraz użytkowników testowych
./manage-sso.ps1 status
cd ../..
```

Jeżeli PowerShell blokuje skrypty: `powershell -ExecutionPolicy Bypass -File .\manage-sso.ps1 start`.

Sprawdzenie: http://localhost:5001/master/.well-known/openid-configuration zwraca JSON z
`"issuer": "http://localhost:5001/master"`.

`manage-sso.ps1 start` jest idempotentny — uruchom go ponownie, gdy logowanie zwraca `invalid_client`,
`invalid redirect_uri` albo API odpowiada 403 dla handlowca (brak roli w tokenie). Po ponownym provisioningu
wyloguj się i zaloguj ponownie, aby dostać świeży token.

## Krok 2. Baza danych i obserwowalność

```powershell
docker compose up -d            # PostgreSQL, Seq, Jaeger, Grafana, Prometheus, Loki, Tempo, OTel Collector, RabbitMQ, Redis
# albo minimum potrzebne do testów funkcjonalnych:
docker compose up -d postgres
```

Sprawdzenie: `docker compose ps` — `dotnet-postgres` ma status `healthy`.
Bez kolektora OpenTelemetry API działa normalnie, tylko telemetria nie trafia do Seq ani Jaegera.

## Krok 3. Schemat bazy (migracje EF Core)

API nie tworzy ani nie migruje schematu przy starcie. Zastosuj migracje jawnie (jednorazowo i po każdej nowej migracji):

```powershell
dotnet tool restore
dotnet ef database update --project Backend/src/SDC.CRM.Infrastructure --startup-project Backend/src/SDC.CRM.Api
```

Błąd `relation "Leads" already exists` oznacza bazę utworzoną dawniej przez `EnsureCreated()` — patrz
[Backend/README.md](../../Backend/README.md), sekcja „Migracje schematu”.

## Krok 4. Backend API

```powershell
dotnet run --project Backend/src/SDC.CRM.Api
```

- Domyślny profil `http` z `launchSettings.json`: adres http://localhost:5080, środowisko `Development`
  (ustawienia z `appsettings.Development.json`). Profilu `https` nie używaj do testów z tego katalogu —
  przekierowuje żądania HTTP na `https://localhost:7080`.
- W Riderze: konfiguracja uruchomieniowa `SDC.CRM.Api` z profilem `http`.
- Zatrzymanie: `Ctrl+C`.

Sprawdzenie (bez tokena API musi odpowiedzieć 401 i zwrócić nagłówek korelacji):

```powershell
try { Invoke-WebRequest http://localhost:5080/api/leads/mine -UseBasicParsing } catch { [int]$_.Exception.Response.StatusCode; $_.Exception.Response.Headers['X-Correlation-ID'] }
```

Oczekiwane: `401` i identyfikator korelacji (32 znaki szesnastkowe).

> Uruchomione API blokuje pliki w `bin/Debug`, przez co `dotnet test` w konfiguracji Debug kończy się błędem
> `MSB3027`. Zatrzymaj API przed testami albo uruchamiaj je w Release:
> `dotnet test --solution Backend/SDC.CRM.Backend.slnf -c Release`.

## Krok 5. Aplikacja web (Angular)

```powershell
cd Frontend/Web
nvm use 24.21.0     # opcjonalnie, jeśli używasz nvm - wersja z .nvmrc
npm ci              # za pierwszym razem i po każdej zmianie package-lock.json
npm start           # ng serve -> http://localhost:4200
```

- Serwer deweloperski przekazuje żądania `/api/*` do http://localhost:5080 (`proxy.conf.json`), więc API musi działać.
- Otwieraj aplikację pod adresem `http://localhost:4200` — tylko ten adres jest zarejestrowany w SSO jako adres powrotu.
- Zatrzymanie: `Ctrl+C`.

Sprawdzenie: wejście na http://localhost:4200 przekierowuje na stronę logowania SSO.

## Krok 6a. Aplikacja mobilna — Android

1. Zainstaluj workload (jednorazowo):

   ```powershell
   dotnet workload restore Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj
   ```

2. Uruchom emulator (Android Studio → Device Manager) albo podłącz telefon z włączonym debugowaniem USB.
   Jeśli brakuje Android SDK, możesz go doinstalować tak jak CI:

   ```powershell
   dotnet build Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj -t:InstallAndroidDependencies -f net10.0-android "-p:AndroidSdkDirectory=<ścieżka do Android SDK>" -p:AcceptAndroidSDKLicenses=True
   ```

3. Przekieruj porty SSO i API na `localhost` urządzenia (powtarzaj po każdym restarcie emulatora lub ponownym podłączeniu telefonu):

   ```powershell
   adb devices                     # urządzenie musi mieć status "device"
   adb reverse tcp:5001 tcp:5001
   adb reverse tcp:5080 tcp:5080
   adb reverse --list              # kontrola
   ```

4. Zbuduj, zainstaluj i uruchom aplikację:

   ```powershell
   dotnet build Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj -t:Run -f net10.0-android
   ```

   W Riderze: konfiguracja `SDC.CRM.Mobile`, cel — emulator lub urządzenie z Androidem.

Sprawdzenie: w przeglądarce Chrome **na urządzeniu** otwórz
http://localhost:5001/master/.well-known/openid-configuration — musi pokazać JSON. Jeżeli nie, `adb reverse` nie działa.

Logowanie odbywa się w przeglądarce systemowej urządzenia; po zalogowaniu SSO wraca do aplikacji adresem
`com.sdc.crm.mobile://callback`.

## Krok 6b. Aplikacja mobilna — Windows

```powershell
dotnet workload restore Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj
dotnet run --project Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj -f net10.0-windows10.0.19041.0
```

- Aplikacja działa w trybie niespakowanym (`WindowsPackageType=None`), SSO i API widzi bezpośrednio pod `localhost`.
- W Riderze: konfiguracja `SDC.CRM.Mobile`, cel — Windows.

> **Uwaga — logowanie na Windows nie było weryfikowane end-to-end.** Logowanie używa `WebAuthenticator` z .NET MAUI
> i powrotu adresem `com.sdc.crm.mobile://callback`. Na Windows `WebAuthenticator` ma znane ograniczenia
> (zgłoszenie dotnet/maui#2702), a aplikacja niespakowana nie rejestruje obsługi własnego schematu adresów.
> Jeżeli logowanie zakończy się komunikatem „Logowanie nie powiodło się…”, zapisz to jako wynik scenariusza
> i testuj przepływy po zalogowaniu na Androidzie. Ekran logowania, obsługę błędów i stan połączenia można
> sprawdzić także na Windows.

## Zatrzymanie środowiska

```powershell
# API i web: Ctrl+C w ich terminalach
docker compose stop                                  # baza i obserwowalność (dane zostają)
docker compose -f Integrations/Sso/docker-compose.yml stop   # SSO (dane zostają)
```

`docker compose down -v` usuwa także dane (wolumeny) — wtedy trzeba ponownie zastosować migracje (krok 3),
a dla SSO uruchomić `./manage-sso.ps1 start`.

## Lista kontrolna przed testami

- [ ] http://localhost:5001/master/.well-known/openid-configuration zwraca JSON
- [ ] `docker compose ps`: `dotnet-postgres` — `healthy`
- [ ] migracje zastosowane (`dotnet ef database update` bez błędów)
- [ ] http://localhost:5080/api/leads/mine bez tokena zwraca `401` z nagłówkiem `X-Correlation-ID`
- [ ] http://localhost:4200 przekierowuje na logowanie SSO
- [ ] (Android) `adb reverse --list` pokazuje porty 5001 i 5080

## Rozwiązywanie problemów

| Objaw | Przyczyna | Rozwiązanie |
|---|---|---|
| API nie startuje: „OIDC authority is not configured” albo brak `ConnectionStrings:Crm` | brak konfiguracji środowiska | uruchom z profilem `http` (środowisko `Development`) |
| API nie startuje: port 5080 zajęty | działa inna instancja API (np. z Ridera) | zatrzymaj ją albo używaj już działającej |
| Web: lista pokazuje „Nie udało się pobrać leadów. Sprawdź, czy backend jest uruchomiony.” | API nie działa **albo** zwróciło 401 / 403 (web pokazuje ten sam komunikat dla każdego błędu) | sprawdź kartę Network w DevTools — kod odpowiedzi `/api/leads/mine` |
| Web lub mobile: SSO pokazuje `invalid redirect_uri` / `invalid_client` | klienci w SSO nieaktualni | `./manage-sso.ps1 provision` w `Integrations/Sso` |
| API zwraca 403 dla `handlowiec` | token bez claimu `role` (sprzed provisioningu) | wyloguj się i zaloguj ponownie |
| API zwraca 401 mimo tokena | token wygasł albo wystawiony dla innego adresu niż `http://localhost:5001/master` | zaloguj się ponownie; używaj `localhost`, nie `127.0.0.1` |
| Android: błąd połączenia przy logowaniu lub pobieraniu leadów | brak `adb reverse` | powtórz krok 6a.3 |
| `dotnet test` w Debug: `MSB3027` | działające API blokuje pliki | zatrzymaj API albo `-c Release` |
| `npm run test:ci` zawiesza się bez komunikatu | pytanie Angular CLI przy pierwszym uruchomieniu | uruchom raz interaktywnie albo ustaw `$env:CI = 'true'` |

