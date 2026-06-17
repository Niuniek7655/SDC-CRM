# SDC CRM Mobile

Aplikacja mobilna .NET MAUI dla systemu SDC CRM.

## Obsługiwane platformy

- Android (API 21+)
- Windows (10.0.17763+)

## Wymagania

- .NET 10 SDK
- Workloads: `maui-android`, `maui-windows`

## Instalacja workloadów

```bash
dotnet workload restore
```

Lub ręcznie:

```bash
dotnet workload install maui-android maui-windows
```

## Budowanie

```bash
dotnet restore
dotnet build
```

### Budowanie dla konkretnej platformy

**Android:**
```bash
dotnet build -f net10.0-android
```

**Windows:**
```bash
dotnet build -f net10.0-windows10.0.19041.0
```

## Uruchamianie

**Windows:**
```bash
dotnet run -f net10.0-windows10.0.19041.0
```

**Android (wymaga emulatora lub urządzenia):**
```bash
dotnet run -f net10.0-android
```

## Struktura projektu

```
SDC.CRM.Mobile/
├── Presentation/           # Warstwa prezentacji (MVVM)
│   ├── Views/              # Strony XAML
│   ├── ViewModels/         # ViewModele
│   ├── Navigation/         # Nawigacja
│   ├── Controls/           # Własne kontrolki
│   └── Converters/         # Konwertery wartości
├── Application/            # Warstwa aplikacji
│   ├── UseCases/           # Przypadki użycia
│   ├── Interfaces/         # Interfejsy
│   ├── DTOs/               # Obiekty transferu danych
│   └── Validation/         # Walidacja
├── Domain/                 # Warstwa domenowa (mobile)
│   ├── Models/             # Modele domenowe
│   └── ValueObjects/       # Value Objects
├── Infrastructure/         # Warstwa infrastruktury
│   ├── Api/                # Klienci API
│   ├── Auth/               # Autoryzacja i tokeny
│   ├── Storage/            # Lokalne przechowywanie
│   └── Connectivity/       # Sprawdzanie połączenia
├── Platforms/              # Kod specyficzny dla platform
│   ├── Android/
│   └── Windows/
└── Resources/              # Zasoby (ikony, czcionki, style)
```

## Architektura

Aplikacja wykorzystuje wzorzec **MVVM** z biblioteką **CommunityToolkit.Mvvm**.

### Kluczowe komponenty

- **ITokenStorage** - Bezpieczne przechowywanie tokenów w SecureStorage
- **IConnectivityService** - Monitoring stanu połączenia sieciowego
- **INavigationService** - Abstrakcja nawigacji Shell

## Konfiguracja DI

Wszystkie zależności są rejestrowane w `MauiProgram.cs`:

```csharp
// Infrastructure
builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

// Pages & ViewModels
builder.Services.AddTransient<MainPage>();
builder.Services.AddTransient<MainPageViewModel>();
```

## Język domeny

Aplikacja używa wspólnego języka domeny CRM:

- `Customer` - Klient
- `ContactPerson` - Osoba kontaktowa
- `Lead` - Lead (potencjalny klient)
- `Opportunity` - Szansa sprzedażowa
- `SalesActivity` - Aktywność sprzedażowa
- `SalesNote` - Notatka sprzedażowa
- `FollowUp` - Działanie follow-up
- `SalesOrder` - Zamówienie sprzedażowe
- `BackofficeOrderCase` - Sprawa backoffice

## Testy

```bash
dotnet test
```

Reguły testowania znajdują się w `.github/instructions/99-tdd-tunit-nsubstitute.instructions.md`.

