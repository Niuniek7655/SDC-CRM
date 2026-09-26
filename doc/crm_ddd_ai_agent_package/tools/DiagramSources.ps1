# Wspólne funkcje skryptów render-diagrams.ps1 i validate-docs.ps1 (Windows PowerShell 5.1 i PowerShell 7+).

$script:Utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$script:IsWindowsPlatform = [System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT

# Katalog pakietu DDD (nadrzędny względem tools/).
function Get-PackageRoot {
    return (Split-Path -Parent $PSScriptRoot)
}

# Ujednolicone końce linii - skróty nie zależą od core.autocrlf.
function ConvertTo-NormalizedText {
    param([string]$Text)
    return (($Text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd()
}

function Get-Sha256Hex {
    param([string]$Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Text))
        return (($hash | ForEach-Object { $_.ToString('x2') }) -join '')
    }
    finally {
        $sha.Dispose()
    }
}

function Get-RelativePath {
    param([string]$Root, [string]$Path)
    $rootFull = [System.IO.Path]::GetFullPath($Root).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    $full = [System.IO.Path]::GetFullPath($Path)
    if ($full.StartsWith($rootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($rootFull.Length).Replace('\', '/')
    }
    # Plik poza katalogiem bazowym (np. doc/04-*.md względem pakietu) - ścieżka z "../".
    $relative = (New-Object System.Uri($rootFull)).MakeRelativeUri((New-Object System.Uri($full))).ToString()
    return [System.Uri]::UnescapeDataString($relative).Replace('\', '/')
}

function Write-Utf8File {
    param([string]$Path, [string]$Content)
    [System.IO.File]::WriteAllText($Path, $Content, $script:Utf8NoBom)
}

# Diagramy: blok ```mermaid poprzedzony znacznikiem <!-- diagram: NAZWA --> w plikach .md pod ai_readable/
# oraz w plikach .md bezpośrednio w katalogu doc/ (np. plan realizacji PBI).
# Skrót obejmuje kod diagramu i konfigurację Mermaid, więc zmiana któregokolwiek oznacza nieaktualny PNG.
function Get-DiagramSources {
    param([string]$PackageRoot, [string]$ConfigPath)

    $config = ConvertTo-NormalizedText ([System.IO.File]::ReadAllText($ConfigPath))
    $pattern = '<!--\s*diagram:\s*(?<name>[A-Za-z0-9_\-]+)\s*-->[ \t]*\n```mermaid[ \t]*\n(?<code>.*?)\n```'
    $files = @(Get-ChildItem -Path (Join-Path $PackageRoot 'ai_readable') -Recurse -Filter '*.md') +
        @(Get-ChildItem -Path (Split-Path -Parent $PackageRoot) -Filter '*.md' -File) | Sort-Object FullName

    foreach ($file in $files) {
        $text = ConvertTo-NormalizedText ([System.IO.File]::ReadAllText($file.FullName))
        foreach ($match in [regex]::Matches($text, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
            $code = $match.Groups['code'].Value
            [pscustomobject]@{
                Name   = $match.Groups['name'].Value
                Source = Get-RelativePath -Root $PackageRoot -Path $file.FullName
                Code   = $code
                Hash   = Get-Sha256Hex ($code + "`n--- mermaid-config ---`n" + $config)
            }
        }
    }
}

