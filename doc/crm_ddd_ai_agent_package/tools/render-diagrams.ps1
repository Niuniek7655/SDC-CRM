<#
.SYNOPSIS
    Renderuje diagramy Mermaid pakietu DDD do plików PNG.

.DESCRIPTION
    Źródłem każdego diagramu jest blok ```mermaid poprzedzony znacznikiem <!-- diagram: NAZWA -->
    w pliku .md pod ai_readable/. Wynik trafia do diagrams/png/NAZWA.png, a skróty źródeł do
    diagrams/diagrams.manifest.json (validate-docs.ps1 wykrywa na tej podstawie nieaktualne PNG).

    Wymaga Node.js z npx. Używa @mermaid-js/mermaid-cli w przypiętej wersji. Jeżeli w systemie jest Chrome
    lub Edge (albo ustawiono PUPPETEER_EXECUTABLE_PATH), puppeteer nie pobiera własnej przeglądarki.

.PARAMETER Force
    Renderuje wszystkie diagramy, także te, których źródło się nie zmieniło.

.PARAMETER Name
    Renderuje tylko wskazane diagramy (nazwy bez rozszerzenia, np. 02_process_01_lead_pipeline).

.EXAMPLE
    ./doc/crm_ddd_ai_agent_package/tools/render-diagrams.ps1
#>
[CmdletBinding()]
param(
    [switch]$Force,
    [string[]]$Name
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'DiagramSources.ps1')

$mermaidCli = '@mermaid-js/mermaid-cli@12.0.0'
$packageRoot = Get-PackageRoot
$configPath = Join-Path $PSScriptRoot 'mermaid-config.json'
$pngDir = Join-Path $packageRoot 'diagrams/png'
$manifestPath = Join-Path $packageRoot 'diagrams/diagrams.manifest.json'

if (-not (Get-Command npx -ErrorAction SilentlyContinue)) {
    throw 'Nie znaleziono npx. Zainstaluj Node.js (wersja z Frontend/Web/.nvmrc).'
}

function Find-Browser {
    if ($env:PUPPETEER_EXECUTABLE_PATH -and (Test-Path $env:PUPPETEER_EXECUTABLE_PATH)) {
        return $env:PUPPETEER_EXECUTABLE_PATH
    }
    $candidates = @()
    if ($script:IsWindowsPlatform) {
        foreach ($base in @($env:ProgramFiles, ${env:ProgramFiles(x86)}, $env:LOCALAPPDATA)) {
            if ($base) {
                $candidates += Join-Path $base 'Google\Chrome\Application\chrome.exe'
                $candidates += Join-Path $base 'Microsoft\Edge\Application\msedge.exe'
            }
        }
    }
    else {
        foreach ($command in 'google-chrome', 'google-chrome-stable', 'chromium', 'chromium-browser', 'microsoft-edge') {
            $found = Get-Command $command -ErrorAction SilentlyContinue
            if ($found) { $candidates += $found.Source }
        }
        $candidates += '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'
    }
    return $candidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
}

$diagrams = @(Get-DiagramSources -PackageRoot $packageRoot -ConfigPath $configPath)
if ($diagrams.Count -eq 0) {
    throw 'Nie znaleziono żadnego diagramu (<!-- diagram: NAZWA --> + blok mermaid).'
}
$duplicates = @($diagrams | Group-Object Name | Where-Object { $_.Count -gt 1 })
if ($duplicates.Count -gt 0) {
    throw "Zduplikowane nazwy diagramów: $(($duplicates | ForEach-Object Name) -join ', ')"
}
if ($Name) {
    # "powershell -File" przekazuje listę "a,b" jako jeden napis.
    $Name = @($Name | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $unknown = @($Name | Where-Object { $diagrams.Name -notcontains $_ })
    if ($unknown.Count -gt 0) { throw "Nieznane diagramy: $($unknown -join ', ')" }
}

$hashes = @{}
if (Test-Path $manifestPath) {
    foreach ($entry in (Get-Content $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json).diagrams) {
        $hashes[$entry.name] = $entry.sha256
    }
}

$workDir = Join-Path ([System.IO.Path]::GetTempPath()) 'sdc-crm-diagrams'
New-Item -ItemType Directory -Force -Path $workDir, $pngDir | Out-Null

$puppeteerArgs = @()
$browser = Find-Browser
if ($browser) {
    $env:PUPPETEER_SKIP_DOWNLOAD = 'true'
    $puppeteerConfig = @{ executablePath = $browser }
    if (-not $script:IsWindowsPlatform) { $puppeteerConfig.args = @('--no-sandbox') }
    $puppeteerConfigPath = Join-Path $workDir 'puppeteer-config.json'
    Write-Utf8File -Path $puppeteerConfigPath -Content ($puppeteerConfig | ConvertTo-Json)
    $puppeteerArgs = @('-p', $puppeteerConfigPath)
    Write-Host "Przeglądarka: $browser"
}

$rendered = 0
foreach ($diagram in $diagrams) {
    $pngPath = Join-Path $pngDir "$($diagram.Name).png"
    $selected = (-not $Name) -or ($Name -contains $diagram.Name)
    $upToDate = (Test-Path $pngPath) -and ($hashes[$diagram.Name] -eq $diagram.Hash)
    if (-not $selected -or ($upToDate -and -not $Force)) { continue }

    $sourcePath = Join-Path $workDir "$($diagram.Name).mmd"
    Write-Utf8File -Path $sourcePath -Content ($diagram.Code + "`n")
    Write-Host "Renderowanie $($diagram.Name)  <-  $($diagram.Source)"
    & npx --yes $mermaidCli -i $sourcePath -o $pngPath -c $configPath -b white -s 2 -q @puppeteerArgs
    if ($LASTEXITCODE -ne 0) { throw "Nie udało się wyrenderować diagramu $($diagram.Name)." }
    $hashes[$diagram.Name] = $diagram.Hash
    $rendered++
}

# Katalog png/ zawiera wyłącznie pliki generowane - usuń PNG bez źródła.
$known = @($diagrams | ForEach-Object { "$($_.Name).png" })
foreach ($orphan in Get-ChildItem -Path $pngDir -Filter '*.png' | Where-Object { $known -notcontains $_.Name }) {
    Write-Host "Usuwanie PNG bez źródła: $($orphan.Name)"
    Remove-Item $orphan.FullName
}

# Manifest zapisywany ręcznie (a nie ConvertTo-Json), żeby format był identyczny w PowerShell 5.1 i 7.
$sorted = @($diagrams | Sort-Object Name)
$lines = @(
    '{',
    '  "comment": "Plik generowany przez tools/render-diagrams.ps1 - nie edytuj ręcznie.",',
    "  `"renderer`": `"$mermaidCli`",",
    '  "config": "tools/mermaid-config.json",',
    '  "diagrams": ['
)
for ($i = 0; $i -lt $sorted.Count; $i++) {
    $diagram = $sorted[$i]
    $hash = if ($hashes[$diagram.Name]) { "`"$($hashes[$diagram.Name])`"" } else { 'null' }
    $comma = if ($i -lt $sorted.Count - 1) { ',' } else { '' }
    $lines += "    { `"name`": `"$($diagram.Name)`", `"source`": `"$($diagram.Source)`", `"png`": `"diagrams/png/$($diagram.Name).png`", `"sha256`": $hash }$comma"
}
$lines += '  ]', '}'
Write-Utf8File -Path $manifestPath -Content (($lines -join "`n") + "`n")
Write-Host "Wyrenderowano: $rendered, diagramów łącznie: $($diagrams.Count)."


