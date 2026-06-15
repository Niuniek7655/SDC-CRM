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

Testy:

- `tests/SDC.CRM.Domain.Tests` — testy reguł biznesowych agregatów.
- `tests/SDC.CRM.Application.Tests` — testy przypadków użycia (z atrapami).

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

Domyślnie używany jest **SQLite** (`crm.db`), tworzony automatycznie przy starcie
przez `EnsureCreated()` (tylko na potrzeby developmentu).

> TODO (decyzja techniczna): przed produkcją zastąpić `EnsureCreated()` migracjami EF Core
> oraz docelowym dostawcą bazy (np. SQL Server / PostgreSQL).

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
