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
- `tests/SDC.CRM.Api.Tests` — testy tożsamości/uwierzytelniania (`CurrentUser` mapujący claimy tokena na role i identyfikator domenowy).

Konwencja nazw testów: `Metoda__When_scenariusz__Should_oczekiwany_rezultat`.

## Zasady warstw

- Reguły biznesowe są w domenie (agregaty/encje/value objecty), nie w kontrolerach.
- Domena nie zna EF Core ani ASP.NET.
- API nie wystawia encji persystencji — używa kontraktów request/response oraz DTO.
- Repozytoria są zdefiniowane jako interfejsy w `Application`, a implementowane w `Infrastructure`.

## Zaimplementowany pionowy wycinek (vertical slice)

Zgodnie z kolejnością wdrażania z instrukcji projektu:

1. **Zarejestruj leada** — `POST /api/leads`
2. **Pokaż moje leady** — `GET /api/leads/mine?salespersonId={guid}`

Agregat `Lead` pilnuje reguł:

- lead musi mieć nazwę firmy, osobę kontaktową i przypisanego handlowca,
- odrzucony lead nie może być zakwalifikowany,
- odrzucenie leada wymaga podania powodu.

## Wymagania

- .NET SDK 10.0+

## Budowanie i uruchamianie

Wszystkie polecenia uruchamiaj z katalogu `backend` (lub wskaż solucję `../SDC-CRM.slnx`).

```bash
dotnet restore
dotnet build
dotnet test

# uruchomienie API
dotnet run --project src/SDC.CRM.Api
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
Schemat jest tworzony automatycznie przy starcie przez `EnsureCreated()` (tylko na potrzeby developmentu).

> TODO (decyzja techniczna): przed produkcją zastąpić `EnsureCreated()` migracjami EF Core
> (`dotnet ef migrations add`) i uruchamiać je kontrolowanie zamiast tworzenia schematu w runtime.

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

Brak `ConnectionStrings:Crm` lub `Oidc:Authority` zatrzymuje start aplikacji z komunikatem
wskazującym brakujące ustawienie (fail-fast zamiast cichego użycia wartości deweloperskich).

## Przykładowe żądanie

```bash
curl -X POST http://localhost:5080/api/leads \
  -H "Content-Type: application/json" \
  -d '{
    "companyName": "Acme Sp. z o.o.",
    "contactName": "Jan Kowalski",
    "contactEmail": "jan.kowalski@acme.test",
    "contactPhone": "+48 600 100 200",
    "source": "Targi",
    "assignedSalespersonId": "00000000-0000-0000-0000-000000000001"
  }'
```
