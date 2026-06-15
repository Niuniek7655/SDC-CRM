# SDC-CRM Frontend

Frontend aplikacji CRM napisany w **Angular 22** (standalone, signals, nowy build system `@angular/build`).

## Zastosowana architektura

Użyto popularnego w ekosystemie Angulara podejścia **feature-based + warstwy** (czasem
nazywanego "feature-sliced"). Kod jest podzielony według **funkcji biznesowych**, a nie
według typów technicznych. Dzięki temu aplikacja dobrze się skaluje, a każda funkcja jest
samowystarczalna i ładowana leniwie (lazy loading).

```text
src/
  environments/            # konfiguracja środowisk (dev / prod)
  app/
    app.ts                 # komponent główny (tylko <router-outlet/>)
    app.config.ts          # konfiguracja providerów (router, http, change detection)
    app.routes.ts          # routing najwyższego poziomu
    core/                  # rzeczy globalne, jednorazowe (singletony)
      models/              #   - modele/typy domenowe
      interceptors/        #   - interceptory HTTP (np. obsługa błędów)
    shared/                # komponenty/pipe'y/dyrektywy współdzielone między funkcjami
    layout/                # szkielet UI (Shell: nagłówek, nawigacja, outlet)
    features/              # funkcje biznesowe (każda lazy-loaded)
      leads/
        leads.routes.ts    #   - routing funkcji
        data/              #   - dostęp do danych (serwisy API)
        pages/             #   - widoki (strony) funkcji
          lead-list/
          lead-register/
```

### Kluczowe zasady

- **Standalone components** — bez `NgModule`. Bootstrap przez `bootstrapApplication`.
- **Signals** — stan komponentów trzymany w `signal()` (reaktywność).
- **Nowy control flow** — `@if`, `@for`, `@switch` w szablonach (zamiast `*ngIf`/`*ngFor`).
- **`inject()`** zamiast wstrzykiwania przez konstruktor.
- **`ChangeDetectionStrategy.OnPush`** we wszystkich komponentach.
- **Lazy loading** — funkcje i strony ładowane przez `loadChildren` / `loadComponent`.
- **Funkcyjne interceptory** (`HttpInterceptorFn`) zamiast klas.
- **Reactive Forms** (typowane, `NonNullableFormBuilder`) dla formularzy.
- **Separacja warstw** — `core` (globalne), `shared` (wielokrotnego użytku),
  `features` (biznes), `layout` (szkielet UI).

### Dlaczego tak?

- Podział wg funkcji = łatwiej znaleźć i rozwijać kod jednej domeny (np. `leads/`).
- Lazy loading = mniejszy bundle startowy i szybsze pierwsze ładowanie.
- `core` zbiera elementy, które mają istnieć w jednej instancji w całej aplikacji.
- `shared` zapobiega duplikacji wspólnego UI.

## Zaimplementowana funkcja: Leady

Spójna z backendem (`/api/leads`):

- **Lista moich leadów** — `GET /api/leads/mine?salespersonId=...` (`/leads`)
- **Rejestracja leada** — `POST /api/leads` (`/leads/new`)

> Uwaga: `salespersonId` jest tymczasowo zahardkodowane (`...0001`) do czasu dodania
> uwierzytelniania. W kodzie oznaczono to komentarzami `TODO`.

## Wymagania

- Node.js 20.19+ / 22.12+ (zgodnie z wymaganiami Angular 22)
- npm 10+

## Uruchamianie

```bash
npm install
npm start            # ng serve -> http://localhost:4200
```

Dev server proxuje żądania `/api/*` do backendu pod `http://localhost:5080`
(konfiguracja w `proxy.conf.json`), dzięki czemu nie ma problemów z CORS.
Upewnij się, że backend (`backend/src/SDC.CRM.Api`) jest uruchomiony.

## Budowanie i testy

```bash
npm run build        # produkcyjny build do dist/
npm test             # testy jednostkowe (Karma + Jasmine)
```

## Możliwe kolejne kroki

- Dodać `core/auth` (logowanie, guardy tras, token interceptor) i prawdziwego `salespersonId`.
- Dodać kolejne funkcje zgodnie z workflow CRM: `opportunities`, `orders`, `backoffice`.
- Rozważyć tryb **zoneless** (`provideZonelessChangeDetection`) — projekt jest już oparty
  na signals i `OnPush`, więc migracja będzie prosta.
