# Testy ręczne i dymne SDC-CRM

Scenariusze do przetestowania aktualnie zaimplementowanego zakresu (logowanie SSO, uprawnienia według ról,
rejestracja leada i lista „Moje leady”) w każdym kliencie i bezpośrednio w API.

| Plik | Zawartość |
|---|---|
| [01-uruchomienie-srodowiska.md](01-uruchomienie-srodowiska.md) | Co i w jakiej kolejności uruchomić: SSO, baza, backend, web, mobile (Android i Windows); porty, użytkownicy testowi, rozwiązywanie problemów |
| [02-scenariusze-web.md](02-scenariusze-web.md) | Scenariusze WEB-01…WEB-18 dla aplikacji Angular |
| [03-scenariusze-mobile.md](03-scenariusze-mobile.md) | Scenariusze MOB-01…MOB-15 dla aplikacji .NET MAUI (Android i Windows) |
| [04-testy-endpointow-api.md](04-testy-endpointow-api.md) | Katalog endpointów, kontrakty, pozyskanie tokena, scenariusze API-01…API-52 |
| [api/test-api.ps1](api/test-api.ps1) | Skrypt PowerShell wykonujący automatycznie scenariusze API |
| [api/crm-api.http](api/crm-api.http) | Te same scenariusze dla klienta HTTP w Riderze (z asercjami) |

## Zalecana kolejność

1. Uruchom środowisko i przejdź listę kontrolną z [01-uruchomienie-srodowiska.md](01-uruchomienie-srodowiska.md).
2. Uruchom testy automatyczne (szybka informacja, czy kod jest w dobrym stanie):

   ```powershell
   dotnet test --solution Backend/SDC.CRM.Backend.slnf -c Release
   dotnet test --project Frontend/Mobile/tests/SDC.CRM.Mobile.Tests/SDC.CRM.Mobile.Tests.csproj
   cd Frontend/Web; $env:CI = 'true'; npm run test:ci; cd ../..
   ```

3. Uruchom `./doc/testy/api/test-api.ps1` bez tokenów (API-01…API-08), a po zalogowaniu w web — z tokenami.
4. Przejdź scenariusze web, a następnie mobile (scenariusze mobile korzystają z leadów utworzonych w web lub API).

## Zakres poza testami

Szczegóły leada, klient, szanse sprzedaży, zamówienia i backoffice nie są jeszcze zaimplementowane
(plan: [../04-plan-realizacji-pbi.md](../04-plan-realizacji-pbi.md)). Przy dodawaniu kolejnych funkcji dopisuj tu
nowe scenariusze z kolejnymi identyfikatorami (WEB-xx, MOB-xx, API-xx).

