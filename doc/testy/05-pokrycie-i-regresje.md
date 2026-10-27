# 5. Pokrycie, znane regresje i wyniki odbioru

Uzupełnienie PR #5 obejmuje wyłącznie testy, ich hosty, narzędzia i dokumentację. Kod aplikacji pozostaje bez zmian.
Scenariusz opisany nie oznacza scenariusza wykonanego. SKIP, Explicit i częściowy smoke nie są PASS.

## Uruchamianie

Z katalogu głównego:

```powershell
dotnet test --solution Backend/SDC.CRM.Backend.slnf -c Release
dotnet test --project Frontend/Mobile/tests/SDC.CRM.Mobile.Tests/SDC.CRM.Mobile.Tests.csproj -c Release
node doc/testy/api/tests/test-smoke-runner.mjs
./doc/crm_ddd_ai_agent_package/tools/validate-docs.ps1
# z Frontend/Web:
npm run test:ci
```

Pełny smoke na działającym lokalnym środowisku:

```powershell
./doc/testy/api/test-api.ps1 -SalespersonToken $handlowiec -OtherSalesToken $kierownik -BackofficeToken $backoffice -RequireAllTokens
```

Tokeny muszą pochodzić od trzech różnych użytkowników. Wiele okien incognito tej samej przeglądarki nie izoluje sesji SSO; użyj różnych profili/przeglądarek. Skrypt sprawdza różne sub przed żądaniami. Dekodowanie payload nie potwierdza podpisu — robi to API.

Kody smoke: 0 — cały zestaw smoke wykonany, 1 — błąd, 2 — niekompletny zakres. Bez tokenów sprawdzane jest tylko osiem przypadków. HTTP Client zawiera także API-03/05/06/07/17/21 i dwukierunkową izolację API-30…33; uruchamiaj cały plik od API-01, by wyzerować jego globalny stan.

## Prawdziwy PostgreSQL

Testy tworzą osobną bazę sdc_crm_tests_<losowy identyfikator> dla każdego przypadku, migrują ją i usuwają w finally/DisposeAsync. Nie czyszczą appdb. Konto potrzebuje uprawnienia CREATE DATABASE. Nigdy nie kieruj ich na produkcyjny serwer.

```powershell
$env:SDC_CRM_TEST_POSTGRES = 'Host=localhost;Port=5432;Database=postgres;Username=app;Password=app'
dotnet test --project Backend/tests/SDC.CRM.Api.IntegrationTests/SDC.CRM.Api.IntegrationTests.csproj -c Release --treenode-filter "/*/*/PostgresLeadRepositoryTests/*" --minimum-expected-tests 3
```

Brak zmiennej oznacza błąd, bez fallbacku na bazę aplikacji. Jawny filtr uruchamia klasę oznaczoną Explicit. CI test-assets ma dedykowany tymczasowy serwis PostgreSQL. Te testy sprawdzają migracje, mapowanie/normalizację, trwałość między DbContextami, izolację w obie strony i pełną kolejność. Nie zastępują restartu realnego procesu API (API-54).

## Znane błędy — jawne testy czerwone

Te asercje opisują pożądane niezmienniki. Nie zmieniono oczekiwań na błędne zachowanie i nie naprawiano kodu aplikacji w PR testowym. Klasy/metody mają Explicit i kategorię KnownRegression; nie są zaliczane jako zielone pokrycie zwykłego CI. Po poprawie aplikacji należy usunąć Explicit i przenieść je do obowiązkowej regresji.

```powershell
# Cztery regresje Mobile: spóźniony refresh, spóźniona lista, binding refresh, równoległe refresh.
dotnet test --project Frontend/Mobile/tests/SDC.CRM.Mobile.Tests/SDC.CRM.Mobile.Tests.csproj -c Release --treenode-filter "/*/*/KnownSessionRegressionTests/*" --minimum-expected-tests 4
# Pięć kolumn: N+1 powinno dawać 400 bez zapisu.
dotnet test --project Backend/tests/SDC.CRM.Api.IntegrationTests/SDC.CRM.Api.IntegrationTests.csproj -c Release --treenode-filter "/*/*/KnownValidationRegressionTests/*" --minimum-expected-tests 5
# Poprawnie podpisany JWT bez sub powinien być odrzucony.
dotnet test --project Backend/tests/SDC.CRM.Api.IntegrationTests/SDC.CRM.Api.IntegrationTests.csproj -c Release --treenode-filter "/*/*/JwtValidationTests/Jwt__When_subject_is_missing__Should_reject_read_and_write" --minimum-expected-tests 1
```

W obecnym kodzie spodziewany jest FAIL tych regresji. Nie ignoruj kodu wyjścia i nie traktuj ich pominięcia jako usunięcia błędów. Brak sub został ujęty jako pożądana odmowa dostępu, nie jako działająca już ochrona.

## Mapa pokrycia

| Luka z analizy | Dodane automaty | Dodatkowy manual / granica |
|---|---|---|
| Logout podczas refresh/GET | Explicit KnownSessionRegressionTests z TaskCompletionSource | MOB-16/17, także A→B na urządzeniu |
| Cykl OIDC, refresh, awaria SSO | AuthService spec: stan po zdarzeniach, odnowiony token, role, discovery error; istniejące unity Mobile; jawna regresja równoległego refresh | WEB-20/24/25, MOB-18/19; prawdziwy provider/rotacja i callback nie są mockowym testem fasady |
| Prawdziwa baza i trwałość | PostgresLeadRepositoryTests, osobny job CI | API-54/56: restart procesu, pełne HTTP i awaria bazy |
| Izolacja | Smoke i HTTP: własne dane A/B oraz brak cudzych w obie strony; PostgreSQL | WEB-10 w niezależnych profilach, MOB-17 |
| Granice/normalizacja | Domain/Email, formularz N−1/N, API N−1/N, Explicit API N+1 | WEB-23/API-55/60: pełny kontrakt i błędy UI |
| Rzeczywista walidacja JWT/role | Podpisane tokeny: zły podpis/issuer/audience/exp/nbf/brak roli; pięć ról × GET/POST; Explicit brak sub | API-51/52/53/59 z prawdziwym SSO |
| Formularz i opóźnione odpowiedzi | LeadRegister: walidacja, payload, pending, błędy i retry; LeadList loading/retry | WEB-19/21/22: nawigacja/deep link, niepewny zapis i idempotencja |
| Platforma i magazyn Mobile | Stan listy po offline oraz retry, Explicit binding refresh | MOB-20/21/22: SecureStorage, XAML, timeout/JSON, resume |
| Reguły domeny/zdarzenia | LeadBehaviorTests: właściciel, trim/null, payload zdarzeń, idempotencja Qualify, Reject | Brak endpointów Qualify/Reject — testowane tylko w domenie |
| CORS | OPTIONS dozwolony/obcy origin | API-57: wymuszenie w przeglądarce i odczyt korelacji na błędach |
| Sam runner i CI | Fault injection smoke, parser PowerShell, walidator dokumentacji, filtry workflow | HTTP Client wymaga wykonania w Riderze; brak wbudowanego runnera JetBrains w CI |
| Obserwowalność i wydajność | Dotychczasowe testy korelacji | API-21 i API-58; sprawdź logi/trace, brak danych wrażliwych, pomiary dla około 20 osób |

## Rejestr wykonania

Dla każdego manuala zapisz: commit, platformę/przeglądarkę, ID scenariusza, PASS/FAIL/SKIP, oczekiwanie, wynik, korelację i zgłoszenie błędu. Bez tokenów, kodów autoryzacji, haseł i rzeczywistych danych klientów. Pustej tabeli nie traktuj jako testu zakończonego powodzeniem.

CI test-assets uruchamia walidator dokumentacji, parser skryptów, samotest smoke i prawdziwą bazę. Zmiany doc/testy uruchamiają także istniejące workflow backend/web/mobile. Znane czerwone regresje mają jawne komendy powyżej; stanowią listę blokad odbioru, nie znikają dzięki zielonemu CI podstawowemu.
