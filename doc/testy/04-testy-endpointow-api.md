# 4. Testy endpointów API

Jak przetestować endpointy backendu (`Backend/src/SDC.CRM.Api`) na działającym środowisku:
katalog endpointów, kontrakty, pozyskanie tokena i cztery sposoby wykonania scenariuszy API-xx.

Środowisko: [01-uruchomienie-srodowiska.md](01-uruchomienie-srodowiska.md) — kroki 1–4 (do zdobycia tokenów także krok 5).

## Dostępne endpointy

| Metoda | Ścieżka | Uwierzytelnienie | Role (polityka) | Odpowiedzi |
|---|---|---|---|---|
| `POST` | `/api/leads` | token Bearer z SSO | `Salesperson`, `SalesManager`, `Admin` (`leads:register`) | `201`, `400`, `401`, `403` |
| `GET` | `/api/leads/mine` | token Bearer z SSO | `Salesperson`, `SalesManager`, `Admin` (`leads:view-own`) | `200`, `401`, `403` |
| `GET` | `/openapi/v1.json` | brak | — (tylko środowisko `Development`) | `200` |

Wszystkie odpowiedzi API zawierają nagłówek **`X-Correlation-ID`** (także `401` i `403`).

### `POST /api/leads` — rejestracja leada (CRM-001)

Żądanie (`Content-Type: application/json`):

```json
{
  "companyName": "Acme Sp. z o.o.",
  "contactName": "Jan Kowalski",
  "contactEmail": "jan.kowalski@acme.test",
  "contactPhone": "+48 600 100 200",
  "source": "Targi"
}
```

| Pole | Wymagane | Reguła | Błąd |
|---|---|---|---|
| `companyName` | tak | nie może być puste ani złożone z samych spacji; przycinane | `400` `"A lead must have a company name."` |
| `contactName` | tak | jak wyżej | `400` `"A lead must have a contact name."` |
| `contactEmail` | tak | zawiera `@`, nie na początku ani na końcu; przycinane | `400` `"Contact e-mail is required."` / `"Contact e-mail '…' is not valid."` |
| `contactPhone` | nie | puste → `null`; przycinane | — |
| `source` | nie | puste → `null`; przycinane | — |

- Właściciel leada (handlowiec) **zawsze** pochodzi z tokena (claim `sub`). Pole `assignedSalespersonId` w treści
  jest ignorowane.
- Nowy lead ma status `New`.
- Odpowiedź `201 Created`: `{ "id": "<guid>" }` oraz nagłówek `Location` wskazujący `/api/leads/mine`.
- Naruszenie reguły domenowej: `400` z treścią `ProblemDetails`:
  `{ "title": "Business rule violated", "detail": "<reguła>", "status": 400 }`.
- Brak pola lub niepoprawny JSON: `400` z walidacji żądania ASP.NET Core (`ValidationProblemDetails` z polem
  `errors`) albo z reguły domenowej.
- Limity długości kolumn w bazie: `companyName` i `contactName` 200, `contactEmail` 320, `contactPhone` 50,
  `source` 100 znaków. API ich nie sprawdza — patrz scenariusz API-50 (znana luka).

### `GET /api/leads/mine` — moje leady (CRM-002)

Zwraca leady, których właścicielem jest wywołujący, posortowane od najnowszego:

```json
[
  {
    "id": "3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77",
    "companyName": "Acme Sp. z o.o.",
    "contactName": "Jan Kowalski",
    "contactEmail": "jan.kowalski@acme.test",
    "status": "New",
    "createdAtUtc": "2026-10-01T19:30:00.000000Z"
  }
]
```

`status`: `New`, `Qualified` albo `Rejected` (przez API osiągalny jest obecnie tylko `New`).
`SalesManager` i `Admin` także widzą tylko **własne** leady (Q-13).

### Nagłówek `X-Correlation-ID`

- Wartość przesłana przez klienta wraca bez zmian, jeśli jest bezpieczna: do 64 znaków `A-Z a-z 0-9 - _ . :`.
- Brak nagłówka lub wartość niebezpieczna → API nadaje nowy identyfikator (trace-id W3C, 32 znaki szesnastkowe).
- Ten sam identyfikator trafia do logów (`CorrelationId`) i do śladu (tag `correlation.id`).

## Pozyskanie tokena dostępowego

Klienci w SSO są publiczni (Authorization Code + PKCE), więc token uzyskuje się przez logowanie w przeglądarce.
Najprościej przez aplikację web:

1. Otwórz **okno incognito / InPrivate** (osobne dla każdego użytkownika) i zaloguj się na http://localhost:4200.
2. Otwórz DevTools (`F12`) → Console i wykonaj:

   ```javascript
   copy(sessionStorage.getItem('access_token'))
   ```

   Token jest w schowku. To samo znajdziesz w Application → Session Storage → `http://localhost:4200` → `access_token`.
3. Zapisz tokeny w zmiennych PowerShell:

   ```powershell
   $handlowiec = '<token użytkownika handlowiec>'
   $kierownik  = '<token użytkownika kierownik.sprzedazy>'
   $backoffice = '<token użytkownika backoffice>'
   ```

Podgląd zawartości tokena (rola, wystawca, ważność) bez wysyłania go do zewnętrznych serwisów:

```powershell
$payload = $handlowiec.Split('.')[1].Replace('-', '+').Replace('_', '/')
$payload += '=' * ((4 - $payload.Length % 4) % 4)
$claims = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload)) | ConvertFrom-Json
$claims | Select-Object sub, role, iss, aud
[DateTimeOffset]::FromUnixTimeSeconds($claims.exp).LocalDateTime   # ważny do
```

Oczekiwane: `iss` = `http://localhost:5001/master`, `role` zawiera rolę użytkownika (np. `Salesperson`).
Brak `role` → uruchom `./manage-sso.ps1 provision` i zaloguj się ponownie. Po wygaśnięciu tokena API zwraca `401` —
odśwież stronę aplikacji web (odświeży token) i skopiuj go ponownie.

## Sposób 1 — skrypt `test-api.ps1` (zalecany)

[api/test-api.ps1](api/test-api.ps1) wykonuje automatycznie scenariusze API-01…API-41 i wypisuje wynik każdego z nich.
Działa w Windows PowerShell 5.1 i PowerShell 7.

```powershell
# bez tokenów: uwierzytelnianie, nagłówek korelacji, OpenAPI (API-01..API-08)
./doc/testy/api/test-api.ps1

# komplet scenariuszy
./doc/testy/api/test-api.ps1 -SalespersonToken $handlowiec -OtherSalesToken $kierownik -BackofficeToken $backoffice

# inny adres API
./doc/testy/api/test-api.ps1 -BaseUrl http://localhost:5080
```

- Grupy bez podanego tokena są pomijane (`pominięto`).
- Skrypt tworzy leady z prefiksem `[SMOKE]` i wypisuje ich identyfikatory (API nie ma usuwania leadów).
- Kod wyjścia `1` oznacza co najmniej jeden niezgodny wynik.
- Jeśli PowerShell blokuje skrypty: `powershell -ExecutionPolicy Bypass -File .\doc\testy\api\test-api.ps1 …`.

## Sposób 2 — klient HTTP w Riderze

[api/crm-api.http](api/crm-api.http) zawiera te same scenariusze z asercjami (bloki `> {% … %}`).

1. Obok pliku utwórz `doc/testy/api/http-client.private.env.json` (plik jest w `.gitignore`):

   ```json
   {
     "local": {
       "salespersonToken": "<token handlowiec>",
       "otherSalesToken": "<token kierownik.sprzedazy>",
       "backofficeToken": "<token backoffice>"
     }
   }
   ```

2. Otwórz `crm-api.http`, wybierz środowisko `local` (adres z [api/http-client.env.json](api/http-client.env.json)).
3. Uruchom pojedyncze żądanie (ikona ▶ przy `###`) albo cały plik („Run All Requests in File”).
   Wyniki asercji są w oknie Services → HTTP Client → Tests.

Kolejność ma znaczenie: API-13 i API-30 korzystają z identyfikatora zapisanego przez wcześniejsze żądania
(`client.global.set("leadId", …)`).

## Sposób 3 — PowerShell ręcznie

```powershell
$api = 'http://localhost:5080'
$h = @{ Authorization = "Bearer $handlowiec"; 'X-Correlation-ID' = 'manual-1' }

# moje leady
Invoke-RestMethod "$api/api/leads/mine" -Headers $h

# rejestracja leada
$body = @{ companyName = '[TEST-API] Acme'; contactName = 'Jan Kowalski'; contactEmail = 'jan@acme.test'; source = 'Targi' } | ConvertTo-Json
Invoke-RestMethod "$api/api/leads" -Method Post -Headers $h -ContentType 'application/json' -Body $body

# odpowiedź błędu (4xx): kod i treść
try {
    $bad = @{ companyName = '   '; contactName = 'Jan'; contactEmail = 'jan@acme.test' } | ConvertTo-Json
    Invoke-RestMethod "$api/api/leads" -Method Post -Headers $h -ContentType 'application/json' -Body $bad
}
catch {
    [int]$_.Exception.Response.StatusCode     # 400
    $_.ErrorDetails.Message                   # ProblemDetails z "detail"
}
```

## Sposób 4 — curl

W PowerShell wywołuj `curl.exe` (samo `curl` to alias `Invoke-WebRequest` w Windows PowerShell 5.1).
Treść żądania najwygodniej przekazać z pliku:

```powershell
'{"companyName":"[TEST-API] Curl","contactName":"Jan Kowalski","contactEmail":"jan@acme.test"}' | Set-Content lead.json -Encoding ascii
curl.exe -i -X POST http://localhost:5080/api/leads -H "Authorization: Bearer $handlowiec" -H "Content-Type: application/json" --data-binary "@lead.json"
curl.exe -i http://localhost:5080/api/leads/mine -H "Authorization: Bearer $handlowiec"
```

`-i` pokazuje nagłówki odpowiedzi, w tym `X-Correlation-ID` i `Location`.

## Katalog scenariuszy API

Kolumna „Automat”: S — [api/test-api.ps1](api/test-api.ps1), H — [api/crm-api.http](api/crm-api.http), R — tylko ręcznie.

| ID | Token | Żądanie | Oczekiwany wynik | Automat |
|---|---|---|---|---|
| API-01 | brak | `GET /api/leads/mine` | `401` | S, H |
| API-02 | brak | `POST /api/leads` (poprawna treść) | `401`, nic nie zapisano | S, H |
| API-03 | nieprawidłowy (`Bearer not-a-valid.jwt.token`) | `GET /api/leads/mine` | `401` | S |
| API-04 | brak | `GET /api/leads/mine` z `X-Correlation-ID: smoke-…` | ten sam identyfikator w odpowiedzi | S, H |
| API-05 | brak | `X-Correlation-ID: <b>x</b>` | nowy identyfikator w odpowiedzi | S |
| API-06 | brak | `X-Correlation-ID` dłuższy niż 64 znaki | nowy identyfikator w odpowiedzi | S |
| API-07 | brak | bez `X-Correlation-ID` | identyfikator nadany przez API | S |
| API-08 | brak | `GET /openapi/v1.json` | `200`, dokument zawiera `/api/leads` i `/api/leads/mine` | S, H |
| API-10 | handlowiec | `GET /api/leads/mine` | `200`, tablica JSON | S, H |
| API-11 | handlowiec | `POST /api/leads` — wszystkie pola | `201`, `id` (GUID) i nagłówek `Location` | S, H |
| API-12 | handlowiec | `POST /api/leads` — tylko pola wymagane | `201` | S, H |
| API-13 | handlowiec | `GET /api/leads/mine` | utworzone leady na liście, status `New`, najnowszy pierwszy | S, H |
| API-14 | handlowiec | `companyName: "   "` | `400`, `detail` = `A lead must have a company name.` | S, H |
| API-15 | handlowiec | `contactName: "   "` | `400`, `detail` = `A lead must have a contact name.` | S, H |
| API-16 | handlowiec | `contactEmail: "jan.kowalski"` | `400`, `detail` zawiera `is not valid` | S, H |
| API-17 | handlowiec | `contactEmail: ""` | `400` | S |
| API-18 | handlowiec | brak pola `companyName` | `400` | S, H |
| API-19 | handlowiec | niepoprawny JSON | `400` | S, H |
| API-20 | handlowiec | treść z `assignedSalespersonId` innej osoby | `201`, lead na liście **wywołującego** | S, H |
| API-21 | handlowiec | `GET /api/leads/mine` z `X-Correlation-ID` | `200`, ten sam identyfikator; widoczny w Seq i Jaegerze | S |
| API-30 | kierownik.sprzedazy | `GET /api/leads/mine` | `200`, brak leadów handlowca | S, H |
| API-40 | backoffice | `GET /api/leads/mine` | `403` | S, H |
| API-41 | backoffice | `POST /api/leads` | `403`, nic nie zapisano | S, H |
| API-50 | handlowiec | `contactPhone` dłuższy niż 50 znaków | oczekiwane biznesowo `400`; **prawdopodobnie obecnie `500`** (błąd bazy) — znana luka do zgłoszenia | R |
| API-51 | handlowiec | token po wygaśnięciu (`exp`) | `401` | R |
| API-52 | admin | `GET /api/leads/mine`, `POST /api/leads` | `200` / `201` — rola `Admin` ma dostęp | R |

## Weryfikacja w bazie danych

Połącz się z bazą CRM (interaktywnie — unikasz problemów z cudzysłowami w PowerShell):

```powershell
docker exec -it dotnet-postgres psql -U app -d appdb
```

```sql
-- ostatnie leady
SELECT "CompanyName", "ContactName", "ContactPhone", "Source", "Status", "AssignedSalespersonId", "CreatedAtUtc"
FROM "Leads" ORDER BY "CreatedAtUtc" DESC LIMIT 10;

-- leady utworzone przez testy
SELECT COUNT(*) FROM "Leads" WHERE "CompanyName" LIKE '[SMOKE]%' OR "CompanyName" LIKE '[TEST-%';

-- zastosowane migracje
SELECT "MigrationId" FROM "__EFMigrationsHistory";
```

Oczekiwane: leady jednego użytkownika mają ten sam `AssignedSalespersonId` (wyliczany z claimu `sub` tokena),
różni użytkownicy — różne wartości; puste pola opcjonalne to `NULL`; `\q` kończy sesję.

Aby wyczyścić dane testowe, odtwórz lokalną bazę (usuwa **wszystkie** leady):

```powershell
docker exec -it dotnet-postgres psql -U app -d postgres -c "DROP DATABASE appdb WITH (FORCE);"
dotnet ef database update --project Backend/src/SDC.CRM.Infrastructure --startup-project Backend/src/SDC.CRM.Api
```

## Weryfikacja obserwowalności

Wymaga `docker compose up -d` (kolektor OpenTelemetry, Seq, Jaeger). Wyślij żądanie z własnym identyfikatorem
(np. API-21 albo `X-Correlation-ID: manual-1`), a następnie:

| Narzędzie | Gdzie | Co sprawdzić |
|---|---|---|
| Seq | http://localhost:5341 → zapytanie `CorrelationId = 'manual-1'` | wpisy logu żądania, m.in. „Lead … registered by …” po rejestracji |
| Jaeger | http://localhost:16686 → Service `sdc-crm-api` → Tags `correlation.id=manual-1` | ślad żądania HTTP z zapytaniem do PostgreSQL |
| Grafana | http://localhost:3000 → Explore (Tempo / Loki / Prometheus) | te same ślady i logi, metryki HTTP |

Logi nie mogą zawierać tokenów ani danych kontaktowych klienta (zgodnie z regułami obserwowalności) — przy okazji
sprawdź, że w Seq nie widać nagłówka `Authorization` ani adresów e-mail z treści żądań.

## Testy automatyczne backendu

Scenariusze ręczne uzupełniają testy jednostkowe i integracyjne HTTP (TestServer, bez bazy i SSO), opisane w
[Backend/README.md](../../Backend/README.md):

```powershell
dotnet test --solution Backend/SDC.CRM.Backend.slnf -c Release
```

