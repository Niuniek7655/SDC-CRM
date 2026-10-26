<#
.SYNOPSIS
    Testy dymne (smoke) endpointów SDC-CRM API na działającym lokalnym środowisku.

.DESCRIPTION
    Wykonuje scenariusze API-xx opisane w doc/testy/04-testy-endpointow-api.md i wypisuje wynik każdego z nich.

    - Bez tokenów: uwierzytelnianie (401), nagłówek X-Correlation-ID i dokument OpenAPI.
    - Z -SalespersonToken: lista, rejestracja, reguły domenowe i walidacja żądań, właściciel leada z tokena.
    - Z -OtherSalesToken: izolacja danych między użytkownikami sprzedaży.
    - Z -BackofficeToken: odmowa dostępu (403) dla ról spoza sprzedaży.

    Token dostępowy skopiuj z aplikacji web po zalogowaniu danym użytkownikiem (konsola DevTools):
        copy(sessionStorage.getItem('access_token'))

    Skrypt tworzy leady z prefiksem "[SMOKE]" (API nie ma usuwania leadów).
    Kod wyjścia 1 oznacza co najmniej jeden niezgodny wynik.

.EXAMPLE
    ./doc/testy/api/test-api.ps1

.EXAMPLE
    ./doc/testy/api/test-api.ps1 -SalespersonToken $handlowiec -OtherSalesToken $kierownik -BackofficeToken $backoffice
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5080',

    # Token użytkownika z rolą Salesperson (np. handlowiec).
    [string]$SalespersonToken,

    # Token innego użytkownika sprzedaży (np. kierownik.sprzedazy) - test izolacji danych.
    [string]$OtherSalesToken,

    # Token użytkownika z rolą BackofficeUser lub BackofficeManager (np. backoffice).
    [string]$BackofficeToken
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http

$handler = New-Object System.Net.Http.HttpClientHandler
$handler.AllowAutoRedirect = $false
$client = New-Object System.Net.Http.HttpClient($handler)
$client.Timeout = [TimeSpan]::FromSeconds(15)
$BaseUrl = $BaseUrl.TrimEnd('/')
$runId = [Guid]::NewGuid().ToString('N').Substring(0, 8)

$script:Results = New-Object System.Collections.Generic.List[object]

function Invoke-Api {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [string]$Token,
        [object]$Body,
        [string]$RawBody,
        [hashtable]$Headers = @{}
    )

    $request = New-Object System.Net.Http.HttpRequestMessage((New-Object System.Net.Http.HttpMethod($Method)), "$BaseUrl$Path")
    if ($Token) {
        $request.Headers.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue('Bearer', $Token)
    }
    foreach ($key in $Headers.Keys) {
        [void]$request.Headers.TryAddWithoutValidation($key, [string]$Headers[$key])
    }

    $json = $null
    if ($null -ne $Body) { $json = $Body | ConvertTo-Json -Depth 5 -Compress }
    elseif ($RawBody) { $json = $RawBody }
    if ($null -ne $json) {
        $request.Content = New-Object System.Net.Http.StringContent($json, [System.Text.Encoding]::UTF8, 'application/json')
    }

    try {
        $response = $client.SendAsync($request).GetAwaiter().GetResult()
    }
    catch {
        throw "Brak połączenia z $BaseUrl - czy API działa? ($($_.Exception.GetBaseException().Message))"
    }

    $text = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    $values = $null
    $correlationId = $null
    if ($response.Headers.TryGetValues('X-Correlation-ID', [ref]$values)) { $correlationId = @($values)[0] }

    $data = $null
    if ($text) { try { $data = $text | ConvertFrom-Json } catch { $data = $null } }

    [pscustomobject]@{
        Status        = [int]$response.StatusCode
        Text          = $text
        Data          = $data
        CorrelationId = $correlationId
        Location      = $response.Headers.Location
    }
}

function Add-Result {
    param([string]$Id, [string]$Name, [bool]$Passed, [string]$Details)
    $script:Results.Add([pscustomobject]@{ Id = $Id; Name = $Name; Result = $(if ($Passed) { 'OK' } else { 'BŁĄD' }); Details = $Details })
    $color = if ($Passed) { 'Green' } else { 'Red' }
    $mark = if ($Passed) { 'OK  ' } else { 'BŁĄD' }
    Write-Host ("[{0}] {1,-7} {2}" -f $mark, $Id, $Name) -ForegroundColor $color
    if (-not $Passed -and $Details) { Write-Host "         $Details" -ForegroundColor DarkRed }
}

function Skip-Group {
    param([string]$Ids, [string]$Reason)
    $script:Results.Add([pscustomobject]@{ Id = $Ids; Name = $Reason; Result = 'POMINIĘTO'; Details = '' })
    Write-Host ("[----] {0,-7} pominięto: {1}" -f $Ids, $Reason) -ForegroundColor DarkYellow
}

function Test-Status {
    param([string]$Id, [string]$Name, [object]$Response, [int]$Expected, [scriptblock]$Extra)
    $passed = $Response.Status -eq $Expected
    $details = "oczekiwano HTTP $Expected, otrzymano HTTP $($Response.Status) $($Response.Text)"
    if ($passed -and $Extra) {
        $message = & $Extra $Response
        if ($message) { $passed = $false; $details = $message }
    }
    Add-Result -Id $Id -Name $Name -Passed $passed -Details $details
}

function New-LeadBody {
    param([string]$CompanyName = "[SMOKE] Acme $runId", [string]$ContactName = 'Jan Kowalski', [string]$ContactEmail = 'jan.kowalski@acme.test')
    @{ companyName = $CompanyName; contactName = $ContactName; contactEmail = $ContactEmail; contactPhone = '+48 600 100 200'; source = 'Targi' }
}

Write-Host "SDC-CRM API smoke tests - $BaseUrl (przebieg $runId)" -ForegroundColor Cyan

# ---------------------------------------------------------------- bez tokena
Test-Status 'API-01' 'GET /api/leads/mine bez tokena -> 401' (Invoke-Api GET '/api/leads/mine') 401
Test-Status 'API-02' 'POST /api/leads bez tokena -> 401' (Invoke-Api POST '/api/leads' -Body (New-LeadBody)) 401
Test-Status 'API-03' 'GET /api/leads/mine z nieprawidłowym tokenem -> 401' (Invoke-Api GET '/api/leads/mine' -Token 'not-a-valid.jwt.token') 401

$sentId = "smoke-$runId"
Test-Status 'API-04' 'Bezpieczny X-Correlation-ID wraca w odpowiedzi' (Invoke-Api GET '/api/leads/mine' -Headers @{ 'X-Correlation-ID' = $sentId }) 401 {
    param($r) if ($r.CorrelationId -ne $sentId) { "oczekiwano X-Correlation-ID '$sentId', otrzymano '$($r.CorrelationId)'" }
}

$unsafe = '<b>x</b>'
Test-Status 'API-05' 'Niebezpieczny X-Correlation-ID jest zastępowany nowym' (Invoke-Api GET '/api/leads/mine' -Headers @{ 'X-Correlation-ID' = $unsafe }) 401 {
    param($r) if (-not $r.CorrelationId -or $r.CorrelationId -eq $unsafe) { "oczekiwano nowego identyfikatora, otrzymano '$($r.CorrelationId)'" }
}

$tooLong = 'a' * 65
Test-Status 'API-06' 'X-Correlation-ID dłuższy niż 64 znaki jest zastępowany' (Invoke-Api GET '/api/leads/mine' -Headers @{ 'X-Correlation-ID' = $tooLong }) 401 {
    param($r) if (-not $r.CorrelationId -or $r.CorrelationId -eq $tooLong) { "oczekiwano nowego identyfikatora, otrzymano '$($r.CorrelationId)'" }
}

Test-Status 'API-07' 'Brak X-Correlation-ID -> identyfikator nadany przez API' (Invoke-Api GET '/api/leads/mine') 401 {
    param($r) if (-not $r.CorrelationId) { 'brak nagłówka X-Correlation-ID w odpowiedzi' }
}

Test-Status 'API-08' 'GET /openapi/v1.json (Development) opisuje /api/leads' (Invoke-Api GET '/openapi/v1.json') 200 {
    param($r) if ($r.Text -notmatch '/api/leads/mine' -or $r.Text -notmatch '"/api/leads"') { 'dokument OpenAPI nie zawiera ścieżek /api/leads i /api/leads/mine' }
}

# ---------------------------------------------------------------- handlowiec
$createdIds = @()
if (-not $SalespersonToken) {
    Skip-Group 'API-10..21' 'brak -SalespersonToken'
}
else {
    Test-Status 'API-10' 'GET /api/leads/mine -> 200 i tablica JSON' (Invoke-Api GET '/api/leads/mine' -Token $SalespersonToken) 200 {
        param($r) if (-not $r.Text.TrimStart().StartsWith('[')) { "oczekiwano tablicy JSON, otrzymano: $($r.Text)" }
    }

    $created = Invoke-Api POST '/api/leads' -Token $SalespersonToken -Body (New-LeadBody)
    Test-Status 'API-11' 'POST /api/leads (wszystkie pola) -> 201 z id i Location' $created 201 {
        param($r)
        $id = [Guid]::Empty
        if (-not $r.Data -or -not [Guid]::TryParse([string]$r.Data.id, [ref]$id) -or $id -eq [Guid]::Empty) { return "brak poprawnego id w odpowiedzi: $($r.Text)" }
        if (-not $r.Location) { return 'brak nagłówka Location' }
    }
    if ($created.Status -eq 201) { $createdIds += [string]$created.Data.id }

    $minimal = Invoke-Api POST '/api/leads' -Token $SalespersonToken -Body @{ companyName = "[SMOKE] Minimal $runId"; contactName = 'Anna Nowak'; contactEmail = 'anna@beta.test' }
    Test-Status 'API-12' 'POST /api/leads (tylko pola wymagane) -> 201' $minimal 201
    if ($minimal.Status -eq 201) { $createdIds += [string]$minimal.Data.id }

    $mine = Invoke-Api GET '/api/leads/mine' -Token $SalespersonToken
    Test-Status 'API-13' 'Nowe leady na liście, status New, najnowszy pierwszy' $mine 200 {
        param($r)
        $list = @($r.Data)
        foreach ($id in $createdIds) {
            $lead = $list | Where-Object { $_.id -eq $id }
            if (-not $lead) { return "lead $id nie występuje na liście właściciela" }
            if ($lead.status -ne 'New') { return "lead $id ma status '$($lead.status)', oczekiwano 'New'" }
        }
        if ($createdIds.Count -gt 0 -and $list[0].id -ne $createdIds[-1]) { return "pierwszy na liście jest $($list[0].id), oczekiwano ostatnio utworzonego $($createdIds[-1])" }
    }

    Test-Status 'API-14' 'Nazwa firmy z samych spacji -> 400 (reguła domenowa)' (Invoke-Api POST '/api/leads' -Token $SalespersonToken -Body (New-LeadBody -CompanyName '   ')) 400 {
        param($r) if ($r.Data.detail -ne 'A lead must have a company name.') { "nieoczekiwany detail: $($r.Text)" }
    }

    Test-Status 'API-15' 'Osoba kontaktowa z samych spacji -> 400 (reguła domenowa)' (Invoke-Api POST '/api/leads' -Token $SalespersonToken -Body (New-LeadBody -ContactName '   ')) 400 {
        param($r) if ($r.Data.detail -ne 'A lead must have a contact name.') { "nieoczekiwany detail: $($r.Text)" }
    }

    Test-Status 'API-16' 'E-mail bez @ -> 400 (value object Email)' (Invoke-Api POST '/api/leads' -Token $SalespersonToken -Body (New-LeadBody -ContactEmail 'jan.kowalski')) 400 {
        param($r) if ([string]$r.Data.detail -notlike '*is not valid*') { "nieoczekiwany detail: $($r.Text)" }
    }

    Test-Status 'API-17' 'Pusty e-mail -> 400' (Invoke-Api POST '/api/leads' -Token $SalespersonToken -Body (New-LeadBody -ContactEmail '')) 400
    Test-Status 'API-18' 'Brak pola companyName -> 400' (Invoke-Api POST '/api/leads' -Token $SalespersonToken -Body @{ contactName = 'Jan'; contactEmail = 'jan@acme.test' }) 400
    Test-Status 'API-19' 'Niepoprawny JSON -> 400' (Invoke-Api POST '/api/leads' -Token $SalespersonToken -RawBody '{"companyName":') 400

    $foreignOwner = [Guid]::NewGuid().ToString()
    $spoof = New-LeadBody -CompanyName "[SMOKE] Owner $runId"
    $spoof['assignedSalespersonId'] = $foreignOwner
    $spoofed = Invoke-Api POST '/api/leads' -Token $SalespersonToken -Body $spoof
    Test-Status 'API-20' 'assignedSalespersonId w treści jest ignorowany - właściciel z tokena' $spoofed 201 {
        param($r)
        $list = @((Invoke-Api GET '/api/leads/mine' -Token $SalespersonToken).Data)
        if (-not ($list | Where-Object { $_.id -eq $r.Data.id })) { return "lead $($r.Data.id) nie trafił na listę wywołującego" }
    }
    if ($spoofed.Status -eq 201) { $createdIds += [string]$spoofed.Data.id }

    $sentId = "smoke-auth-$runId"
    Test-Status 'API-21' 'X-Correlation-ID wraca także dla żądania uwierzytelnionego' (Invoke-Api GET '/api/leads/mine' -Token $SalespersonToken -Headers @{ 'X-Correlation-ID' = $sentId }) 200 {
        param($r) if ($r.CorrelationId -ne $sentId) { "oczekiwano '$sentId', otrzymano '$($r.CorrelationId)'" }
    }
    Write-Host "         Seq: CorrelationId = '$sentId' | Jaeger: usługa sdc-crm-api, tag correlation.id=$sentId" -ForegroundColor DarkGray
}

# ---------------------------------------------------------------- inny użytkownik sprzedaży
if (-not $OtherSalesToken) {
    Skip-Group 'API-30' 'brak -OtherSalesToken'
}
elseif ($createdIds.Count -eq 0) {
    Skip-Group 'API-30' 'brak leadów utworzonych tokenem -SalespersonToken w tym przebiegu'
}
else {
    Test-Status 'API-30' 'Inny użytkownik nie widzi leadów handlowca' (Invoke-Api GET '/api/leads/mine' -Token $OtherSalesToken) 200 {
        param($r)
        $visible = @(@($r.Data) | Where-Object { $createdIds -contains $_.id })
        if ($visible.Count -gt 0) { return "widoczne cudze leady: $(($visible | ForEach-Object { $_.id }) -join ', ')" }
    }
}

# ---------------------------------------------------------------- backoffice
if (-not $BackofficeToken) {
    Skip-Group 'API-40..41' 'brak -BackofficeToken'
}
else {
    Test-Status 'API-40' 'Backoffice: GET /api/leads/mine -> 403' (Invoke-Api GET '/api/leads/mine' -Token $BackofficeToken) 403
    Test-Status 'API-41' 'Backoffice: POST /api/leads -> 403' (Invoke-Api POST '/api/leads' -Token $BackofficeToken -Body (New-LeadBody -CompanyName "[SMOKE] Backoffice $runId")) 403
}

# ---------------------------------------------------------------- podsumowanie
$failed = @($script:Results | Where-Object { $_.Result -eq 'BŁĄD' })
$passedCount = @($script:Results | Where-Object { $_.Result -eq 'OK' }).Count
$skipped = @($script:Results | Where-Object { $_.Result -eq 'POMINIĘTO' }).Count
Write-Host ''
Write-Host "Wynik: OK $passedCount, BŁĄD $($failed.Count), pominięte grupy $skipped." -ForegroundColor $(if ($failed.Count -gt 0) { 'Red' } else { 'Green' })
if ($createdIds.Count -gt 0) { Write-Host "Utworzone leady: $($createdIds -join ', ')" -ForegroundColor DarkGray }

$client.Dispose()
if ($failed.Count -gt 0) { exit 1 }


