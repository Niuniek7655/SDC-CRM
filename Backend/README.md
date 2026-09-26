# SDC-CRM Backend

Backend aplikacji CRM napisany w **.NET 10** zgodnie z **Clean Architecture**.

## Architektura

Projekt podzielony jest na cztery warstwy. Zależności wskazują zawsze "do wewnątrz" — w stronę domeny.

```text
SDC.CRM.Api            (Presentation)  ->  Application + Infrastructure
SDC.CRM.Infrastructure (Infrastructure) ->  Application + Domain
SDC.CRM.Application    (Application)   ->  Domain
SDC.CRM.Domain         (Domain)        ->  (brak zależności)
```

| Warstwa | Projekt | Odpowiedzialność |
| --- | --- | --- |
| Domain | `src/SDC.CRM.Domain` | Encje, agregaty, value objecty, zdarzenia domenowe, reguły biznesowe. Bez zależności zewnętrznych. |
| Application | `src/SDC.CRM.Application` | Przypadki użycia (workflow), kontrakty (DTO), abstrakcje persystencji. |
| Infrastructure | `src/SDC.CRM.Infrastructure` | EF Core, `DbContext`, repozytoria, rejestracja zależności. |
| Api | `src/SDC.CRM.Api` | ASP.NET Core, kontrolery, kontrakty HTTP, composition root. |

Testy (zgodnie z regułą 99: **TUnit** + **NSubstitute**):

- `tests/SDC.CRM.Domain.Tests` — testy reguł biznesowych agregatów (prawdziwe obiekty domenowe).
- `tests/SDC.CRM.Application.Tests` — testy przypadków użycia (porty zastąpione substytutami NSubstitute).
- `tests/SDC.CRM.Api.Tests` — testy jednostkowe warstwy API: `CurrentUser` (claimy tokena → role i identyfikator domenowy),
  walidacja konfiguracji przy starcie (fail-fast) i `CorrelationIdMiddleware`.
- `tests/SDC.CRM.Api.IntegrationTests` — testy integracyjne HTTP: prawdziwy pipeline API uruchomiony w pamięci (TestServer).

Konwencja nazw testów: `Metoda__When_scenariusz__Should_oczekiwany_rezultat`.
Opis działania testów API: [Jak działają testy API](#jak-działają-testy-api).

## Zasady warstw

- Reguły biznesowe są w domenie (agregaty/encje/value objecty), nie w kontrolerach.
- Domena nie zna EF Core ani ASP.NET.
- API nie wystawia encji persystencji — używa kontraktów request/response oraz DTO.
- Repozytoria są zdefiniowane jako interfejsy w `Application`, a implementowane w `Infrastructure`.

## Zaimplementowany pionowy wycinek (vertical slice)

Zgodnie z kolejnością wdrażania z instrukcji projektu:

1. **Zarejestruj leada** — `POST /api/leads` (odpowiedź `201 Created` z `{ "id": "<guid>" }`)
2. **Pokaż moje leady** — `GET /api/leads/mine`

Oba endpointy wymagają tokena dostępowego SSO (`Authorization: Bearer ...`) z rolą `Salesperson`,
`SalesManager` lub `Admin`. Właściciel leada jest ustalany z tokena (`CurrentUser`), nigdy z treści żądania.

Agregat `Lead` pilnuje reguł:

- lead musi mieć nazwę firmy, osobę kontaktową i przypisanego handlowca,
- odrzucony lead nie może być zakwalifikowany,
- odrzucenie leada wymaga podania powodu.

## Wymagania

- .NET SDK 10.0+

## Budowanie i uruchamianie

Polecenia uruchamiaj z katalogu głównego repozytorium. Filtr solucji `Backend/SDC.CRM.Backend.slnf`
obejmuje wszystkie projekty backendu (bez aplikacji MAUI), a `global.json` włącza tryb
Microsoft.Testing.Platform wymagany przez TUnit w `dotnet test` na .NET 10 SDK.

```bash
dotnet restore Backend/SDC.CRM.Backend.slnf
dotnet build Backend/SDC.CRM.Backend.slnf
dotnet test --solution Backend/SDC.CRM.Backend.slnf

# jednorazowo: narzędzia lokalne (dotnet-ef) i schemat bazy - patrz "Migracje schematu"
dotnet tool restore
dotnet ef database update --project Backend/src/SDC.CRM.Infrastructure --startup-project Backend/src/SDC.CRM.Api

# uruchomienie API (profil Development z launchSettings.json)
dotnet run --project Backend/src/SDC.CRM.Api
```

API domyślnie nasłuchuje na `http://localhost:5080` (oraz `https://localhost:7080`).
W trybie Development dostępny jest dokument OpenAPI pod `/openapi/v1.json`.

## Baza danych

Używany jest **PostgreSQL** (dostawca `Npgsql.EntityFrameworkCore.PostgreSQL`).
Lokalnie baza uruchamiana jest przez główny `docker-compose.yml` w katalogu repozytorium
(usługa `postgres`: domyślnie baza `appdb`, użytkownik/hasło `app`/`app`, port `5432`).
Domyślne wartości można nadpisać w pliku `.env` w katalogu głównym (wzór: `.env.example`).

```bash
# z katalogu głównego repozytorium
docker compose up -d postgres
```

Connection string konfigurowany jest pod kluczem `ConnectionStrings:Crm`
(wartość deweloperska w `appsettings.Development.json`, patrz [Konfiguracja środowisk](#konfiguracja-środowisk)).

### Migracje schematu (EF Core)

Schemat bazy jest zarządzany migracjami EF Core w `src/SDC.CRM.Infrastructure/Persistence/Migrations`.
Aplikacja **nie** tworzy ani nie migruje schematu przy starcie - migracje stosuje się zawsze jawnie,
także lokalnie. Narzędzie `dotnet-ef` jest przypięte w manifeście `dotnet-tools.json` w katalogu głównym
(`dotnet tool restore`). Projekt `SDC.CRM.Api` jest projektem startowym narzędzi - w środowisku
Development connection string pochodzi z `appsettings.Development.json`.

```bash
# z katalogu głównego repozytorium
dotnet tool restore

# zastosowanie brakujących migracji (tworzy bazę, jeśli nie istnieje)
dotnet ef database update --project Backend/src/SDC.CRM.Infrastructure --startup-project Backend/src/SDC.CRM.Api

# nowa migracja po zmianie modelu (np. nowe pole agregatu) - przeglądana w PR jak zwykły kod
dotnet ef migrations add <NazwaMigracji> --project Backend/src/SDC.CRM.Infrastructure --startup-project Backend/src/SDC.CRM.Api --output-dir Persistence/Migrations

# kontrola: czy model ma zmiany bez migracji
dotnet ef migrations has-pending-model-changes --project Backend/src/SDC.CRM.Infrastructure --startup-project Backend/src/SDC.CRM.Api
```

Inne środowiska: idempotentny skrypt SQL do przeglądu (`dotnet ef migrations script --idempotent ...`)
albo samodzielny plik wykonywalny migracji uruchamiany w procesie wdrożenia
(`dotnet ef migrations bundle ...`, a następnie `./efbundle --connection "<connection string>"`).

> **Jednorazowo po przejściu z `EnsureCreated()`**: lokalna baza `appdb` utworzona wcześniej przy starcie API
> nie ma tabeli `__EFMigrationsHistory`, więc `database update` zgłosi błąd `relation "Leads" already exists`.
> Usuń ją raz, np. `docker exec -it dotnet-postgres psql -U app -d postgres -c "DROP DATABASE appdb;"`,
> i ponownie uruchom `dotnet ef database update` (tracone są tylko lokalne dane testowe).

## Konfiguracja środowisk

`appsettings.json` zawiera wyłącznie ustawienia niezależne od środowiska i nie ujawnia
connection stringa ani adresu dostawcy tożsamości. Wartości do lokalnego developmentu i debugowania
są w `appsettings.Development.json` (profil `http`/`https` w `launchSettings.json` ustawia
`ASPNETCORE_ENVIRONMENT=Development`). W pozostałych środowiskach ustawienia przekazuje się zmiennymi
środowiskowymi (lub magazynem sekretów platformy):

| Ustawienie | Development (`appsettings.Development.json`) | Inne środowiska (zmienna środowiskowa) |
| --- | --- | --- |
| `ConnectionStrings:Crm` | `Host=localhost;Port=5432;Database=appdb;...` | `ConnectionStrings__Crm` |
| `Oidc:Authority` | `http://localhost:5001/master` | `Oidc__Authority` |
| `Oidc:RequireHttpsMetadata` | `false` | domyślnie `true` |
| `Oidc:AllowedCorsOrigins` | `http://localhost:4200` | `Oidc__AllowedCorsOrigins__0`, `__1`, ... |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `http://localhost:4317` (OpenTelemetry Collector) | `OTEL_EXPORTER_OTLP_ENDPOINT` (brak = telemetria nie jest wysyłana) |

Brak `ConnectionStrings:Crm` lub `Oidc:Authority` zatrzymuje start aplikacji z komunikatem
wskazującym brakujące ustawienie (fail-fast zamiast cichego użycia wartości deweloperskich).

## Obserwowalność

API wysyła ślady (traces), metryki i logi przez OTLP do OpenTelemetry Collector
(stos lokalny opisany w `infra/README.md`). Kod domeny i aplikacji nie zależy od bibliotek telemetrii.

- Instrumentacja: żądania HTTP (ASP.NET Core), wywołania `HttpClient`, zapytania do PostgreSQL (Npgsql)
  oraz metryki środowiska uruchomieniowego .NET.
- Eksport włącza ustawienie `OTEL_EXPORTER_OTLP_ENDPOINT`; bez niego (np. w testach) telemetria nie opuszcza procesu.
- Każde żądanie otrzymuje identyfikator korelacji w nagłówku `X-Correlation-ID`. Identyfikator przesłany przez klienta
  jest zachowywany, jeśli jest bezpieczny (do 64 znaków `A-Z a-z 0-9 - _ . :`); w przeciwnym razie nadawany jest nowy -
  trace-id bieżącego śladu W3C, więc nagłówek, logi i ślad mają ten sam identyfikator. Identyfikator wraca w odpowiedzi
  (także dla klientów przeglądarkowych przez CORS), trafia do scope logów (`CorrelationId`) i jako tag `correlation.id` do śladu.
- Podgląd lokalnie: Seq `http://localhost:5341`, Jaeger `http://localhost:16686`, Grafana `http://localhost:3000`.

## Jak działają testy API

Uruchomienie: `dotnet test --solution Backend/SDC.CRM.Backend.slnf` (wszystkie projekty) albo
`dotnet test --project Backend/tests/<Projekt>/<Projekt>.csproj`. Żaden test nie wymaga bazy danych, SSO ani sieci.

### Testy jednostkowe (`SDC.CRM.Api.Tests`)

- **`Configuration/ConfigurationValidationTests`** - buduje `ServiceCollection` z konfiguracją w pamięci
  (`AddInMemoryCollection`) i wywołuje `AddInfrastructure` oraz `AddCrmAuthentication`. Bez `ConnectionStrings:Crm`
  lub `Oidc:Authority` oczekuje `InvalidOperationException`, której komunikat wskazuje zmienną środowiskową
  (`ConnectionStrings__Crm`, `Oidc__Authority`); z ustawieniem sprawdza rejestrację `CrmDbContext` i `OidcOptions`.
  Rejestracja usług niczego nie łączy - test działa bez PostgreSQL i bez dostawcy tożsamości.
- **`Observability/CorrelationIdMiddlewareTests`** - wywołuje middleware bezpośrednio na `DefaultHttpContext`
  (bez serwera HTTP), z `next` jako lambdą. Sprawdza, że:
  - poprawny `X-Correlation-ID` klienta wraca bez zmian w odpowiedzi,
  - przy braku nagłówka identyfikator jest generowany, a gdy trwa żądanie (`Activity` w formacie W3C) - jest to jego trace-id,
  - wartości niebezpieczne (CR/LF - próba wstrzyknięcia nagłówka, same spacje, HTML, ponad 64 znaki) są zastępowane
    nowym identyfikatorem - jeden test sparametryzowany atrybutami `[Arguments]`,
  - identyfikator jest dostępny dla dalszej części pipeline'u (`CorrelationIdMiddleware.GetCorrelationId`),
  - bieżąca aktywność dostaje tag `correlation.id`,
  - otwierany jest scope logów z kluczem `CorrelationId` - `ILogger` jest substytutem NSubstitute i weryfikowane jest
    wywołanie `BeginScope`, bo to właśnie ono dołącza identyfikator do każdego wpisu logu żądania.
- **`Identity/CurrentUserTests`** (istniejące) - mapowanie claimów tokena na identyfikator domenowy i role.

### Testy integracyjne (`SDC.CRM.Api.IntegrationTests`)

`Infrastructure/CrmApiFactory` (`WebApplicationFactory<Program>`) uruchamia prawdziwe API w pamięci (TestServer):
ten sam `Program.cs`, routing, middleware, polityki autoryzacji i serializacja JSON. Różnice względem produkcji:

1. Środowisko `Testing` oraz zastępcze `ConnectionStrings:Crm` i `Oidc:Authority` - wymagane przez walidację
   fail-fast, ale nigdy nieużywane.
2. Schemat uwierzytelniania `Test` (`Infrastructure/TestAuthenticationHandler`) zamiast walidacji JWT. Żądanie jest
   uwierzytelnione, gdy ma nagłówek `X-Test-Subject`; `X-Test-Roles` podaje role (np. `Salesperson`). Handler tworzy
   claimy `sub`, `role` i `name` - takie jak w tokenach SimpleIdServer - więc `CurrentUser` i polityki `CrmPolicies`
   działają dokładnie jak w produkcji. Brak nagłówka oznacza brak poświadczeń, czyli odpowiedź 401.
3. `ILeadRepository` i `IUnitOfWork` są zastąpione substytutami NSubstitute - brak bazy danych; test może sprawdzić,
   czy i z jakim agregatem nastąpił zapis.

Każdy test tworzy własną fabrykę (`await using var factory = new CrmApiFactory()`), więc substytuty nie są
współdzielone i testy mogą biec równolegle. `Leads/LeadsEndpointsTests` sprawdza:

| Test | Scenariusz | Oczekiwany wynik |
| --- | --- | --- |
| `RegisterLead__When_request_has_no_credentials__...` | `POST /api/leads` bez poświadczeń | 401, repozytorium nie zostało wywołane |
| `RegisterLead__When_user_has_no_sales_role__...` | rola `BackofficeUser` | 403, brak zapisu |
| `RegisterLead__When_salesperson_sends_valid_lead__...` | rola `Salesperson` | 201, `id` w treści; lead przypisany do handlowca z tokena (nie z żądania), `SaveChanges` wywołane raz |
| `RegisterLead__When_business_rule_is_violated__...` | pusta nazwa firmy | 400 `ProblemDetails` z opisem reguły, brak `SaveChanges` |
| `GetMyLeads__When_request_has_no_credentials__...` | `GET /api/leads/mine` bez poświadczeń | 401 |
| `GetMyLeads__When_user_has_no_sales_role__...` | rola `BackofficeManager` | 403 |
| `GetMyLeads__When_salesperson_requests_leads__...` | handlowiec z jednym leadem | 200, lista z leadem wywołującego (repozytorium pytane o jego id) |
| `AnyEndpoint__When_request_carries_correlation_id__...` | nagłówek `X-Correlation-ID` | ten sam identyfikator w odpowiedzi |

## Przykładowe żądanie

```bash
# ACCESS_TOKEN - token dostępowy z SSO dla użytkownika z rolą Salesperson
# (np. zaloguj się w aplikacji web i skopiuj token albo użyj przycisku "Authorize" w dokumencie OpenAPI)
curl -X POST http://localhost:5080/api/leads \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: demo-request-1" \
  -d '{
    "companyName": "Acme Sp. z o.o.",
    "contactName": "Jan Kowalski",
    "contactEmail": "jan.kowalski@acme.test",
    "contactPhone": "+48 600 100 200",
    "source": "Targi"
  }'
```

Odpowiedź `201 Created` (nagłówek `X-Correlation-ID: demo-request-1`):

```json
{ "id": "3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77" }
```

Bez tokena API zwraca `401`, dla ról spoza sprzedaży `403`, a przy naruszeniu reguły biznesowej
`400` z `ProblemDetails` (np. `"detail": "A lead must have a company name."`).

