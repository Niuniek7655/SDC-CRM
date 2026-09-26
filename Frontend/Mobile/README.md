# SDC CRM Mobile

Aplikacja mobilna .NET MAUI dla systemu SDC CRM.

## Obsługiwane platformy

- Android (API 21+)
- Windows (10.0.17763+)

iOS i macOS nie są obecnie budowane (`TargetFrameworks` w `SDC.CRM.Mobile.csproj`) - to otwarta decyzja biznesowa.

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

Polecenia uruchamiaj z katalogu głównego repozytorium.

```bash
dotnet workload restore Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj

# Android
dotnet build Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj -f net10.0-android

# Windows
dotnet build Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj -f net10.0-windows10.0.19041.0
```

## Uruchamianie

Wymagane działające SSO (`Integrations/Sso/README.md`) i API (`Backend/README.md`).

**Windows:**
```bash
dotnet run --project Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj -f net10.0-windows10.0.19041.0
```

**Android (emulator lub urządzenie)** - najpierw przekieruj porty SSO i API na `localhost` urządzenia
(wystawca tokenów to `http://localhost:5001/master`, więc aplikacja musi widzieć SSO pod `localhost`):
```bash
adb reverse tcp:5001 tcp:5001
adb reverse tcp:5080 tcp:5080
dotnet build Frontend/Mobile/src/SDC.CRM.Mobile/SDC.CRM.Mobile.csproj -t:Run -f net10.0-android
```

## Struktura projektów

```
Frontend/Mobile/
├── src/
│   ├── SDC.CRM.Mobile/              # aplikacja .NET MAUI (net10.0-android, net10.0-windows)
│   │   ├── Presentation/
│   │   │   ├── Views/               #   strony XAML (LoginPage; MainPage leży w katalogu głównym)
│   │   │   ├── Navigation/          #   ShellNavigationService (Shell)
│   │   │   └── Converters/          #   konwertery wartości (status połączenia, etykiety statusów)
│   │   ├── Infrastructure/
│   │   │   ├── Auth/                #   SecureTokenStorage (SecureStorage), WebAuthenticatorBrowser (przeglądarka systemowa)
│   │   │   └── Connectivity/        #   ConnectivityService (MAUI Connectivity)
│   │   ├── Platforms/               #   kod specyficzny dla Androida i Windows
│   │   ├── Resources/               #   ikony, czcionki, style
│   │   ├── AppShell.xaml            #   trasy Shell (nazwy z AppRoutes)
│   │   └── MauiProgram.cs           #   rejestracja zależności (DI)
│   └── SDC.CRM.Mobile.Core/         # biblioteka net10.0 - logika niezależna od platformy (testowalna)
│       ├── Presentation/
│       │   ├── ViewModels/          #   BaseViewModel, LoginViewModel, MainPageViewModel
│       │   ├── Navigation/          #   INavigationService, AppRoutes
│       │   └── Formatting/          #   LeadStatusLabels (polskie etykiety statusów)
│       └── Infrastructure/
│           ├── Api/                 #   ICrmApiClient, CrmApiClient, AuthHeaderHandler, kontrakty API
│           ├── Auth/                #   IAuthService, OidcAuthService, IOidcSessionClient, DuendeOidcSessionClient, ITokenStorage
│           ├── Configuration/       #   AppConfig (adresy SSO i API)
│           └── Connectivity/        #   IConnectivityService
└── tests/
    └── SDC.CRM.Mobile.Tests/        # testy TUnit + NSubstitute dla SDC.CRM.Mobile.Core
```

Logika (ViewModele, klient API, reguły sesji) jest w `SDC.CRM.Mobile.Core`, bo projekt MAUI celuje wyłącznie
w platformy (`net10.0-android`, `net10.0-windows`) i nie da się go uruchomić w zwykłym teście jednostkowym.
W aplikacji zostają tylko adaptery MAUI, widoki i rejestracja zależności.

## Architektura

Aplikacja wykorzystuje wzorzec **MVVM** z biblioteką **CommunityToolkit.Mvvm** (`[ObservableProperty]` na
właściwościach `partial`, `[RelayCommand]`). Strony wywołują w code-behind tylko `AppearingCommand` /
`DisappearingCommand`; cała logika jest w ViewModelach.

### Sesja i uprawnienia

- **Logowanie** - Authorization Code + PKCE w przeglądarce systemowej (`DuendeOidcSessionClient` +
  `WebAuthenticatorBrowser`); tokeny (access, refresh, id) w `SecureStorage`.
- **Odświeżanie** - `OidcAuthService` odświeża token na minutę przed wygaśnięciem; nieudane odświeżenie kończy sesję.
- **Wylogowanie** - najpierw czyści lokalne tokeny, potem kończy sesję w SSO (`end_session` z `id_token_hint`,
  powrót na `com.sdc.crm.mobile://signout`). Wymaga zarejestrowania tego adresu w kliencie `sdc-crm-mobile`
  (patrz `Integrations/Sso/README.md`).
- **HTTP 401** z API - sesja jest zapominana lokalnie (bez okna przeglądarki) i użytkownik wraca do logowania.
- **HTTP 403** z API - użytkownik pozostaje zalogowany, a ekran pokazuje komunikat o braku uprawnień
  (np. rola `BackofficeUser` na liście leadów).

## Konfiguracja DI

Wszystkie zależności są rejestrowane w `MauiProgram.cs`:

```csharp
// Infrastructure
builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

// Authentication (OIDC via system browser)
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IBrowser, WebAuthenticatorBrowser>();
builder.Services.AddSingleton<IOidcSessionClient, DuendeOidcSessionClient>();
builder.Services.AddSingleton<IAuthService, OidcAuthService>();
builder.Services.AddTransient<AuthHeaderHandler>();

// Typed API client with bearer-token attachment
builder.Services.AddHttpClient<ICrmApiClient, CrmApiClient>(client => client.BaseAddress = new Uri(AppConfig.ApiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();

// Pages & ViewModels
builder.Services.AddTransient<LoginPage>();
builder.Services.AddTransient<LoginViewModel>();
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
dotnet test --project Frontend/Mobile/tests/SDC.CRM.Mobile.Tests/SDC.CRM.Mobile.Tests.csproj
```

Testy (TUnit + NSubstitute, `net10.0`) sprawdzają `SDC.CRM.Mobile.Core` i nie wymagają workloadów MAUI,
emulatora, SSO ani sieci. Reguły testowania: `.github/instructions/99-tdd-tunit-nsubstitute.instructions.md`.

**Atrapy (`TestDoubles/`)**

- `StubHttpMessageHandler` - zastępuje sieć: zwraca przygotowaną odpowiedź HTTP (status, JSON) i zapamiętuje
  wysłane żądania, więc test sprawdza adres, metodę i nagłówki bez serwera.
- `InMemoryTokenStorage` - magazyn tokenów w pamięci (w aplikacji: `SecureStorage`); test czyta jego stan po operacji.
- `FixedTimeProvider` - stały „teraz” do reguł wygasania tokenów, niezależny od zegara systemowego.
- Porty `IAuthService`, `ICrmApiClient`, `INavigationService`, `IConnectivityService`, `IOidcSessionClient` -
  substytuty NSubstitute; zmianę łączności test wywołuje przez `Raise.Event`, a nawigację i wylogowanie
  sprawdza przez `Received`/`DidNotReceive`.

**Klasy testów**

| Klasa | Co sprawdza |
| --- | --- |
| `Infrastructure/Api/CrmApiClientTests` | `GET api/leads/mine` i mapowanie JSON na `LeadSummaryDto`; 401 → `CrmUnauthorizedException`; 403 → `CrmForbiddenException` (nie 401); `POST api/leads` zwraca nowe `id`; 400 → `CrmApiException` z treścią `ProblemDetails` |
| `Infrastructure/Api/AuthHeaderHandlerTests` | handler dokleja `Authorization: Bearer <token>` z bieżącej sesji; bez sesji żądanie idzie bez nagłówka |
| `Infrastructure/Auth/OidcAuthServiceTests` | po zalogowaniu zapisuje access, refresh i id token; błąd dostawcy tożsamości → nic nie jest zapisywane; ważny token zwracany bez odświeżania; token wygasający w ciągu minuty jest odświeżany (id token zostaje zachowany); odrzucone odświeżenie kończy sesję; wylogowanie czyści lokalne tokeny i kończy sesję SSO z `id_token_hint`; błąd `end_session` nie blokuje lokalnego wylogowania; bez id tokena tylko czyszczenie lokalne; `ClearSessionAsync` nie kontaktuje się z SSO; `GetUserAsync` odczytuje nazwę i role z tokena (niepodpisany JWT budowany w teście - podpis weryfikuje API) |
| `Presentation/ViewModels/LoginViewModelTests` | istniejąca sesja → od razu ekran główny; udane logowanie → ekran główny; nieudane → komunikat błędu i brak nawigacji |
| `Presentation/ViewModels/MainPageViewModelTests` | brak sesji → ekran logowania bez wywołania API; zalogowany → powitanie i lista leadów; offline → komunikat bez wywołania API; 401 → lokalne wyczyszczenie sesji i ekran logowania (bez okna przeglądarki); 403 → komunikat „Brak uprawnień…”, użytkownik pozostaje zalogowany; łączność śledzona tylko, gdy strona jest widoczna (jedna subskrypcja przy wielokrotnym pojawieniu się, wypisanie przy ukryciu); wylogowanie czyści listę i przechodzi do logowania |
| `Presentation/Formatting/LeadStatusLabelsTests` | kody statusów → polskie etykiety ze słownika (test sparametryzowany `[Arguments]`); nieznany kod pokazywany bez zmian; brak statusu → pusty tekst |

Testy nie obejmują adapterów MAUI (`SecureStorage`, `WebAuthenticator`, Shell, `Connectivity`) ani przepływu
w prawdziwej przeglądarce - to weryfikuje się ręcznie na urządzeniu z lokalnie uruchomionym SSO i API.

