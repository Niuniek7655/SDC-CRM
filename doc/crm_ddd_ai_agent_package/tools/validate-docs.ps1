<#
.SYNOPSIS
    Sprawdza spójność dokumentacji domeny (doc/) i pakietu DDD.

.DESCRIPTION
    Kontrole:
    - poprawność plików JSON pakietu i wymaganych pól kroków procesów,
    - odwołania między modelem, katalogiem zdarzeń i procesami (konteksty, agregaty, komendy, zapytania, zdarzenia, kroki),
    - zgodność plików .md z .json (nazwy i stany: ✔ ◐ ○ ❓),
    - zgodność słownika (doc/01: konteksty, zdarzenia) i backlogu (doc/03: konteksty, agregaty, komendy, zdarzenia) z modelem,
    - spójność backlogu (tytuł, priorytet, status i zaznaczony checkbox story zgodne z widokiem zbiorczym),
    - plan realizacji (doc/04): każda story z backlogu dokładnie raz, nazwy/priorytety/statusy jak w backlogu,
      ciągła numeracja, zależności tylko od PBI wcześniejszych i o nie niższym priorytecie, opis każdego EN-xx i P-xx,
    - zakazane aliasy nazw (dozwolone tylko w ai_readable/naming_decisions.md) w doc/ oraz w instrukcjach AI
      (.github/copilot-instructions.md, .github/instructions, .github/prompts, .claude, .cursor/rules),
    - pytania Q-xx i decyzje T-xx zdefiniowane w ai_readable/open_questions.md (także w instrukcjach AI),
    - lokalne linki w plikach Markdown,
    - aktualność PNG względem źródeł Mermaid (diagrams/diagrams.manifest.json),
    - ścieżki kodu i foldery wskazane w JSON.
    Kod wyjścia 1 oznacza znalezione błędy.

.EXAMPLE
    ./doc/crm_ddd_ai_agent_package/tools/validate-docs.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'DiagramSources.ps1')

$packageRoot = Get-PackageRoot
$docRoot = Split-Path -Parent $packageRoot
$repoRoot = Split-Path -Parent $docRoot
$aiRoot = Join-Path $packageRoot 'ai_readable'
$script:Errors = New-Object System.Collections.Generic.List[string]

function Add-Error {
    param([string]$Message)
    $script:Errors.Add($Message)
}

function Read-Text {
    param([string]$Path)
    return [System.IO.File]::ReadAllText($Path)
}

function Read-PackageJson {
    param([string]$RelativePath)
    try {
        return (Read-Text (Join-Path $packageRoot $RelativePath) | ConvertFrom-Json)
    }
    catch {
        Add-Error "JSON $RelativePath - $($_.Exception.Message)"
        return $null
    }
}

function Get-Names {
    param($Items)
    return @($Items | Where-Object { $_ })
}

function Test-SameSet {
    param($Left, $Right)
    $a = (@(Get-Names $Left) | Sort-Object -Unique) -join '|'
    $b = (@(Get-Names $Right) | Sort-Object -Unique) -join '|'
    return $a -eq $b
}

$statusSymbols = [ordered]@{ implemented = '✔'; partial = '◐'; planned = '○'; decision_needed = '❓' }

# Wiersz tabeli Markdown zaczynający się od klucza musi zawierać symbol stanu i żadnego innego.
function Test-RowStatus {
    param([string]$Text, [string]$Key, $Status, [string]$Label)
    $row = [regex]::Match($Text, '(?m)^\|\s*' + [regex]::Escape($Key) + '\s*\|.*$')
    if (-not $row.Success) {
        Add-Error "$Label - brak wiersza w tabeli"
        return
    }
    foreach ($entry in $statusSymbols.GetEnumerator()) {
        $present = $row.Value.Contains($entry.Value)
        if ($entry.Key -eq $Status -and -not $present) { Add-Error "$Label - wiersz nie ma stanu $($entry.Value)" }
        if ($entry.Key -ne $Status -and $present) { Add-Error "$Label - wiersz ma stan $($entry.Value), oczekiwano $(if ($Status) { $statusSymbols[$Status] } else { '—' })" }
    }
}

$model = Read-PackageJson 'ai_readable/bounded_contexts_and_aggregates.json'
$catalog = Read-PackageJson 'ai_readable/events_catalog.json'
$processes = Read-PackageJson 'ai_readable/processes/processes.json'

if ($model -and $catalog -and $processes) {
    $validStatuses = @($model.status_values.PSObject.Properties.Name)

    # ---------------------------------------------------------------- model
    $contexts = @{}
    $aggregates = @{}
    $commands = @{}
    $queries = @{}
    $readModels = @{}
    $modelNames = New-Object System.Collections.Generic.List[string]
    $codePaths = New-Object System.Collections.Generic.List[string]

    function Test-Status {
        param($Status, [string]$Label)
        if ($validStatuses -notcontains $Status) { Add-Error "$Label - nieprawidłowy stan '$Status'" }
    }

    $srcRoot = Join-Path $repoRoot 'Backend/src'
    $srcProjects = @()
    if (Test-Path $srcRoot) { $srcProjects = @(Get-ChildItem $srcRoot -Directory) }
    else { Add-Error "Brak katalogu Backend/src - nie można sprawdzić folderów kodu z modelu" }

    foreach ($ctx in $model.bounded_contexts) {
        $contexts[$ctx.name] = $ctx
        foreach ($folder in $ctx.code_folders) {
            if ($srcProjects.Count -eq 0) { break }
            $exists = @($srcProjects | Where-Object { Test-Path (Join-Path $_.FullName $folder.name) }).Count -gt 0
            if ($exists -ne [bool]$folder.exists) { Add-Error "Model: $($ctx.name) - folder $($folder.name): exists=$($folder.exists), w kodzie: $exists" }
        }
        foreach ($agg in $ctx.aggregates) {
            $aggregates[$agg.name] = $ctx.name
            $modelNames.Add($agg.name)
            Test-Status $agg.status "Model: agregat $($agg.name)"
            if ($agg.code) { $codePaths.Add($agg.code) }
            foreach ($item in @($agg.entities) + @($agg.value_objects)) {
                if ($item) { $modelNames.Add($item.name); Test-Status $item.status "Model: $($agg.name).$($item.name)" }
            }
            $emitted = @()
            foreach ($cmd in $agg.commands) {
                if ($commands.ContainsKey($cmd.name)) { Add-Error "Model: komenda $($cmd.name) zdefiniowana więcej niż raz" }
                $commands[$cmd.name] = [pscustomobject]@{ Context = $ctx.name; Aggregate = $agg.name; Emits = @(Get-Names $cmd.emits); Status = $cmd.status }
                $modelNames.Add($cmd.name)
                Test-Status $cmd.status "Model: komenda $($cmd.name)"
                if ($cmd.code) { $codePaths.Add($cmd.code) }
                $emitted += @(Get-Names $cmd.emits)
            }
            if (-not (Test-SameSet $agg.events $emitted)) { Add-Error "Model: agregat $($agg.name) - pole events różni się od zdarzeń emitowanych przez komendy" }
        }
        foreach ($query in $ctx.queries) {
            $queries[$query.name] = $ctx.name
            $modelNames.Add($query.name)
            Test-Status $query.status "Model: zapytanie $($query.name)"
            if ($query.code) { $codePaths.Add($query.code) }
        }
        foreach ($readModel in $ctx.read_models) {
            $readModels[$readModel.name] = $ctx.name
            $modelNames.Add($readModel.name)
            Test-Status $readModel.status "Model: read model $($readModel.name)"
        }
        foreach ($component in $ctx.components) {
            Test-Status $component.status "Model: komponent $($component.name)"
            if ($component.code) { $codePaths.Add($component.code) }
        }
    }
    $supportingAreas = @{}
    foreach ($area in $model.supporting_areas) {
        $supportingAreas[$area.name] = $area
        Test-Status $area.status "Model: obszar $($area.name)"
    }

    # ---------------------------------------------------------------- katalog zdarzeń
    $domainEvents = @{}
    foreach ($event in $catalog.domain_events) {
        if ($domainEvents.ContainsKey($event.name)) { Add-Error "Katalog: zdarzenie $($event.name) występuje więcej niż raz" }
        $domainEvents[$event.name] = $event
        Test-Status $event.status "Katalog: $($event.name)"
        if ($event.code) { $codePaths.Add($event.code) }
        if (-not $contexts.ContainsKey($event.context)) { Add-Error "Katalog: $($event.name) - nieznany kontekst '$($event.context)'" }
        elseif ($aggregates[$event.aggregate] -ne $event.context) { Add-Error "Katalog: $($event.name) - agregat $($event.aggregate) nie należy do $($event.context)" }
        foreach ($raiser in $event.raised_by) {
            if (-not $commands.ContainsKey($raiser)) { Add-Error "Katalog: $($event.name) - nieznana komenda $raiser" }
            elseif ($commands[$raiser].Emits -notcontains $event.name) { Add-Error "Katalog: $($event.name) - komenda $raiser go nie emituje" }
        }
    }
    foreach ($name in $commands.Keys) {
        foreach ($emitted in $commands[$name].Emits) {
            if (-not $domainEvents.ContainsKey($emitted)) { Add-Error "Model: komenda $name emituje nieznane zdarzenie $emitted" }
            elseif (@($domainEvents[$emitted].raised_by) -notcontains $name) { Add-Error "Katalog: $emitted - brak $name w raised_by" }
        }
    }
    $integrationEvents = @{}
    foreach ($event in $catalog.integration_events) {
        $integrationEvents[$event.name] = $event
        Test-Status $event.status "Katalog: $($event.name)"
        if ($event.name -notmatch 'IntegrationEvent$') { Add-Error "Katalog: $($event.name) - nazwa zdarzenia integracyjnego musi kończyć się na IntegrationEvent" }
        if (-not $domainEvents.ContainsKey($event.source_event)) { Add-Error "Katalog: $($event.name) - nieznane zdarzenie źródłowe $($event.source_event)" }
        elseif ($domainEvents[$event.source_event].context -ne $event.publisher_context) { Add-Error "Katalog: $($event.name) - publikuje inny kontekst niż właściciel $($event.source_event)" }
        foreach ($subscriber in $event.subscribers) {
            if (-not $contexts.ContainsKey($subscriber.context)) { Add-Error "Katalog: $($event.name) - nieznany odbiorca '$($subscriber.context)'" }
        }
    }
    $feeds = @{}
    foreach ($feed in $catalog.read_model_feeds) {
        $feeds[$feed.read_model] = $feed
        if (-not $contexts.ContainsKey($feed.context)) { Add-Error "Katalog: zasilanie $($feed.read_model) - nieznany kontekst '$($feed.context)'" }
        foreach ($event in $feed.events) {
            if (-not $domainEvents.ContainsKey($event)) { Add-Error "Katalog: zasilanie $($feed.read_model) - nieznane zdarzenie $event" }
        }
    }
    $audited = @($catalog.domain_events | Where-Object { $_.audited } | ForEach-Object { $_.name })
    if (-not $feeds.ContainsKey('AuditLog')) { Add-Error 'Katalog: brak zasilania AuditLog' }
    elseif (-not (Test-SameSet $audited $feeds['AuditLog'].events)) { Add-Error 'Katalog: zdarzenia audited różnią się od zasilania AuditLog' }

    # ---------------------------------------------------------------- procesy
    $stepFields = 'id', 'type', 'name', 'actor', 'context', 'aggregate', 'commands', 'queries', 'emits', 'publishes', 'trigger', 'sync_calls', 'status', 'backlog', 'open_questions', 'next'
    $stepTypes = @($processes.step_types.PSObject.Properties.Name)
    $knownViews = @($queries.Keys) + @($readModels.Keys) + @($feeds.Keys)
    $diagrams = @(Get-DiagramSources -PackageRoot $packageRoot -ConfigPath (Join-Path $PSScriptRoot 'mermaid-config.json'))

    foreach ($process in $processes.processes) {
        $label = "Proces $($process.id)"
        $documentPath = Join-Path $packageRoot $process.document
        if (-not (Test-Path $documentPath)) { Add-Error "$label - brak dokumentu $($process.document)"; continue }
        $documentText = Read-Text $documentPath
        $diagramName = [System.IO.Path]::GetFileNameWithoutExtension($process.diagram)
        $diagram = $diagrams | Where-Object { $_.Name -eq $diagramName } | Select-Object -First 1
        if (-not $diagram) { Add-Error "$label - brak źródła diagramu $diagramName" }
        elseif ($diagram.Source -ne $process.document) { Add-Error "$label - diagram $diagramName jest w $($diagram.Source), a nie w $($process.document)" }
        foreach ($name in $process.bounded_contexts) { if (-not $contexts.ContainsKey($name)) { Add-Error "$label - nieznany kontekst '$name'" } }
        foreach ($name in $process.aggregates) { if (-not $aggregates.ContainsKey($name)) { Add-Error "$label - nieznany agregat '$name'" } }
        foreach ($view in $process.read_models) {
            if ($knownViews -notcontains $view.name) { Add-Error "$label - nieznany read model '$($view.name)'" }
            if ($documentText -notmatch [regex]::Escape($view.name)) { Add-Error "$label - read model $($view.name) nie występuje w dokumencie" }
        }

        $stepIds = @{}
        foreach ($step in $process.steps) {
            if ($stepIds.ContainsKey($step.id)) { Add-Error "$label - zduplikowany krok $($step.id)" }
            $stepIds[$step.id] = $step
        }
        foreach ($step in $process.steps) {
            $stepLabel = "$label, krok $($step.id)"
            $fields = @($step.PSObject.Properties.Name)
            $missing = @($stepFields | Where-Object { $fields -notcontains $_ })
            if ($missing.Count -gt 0) { Add-Error "$stepLabel - brak pól: $($missing -join ', ')" }
            if ($stepTypes -notcontains $step.type) { Add-Error "$stepLabel - nieznany typ '$($step.type)'" }
            if (@('start', 'decision', 'end') -contains $step.type) {
                if ($null -ne $step.status) { Add-Error "$stepLabel - krok typu $($step.type) nie ma stanu" }
            }
            else { Test-Status $step.status $stepLabel }
            if (-not $contexts.ContainsKey($step.context)) { Add-Error "$stepLabel - nieznany kontekst '$($step.context)'" }
            if ($step.aggregate -and $aggregates[$step.aggregate] -ne $step.context) { Add-Error "$stepLabel - agregat $($step.aggregate) nie należy do $($step.context)" }
            if ($step.type -eq 'command' -and @(Get-Names $step.commands).Count -eq 0) { Add-Error "$stepLabel - krok komendy bez komendy" }

            $stepEmits = @()
            foreach ($command in $step.commands) {
                if (-not $commands.ContainsKey($command)) { Add-Error "$stepLabel - nieznana komenda $command"; continue }
                if ($commands[$command].Context -ne $step.context) { Add-Error "$stepLabel - komenda $command należy do $($commands[$command].Context)" }
                $stepEmits += $commands[$command].Emits
            }
            foreach ($query in $step.queries) { if (-not $queries.ContainsKey($query)) { Add-Error "$stepLabel - nieznane zapytanie $query" } }
            foreach ($event in $step.emits) {
                if (-not $domainEvents.ContainsKey($event)) { Add-Error "$stepLabel - nieznane zdarzenie $event" }
                elseif (@(Get-Names $step.commands).Count -gt 0 -and $stepEmits -notcontains $event) { Add-Error "$stepLabel - zdarzenia $event nie emituje żadna komenda kroku" }
            }
            foreach ($event in $step.publishes) {
                if (-not $integrationEvents.ContainsKey($event)) { Add-Error "$stepLabel - nieznane zdarzenie integracyjne $event" }
                elseif (@($step.emits) -notcontains $integrationEvents[$event].source_event) { Add-Error "$stepLabel - $event wymaga zdarzenia $($integrationEvents[$event].source_event) w emits" }
            }
            if ($step.trigger -and -not ($domainEvents.ContainsKey($step.trigger) -or $integrationEvents.ContainsKey($step.trigger))) { Add-Error "$stepLabel - nieznany trigger $($step.trigger)" }
            foreach ($transition in $step.next) { if (-not $stepIds.ContainsKey($transition.to)) { Add-Error "$stepLabel - przejście do nieistniejącego kroku $($transition.to)" } }

            Test-RowStatus -Text $documentText -Key $step.id -Status $step.status -Label "$($process.document), krok $($step.id)"
            foreach ($name in @(Get-Names $step.commands) + @(Get-Names $step.queries) + @(Get-Names $step.emits) + @(Get-Names $step.publishes)) {
                if ($documentText -notmatch ('\b' + [regex]::Escape($name) + '\b')) { Add-Error "$($process.document) - brak $name (krok $($step.id))" }
            }
        }
    }

    # ---------------------------------------------------------------- MD <-> JSON
    $modelMd = Read-Text (Join-Path $aiRoot 'bounded_contexts_and_aggregates.md')
    foreach ($name in ($modelNames | Sort-Object -Unique)) {
        if (-not $modelMd.Contains('`' + $name + '`')) { Add-Error "bounded_contexts_and_aggregates.md - brak `"$name`"" }
    }
    foreach ($name in $commands.Keys) {
        Test-RowStatus -Text $modelMd -Key ('`' + $name + '`') -Status $commands[$name].Status -Label "bounded_contexts_and_aggregates.md, komenda $name"
    }
    $eventsMd = Read-Text (Join-Path $aiRoot 'events_catalog.md')
    foreach ($event in $catalog.domain_events) {
        Test-RowStatus -Text $eventsMd -Key ('`' + $event.name + '`') -Status $event.status -Label "events_catalog.md, zdarzenie $($event.name)"
    }
    foreach ($event in $catalog.integration_events) {
        Test-RowStatus -Text $eventsMd -Key ('`' + $event.name + '`') -Status $event.status -Label "events_catalog.md, zdarzenie $($event.name)"
    }
    foreach ($feed in $catalog.read_model_feeds) {
        $row = [regex]::Match($eventsMd, '(?m)^\|\s*`' + [regex]::Escape($feed.read_model) + '`.*$')
        if (-not $row.Success) { Add-Error "events_catalog.md - brak wiersza zasilania $($feed.read_model)"; continue }
        $listed = @([regex]::Matches(($row.Value -split '\|')[3], '`([A-Za-z]+)`') | ForEach-Object { $_.Groups[1].Value })
        if (-not (Test-SameSet $listed $feed.events)) { Add-Error "events_catalog.md - zasilanie $($feed.read_model) różni się od JSON" }
    }

    # ---------------------------------------------------------------- słownik i backlog
    $contextNames = @($contexts.Keys) + @($supportingAreas.Keys)
    $allEvents = @($domainEvents.Keys) + @($integrationEvents.Keys)
    $componentNames = @($model.bounded_contexts | ForEach-Object { $_.components } | ForEach-Object { $_.name })
    $aggregateNames = @($aggregates.Keys) + $componentNames + @($model.supporting_areas | ForEach-Object { $_.aggregates })

    $glossary = Read-Text (Join-Path $docRoot '01-slownik-jezyka-wszechobecnego.md')
    $glossaryContexts = @([regex]::Matches($glossary, '(?m)^## 1\.\d+\. (.+?)\s*$') | ForEach-Object { $_.Groups[1].Value })
    if (-not (Test-SameSet $glossaryContexts $contexts.Keys)) { Add-Error "Słownik §1 - konteksty różnią się od modelu: $($glossaryContexts -join ', ')" }
    $eventsSection = [regex]::Match($glossary, '(?ms)^# 8\. Zdarzenia domenowe\s*$(.*?)(?=^# |\z)')
    if (-not $eventsSection.Success) { Add-Error 'Słownik - brak sekcji "# 8. Zdarzenia domenowe"' }
    else {
        $glossaryEvents = @([regex]::Matches($eventsSection.Groups[1].Value, '(?m)^\|\s*`?([A-Z][A-Za-z]+)`?\s*\|') |
                ForEach-Object { $_.Groups[1].Value } | Where-Object { $_ -ne 'Zdarzenie' })
        foreach ($event in $glossaryEvents) { if (-not $domainEvents.ContainsKey($event)) { Add-Error "Słownik §8 - zdarzenie spoza modelu: $event" } }
        foreach ($event in $domainEvents.Keys) { if ($glossaryEvents -notcontains $event) { Add-Error "Słownik §8 - brak zdarzenia $event" } }
    }

    $backlog = Read-Text (Join-Path $docRoot '03-backlog-user-stories.md')
    foreach ($story in [regex]::Matches($backlog, '(?ms)^## (CRM-\d{3}) .*?(?=^## CRM-|^# |\z)')) {
        $id = $story.Groups[1].Value
        $text = $story.Value
        $contextField = [regex]::Match($text, '\*\*Kontekst DDD:\*\*\s*(.+?)\s*$', 'Multiline')
        if ($contextField.Success) {
            foreach ($name in ($contextField.Groups[1].Value -split '\s*/\s*')) { if ($contextNames -notcontains $name) { Add-Error "Backlog $id - nieznany kontekst '$name'" } }
        }
        $aggregateField = [regex]::Match($text, '\*\*Agregat:\*\*\s*(.+?)\s*$', 'Multiline')
        if ($aggregateField.Success) {
            foreach ($name in ($aggregateField.Groups[1].Value -split '\s*/\s*')) { if ($aggregateNames -notcontains $name) { Add-Error "Backlog $id - nieznany agregat '$name'" } }
        }
        foreach ($section in [regex]::Matches($text, '(?ms)^### (Komendy domenowe|Zdarzenia domenowe|Zdarzenia wejściowe)\s*$(.*?)(?=^#|\z)')) {
            foreach ($item in [regex]::Matches($section.Groups[2].Value, '(?m)^- `([A-Za-z]+)`')) {
                $name = $item.Groups[1].Value
                if ($section.Groups[1].Value -eq 'Komendy domenowe') {
                    if (-not $commands.ContainsKey($name)) { Add-Error "Backlog $id - komenda spoza modelu: $name" }
                }
                elseif ($allEvents -notcontains $name) { Add-Error "Backlog $id - zdarzenie spoza modelu: $name" }
            }
        }
    }

    # ---------------------------------------------------------------- backlog: story <-> widok zbiorczy
    $summary = @{}
    foreach ($row in [regex]::Matches($backlog, '(?m)^\|\s*(CRM-\d{3})\s*\|[^|]*\|\s*([^|]+?)\s*\|\s*(Must Have|Should Have|Could Have)\s*\|\s*([^|]+?)\s*\|\s*$')) {
        $summary[$row.Groups[1].Value] = [pscustomobject]@{ Name = $row.Groups[2].Value; Priority = $row.Groups[3].Value; Status = $row.Groups[4].Value }
    }
    foreach ($story in [regex]::Matches($backlog, '(?ms)^## (CRM-\d{3}) — (.+?)\s*$(.*?)(?=^## CRM-|^# |\z)')) {
        $id = $story.Groups[1].Value
        if (-not $summary.ContainsKey($id)) { Add-Error "Backlog $id - brak w widoku zbiorczym"; continue }
        $expected = $summary[$id]
        $body = $story.Groups[3].Value
        if ($story.Groups[2].Value -ne $expected.Name) { Add-Error "Backlog $id - tytuł różni się od widoku zbiorczego ('$($expected.Name)')" }
        $priority = [regex]::Match($body, '\*\*Priorytet:\*\*\s*(.+?)\s*$', 'Multiline').Groups[1].Value
        if ($priority -ne $expected.Priority) { Add-Error "Backlog $id - priorytet '$priority' różni się od widoku zbiorczego ('$($expected.Priority)')" }
        $status = [regex]::Match($body, '\*\*Status:\*\*\s*(.+?)\s*$', 'Multiline').Groups[1].Value
        if ($status -ne $expected.Status) { Add-Error "Backlog $id - status '$status' różni się od widoku zbiorczego ('$($expected.Status)')" }
        $checked = @([regex]::Matches($body, '(?m)^- \[x\] (.+?)\s*$') | ForEach-Object { $_.Groups[1].Value })
        if ($checked.Count -ne 1 -or $checked[0] -ne $expected.Status) { Add-Error "Backlog $id - zaznaczony status ($($checked -join ', ')) różni się od '$($expected.Status)'" }
    }

    # ---------------------------------------------------------------- plan realizacji (doc/04)
    $planPath = Join-Path $docRoot '04-plan-realizacji-pbi.md'
    if (-not (Test-Path $planPath)) { Add-Error 'Brak doc/04-plan-realizacji-pbi.md' }
    else {
        $plan = Read-Text $planPath
        $rank = @{ 'Must Have' = 3; 'Should Have' = 2; 'Could Have' = 1 }
        $planRows = @(foreach ($row in [regex]::Matches($plan, '(?m)^\|\s*(\d+)\s*\|\s*((?:CRM-\d{3}|EN-\d{2}|P-\d{2}))\s*\|(.*)$')) {
                $cells = @($row.Groups[3].Value -split '\|' | ForEach-Object { $_.Trim() })
                [pscustomobject]@{
                    Order = [int]$row.Groups[1].Value; Id = $row.Groups[2].Value; Name = $cells[0]; Type = $cells[1]
                    Priority = $cells[2]; Status = $cells[3]; Depends = $cells[4]
                }
            })
        $positions = @{}
        for ($i = 0; $i -lt $planRows.Count; $i++) {
            $row = $planRows[$i]
            if ($row.Order -ne $i + 1) { Add-Error "Plan - kolejność $($row.Order) ($($row.Id)): oczekiwano $($i + 1)" }
            if ($positions.ContainsKey($row.Id)) { Add-Error "Plan - $($row.Id) występuje więcej niż raz" }
            $positions[$row.Id] = $row
        }
        foreach ($id in $summary.Keys) { if (-not $positions.ContainsKey($id)) { Add-Error "Plan - brak story $id z backlogu" } }
        foreach ($row in $planRows) {
            $label = "Plan - $($row.Id)"
            if ($row.Id -like 'CRM-*') {
                if (-not $summary.ContainsKey($row.Id)) { Add-Error "$label - brak w backlogu"; continue }
                $expected = $summary[$row.Id]
                if ($row.Type -ne 'story') { Add-Error "$label - typ '$($row.Type)', oczekiwano 'story'" }
                if ($row.Name -ne $expected.Name) { Add-Error "$label - nazwa różni się od backlogu ('$($expected.Name)')" }
                if ($row.Priority -ne $expected.Priority) { Add-Error "$label - priorytet '$($row.Priority)', w backlogu '$($expected.Priority)'" }
                if ($row.Status -ne $expected.Status) { Add-Error "$label - status '$($row.Status)', w backlogu '$($expected.Status)'" }
            }
            else {
                $expectedType = if ($row.Id -like 'EN-*') { 'enabler' } else { 'propozycja' }
                if ($row.Type -ne $expectedType) { Add-Error "$label - typ '$($row.Type)', oczekiwano '$expectedType'" }
                if ($row.Status -ne 'Propozycja') { Add-Error "$label - status '$($row.Status)', oczekiwano 'Propozycja'" }
                if ($plan -notmatch ('(?m)^\|\s*' + [regex]::Escape($row.Id) + '\s*\|')) { Add-Error "$label - brak opisu w tabeli enablerów lub propozycji" }
            }
            foreach ($dependency in [regex]::Matches($row.Depends, '(?:CRM-\d{3}|EN-\d{2}|P-\d{2})')) {
                $dep = $dependency.Value
                if (-not $positions.ContainsKey($dep)) { Add-Error "$label - zależność $dep spoza planu"; continue }
                if ($positions[$dep].Order -ge $row.Order) { Add-Error "$label - zależność $dep jest później w kolejności" }
                $itemRank = if ($rank.ContainsKey($row.Priority)) { $rank[$row.Priority] } else { 0 }
                $depRank = if ($rank.ContainsKey($positions[$dep].Priority)) { $rank[$positions[$dep].Priority] } else { 0 }
                if ($depRank -lt $itemRank) { Add-Error "$label ($($row.Priority)) - zależy od $dep o niższym priorytecie ($($positions[$dep].Priority))" }
            }
        }
    }
}

# -------------------------------------------------------------------- pliki Markdown i JSON w doc/
$docFiles = @(Get-ChildItem -Path $docRoot -Recurse -File -Include '*.md', '*.json' |
        Where-Object { $_.FullName -notlike '*diagrams.manifest.json' })
$namingFile = Join-Path $aiRoot 'naming_decisions.md'
$forbidden = @(
    '\bOrderProcess', '\bDeal\b', '\bFollowUpTask\b', '\bBackofficeTask\b', '\bInvoiceRequest\b', '\bOrderAcceptedForFulfillment\b',
    '\bMissingInformation(Requested|Provided)\b', '\bLeadCreated\b', '\bCreateLead\b', '\bSalesOrderSubmitted\b',
    '\bOrder(SubmittedToBackoffice|ReturnedToSales|Completed|Cancelled|Blocked|Assigned|Rejected)\b', '\bOrderApprovedForInvoicing\b',
    '\bTaskStatus\b', '\bTaxId\b', '\bOpportunityStage\b', '\bMoveOpportunityToStage\b', '\bMarkOpportunityAs(Won|Lost)\b',
    '\bSubmitSalesOrder\b', '\bActivityRegistered\b', '\bNoteAdded\b', '\bAddCustomerNote\b', '\bUpdateCustomerData\b',
    '\bCustomerUpdated\b', '\bRegister(PhoneCall|Meeting|EmailActivity|Contact)\b', '\b(Complete|Reject)OrderProcess\b',
    '\bProvideMissingInfo\b', '\bRequestMissingInformation\b', 'Lead & Pipeline', 'Sales Order Capture',
    'Backoffice Order Processing', 'Reporting & Analytics', 'Integration Context', '\bSales Activity\b',
    '\bCreateSalesOrder', '\bCompleteBackofficeOrder', '\bAssignLead\b', '\bCreateOpportunity\b', '\bAddNote\b',
    '\bAssignOrderProcess\b', 'Identity and Access'
)
$openQuestions = Read-Text (Join-Path $aiRoot 'open_questions.md')
$definedIds = @([regex]::Matches($openQuestions, '(?m)^\|\s*([QT]-\d{2})\s*\|') | ForEach-Object { $_.Groups[1].Value })

function Test-ForbiddenAliases {
    param([string]$Relative, [string]$Text, [string[]]$Patterns)
    foreach ($pattern in $Patterns) {
        foreach ($match in [regex]::Matches($Text, $pattern)) {
            $line = ($Text.Substring(0, $match.Index) -split "`n").Count
            Add-Error "$Relative`:$line - zakazany alias '$($match.Value)' (ai_readable/naming_decisions.md)"
        }
    }
}

function Test-QuestionReferences {
    param([string]$Relative, [string]$Text)
    foreach ($match in [regex]::Matches($Text, '\b[QT]-\d{2}\b')) {
        if ($definedIds -notcontains $match.Value) { Add-Error "$Relative - niezdefiniowane $($match.Value) (ai_readable/open_questions.md)" }
    }
}

foreach ($file in $docFiles) {
    $relative = Get-RelativePath -Root $repoRoot -Path $file.FullName
    $text = Read-Text $file.FullName
    if ($file.FullName -ne $namingFile) { Test-ForbiddenAliases -Relative $relative -Text $text -Patterns $forbidden }
    Test-QuestionReferences -Relative $relative -Text $text
    if ($file.Extension -eq '.md') {
        foreach ($link in [regex]::Matches($text, '\]\(([^)\s]+)\)')) {
            $target = $link.Groups[1].Value
            if ($target -match '^(https?|mailto):' -or $target.StartsWith('#')) { continue }
            $path = ($target -split '#')[0]
            if (-not (Test-Path (Join-Path $file.DirectoryName $path))) { Add-Error "$relative - martwy link $target" }
        }
    }
}

# -------------------------------------------------------------------- instrukcje AI (.github, .claude, .cursor)
# Instrukcje wymieniają "Deal" celowo jako nazwę zakazaną, więc ten wzorzec jest tu pomijany.
$instructionFiles = @()
foreach ($relative in '.github/copilot-instructions.md', '.claude/CLAUDE.md') {
    $path = Join-Path $repoRoot $relative
    if (Test-Path $path) { $instructionFiles += Get-Item $path }
}
foreach ($relative in '.github/instructions', '.github/prompts', '.claude/rules', '.cursor/rules') {
    $path = Join-Path $repoRoot $relative
    if (Test-Path $path) { $instructionFiles += @(Get-ChildItem $path -Recurse -File -Include '*.md', '*.mdc') }
}
$instructionPatterns = @($forbidden | Where-Object { $_ -ne '\bDeal\b' })
foreach ($file in $instructionFiles) {
    $relative = Get-RelativePath -Root $repoRoot -Path $file.FullName
    $text = Read-Text $file.FullName
    Test-ForbiddenAliases -Relative $relative -Text $text -Patterns $instructionPatterns
    Test-QuestionReferences -Relative $relative -Text $text
}

# -------------------------------------------------------------------- diagramy
$manifestPath = Join-Path $packageRoot 'diagrams/diagrams.manifest.json'
$diagramSources = @(Get-DiagramSources -PackageRoot $packageRoot -ConfigPath (Join-Path $PSScriptRoot 'mermaid-config.json'))
$manifest = @{}
if (Test-Path $manifestPath) {
    foreach ($entry in (Read-Text $manifestPath | ConvertFrom-Json).diagrams) { $manifest[$entry.name] = $entry }
}
else { Add-Error 'Brak diagrams/diagrams.manifest.json - uruchom tools/render-diagrams.ps1' }
foreach ($diagram in $diagramSources) {
    $png = Join-Path $packageRoot "diagrams/png/$($diagram.Name).png"
    if (-not (Test-Path $png)) { Add-Error "Diagram $($diagram.Name) - brak PNG (tools/render-diagrams.ps1)" }
    elseif (-not $manifest.ContainsKey($diagram.Name) -or $manifest[$diagram.Name].sha256 -ne $diagram.Hash) { Add-Error "Diagram $($diagram.Name) - PNG nieaktualny względem $($diagram.Source) (tools/render-diagrams.ps1)" }
}
foreach ($png in Get-ChildItem (Join-Path $packageRoot 'diagrams/png') -Filter '*.png' -ErrorAction SilentlyContinue) {
    if (@($diagramSources.Name) -notcontains $png.BaseName) { Add-Error "Diagram $($png.Name) - PNG bez źródła" }
}

# -------------------------------------------------------------------- ścieżki kodu
if ($codePaths) {
    foreach ($path in ($codePaths | Sort-Object -Unique)) {
        if (-not (Test-Path (Join-Path $repoRoot $path))) { Add-Error "JSON - nieistniejąca ścieżka kodu $path" }
    }
}

if ($script:Errors.Count -gt 0) {
    foreach ($message in $script:Errors) { Write-Host "BŁĄD: $message" -ForegroundColor Red }
    Write-Host "Znaleziono błędów: $($script:Errors.Count)." -ForegroundColor Red
    exit 1
}
Write-Host "Dokumentacja spójna: $($docFiles.Count) plików w doc/, $($instructionFiles.Count) plików instrukcji AI, $($diagramSources.Count) diagramów." -ForegroundColor Green

