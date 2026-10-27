# Testy ręczne i dymne SDC-CRM

Scenariusze do przetestowania aktualnie zaimplementowanego zakresu (logowanie SSO, uprawnienia według ról,
rejestracja leada i lista „Moje leady”) w każdym kliencie i bezpośrednio w API.

| Plik | Zawartość |
|---|---|
| [01-uruchomienie-srodowiska.md](01-uruchomienie-srodowiska.md) | Co i w jakiej kolejności uruchomić: SSO, baza, backend, web, mobile (Android i Windows); porty, użytkownicy testowi, rozwiązywanie problemów |
| [02-scenariusze-web.md](02-scenariusze-web.md) | Scenariusze WEB-01…WEB-25 dla aplikacji Angular |
| [03-scenariusze-mobile.md](03-scenariusze-mobile.md) | Scenariusze MOB-01…MOB-22 dla aplikacji .NET MAUI (Android i Windows) |
| [04-testy-endpointow-api.md](04-testy-endpointow-api.md) | Katalog endpointów, kontrakty, pozyskanie tokena, scenariusze API-01…API-60 |
| [05-pokrycie-i-regresje.md](05-pokrycie-i-regresje.md) | Mapa pokrycia, testy prawdziwego PostgreSQL, jawne czerwone regresje, CI i rejestr wykonania |
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

3. Uruchom `./doc/testy/api/test-api.ps1` bez tokenów (API-01…API-08, wynik częściowy/exit 2), a po zalogowaniu trzech osób w niezależnych profilach — z tokenami i `-RequireAllTokens`.
4. Wykonaj dodatkowe automaty i znane regresje według [mapy pokrycia](05-pokrycie-i-regresje.md). Nie licz Explicit/SKIP jako PASS.
5. Przejdź scenariusze web, a następnie mobile (scenariusze mobile korzystają z leadów utworzonych w web lub API).

## Zakres poza testami

Szczegóły leada, klient, szanse sprzedaży, zamówienia i backoffice nie są jeszcze zaimplementowane
(plan: [../04-plan-realizacji-pbi.md](../04-plan-realizacji-pbi.md)). Przy dodawaniu kolejnych funkcji dopisuj tu
nowe scenariusze z kolejnymi identyfikatorami (WEB-xx, MOB-xx, API-xx).

