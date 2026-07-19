<#
.SYNOPSIS
    Script to manage SSO environment for SDC-CRM.

.DESCRIPTION
    Allows easy starting, stopping and checking the status of
    the SimpleIdServer SSO environment.
    
    This solution is fully self-contained - all Docker images
    are pulled from public Docker Hub. It does not require access
    to any other repositories.

.EXAMPLE
    .\manage-sso.ps1 start
    .\manage-sso.ps1 stop
    .\manage-sso.ps1 status
    .\manage-sso.ps1 logs
    .\manage-sso.ps1 reset
#>

param(
    [Parameter(Position = 0)]
    [ValidateSet("start", "stop", "status", "logs", "reset", "test", "help")]
    [string]$Action = "help"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Write-ColorMessage {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

function Start-SsoEnvironment {
    Write-ColorMessage "🚀 Starting SSO environment..." "Cyan"
    Push-Location $ScriptDir
    try {
        docker compose up -d
        Write-ColorMessage "✅ Environment started!" "Green"
        Write-ColorMessage ""
        Write-ColorMessage "Wait 30-60 seconds for full initialization." "Yellow"
        Write-ColorMessage ""
        Write-ColorMessage "📍 URLs:" "Cyan"
        Write-ColorMessage "   Identity Server:  http://localhost:5001/master" "White"
        Write-ColorMessage "   Admin Panel:      http://localhost:5002/master/clients" "White"
        Write-ColorMessage ""
        Write-ColorMessage "🔑 Login credentials:" "Cyan"
        Write-ColorMessage "   Login:    administrator" "White"
        Write-ColorMessage "   Password: password" "White"
    }
    finally {
        Pop-Location
    }
}

function Stop-SsoEnvironment {
    Write-ColorMessage "⏹️ Stopping SSO environment..." "Cyan"
    Push-Location $ScriptDir
    try {
        docker compose stop
        Write-ColorMessage "✅ Environment stopped." "Green"
    }
    finally {
        Pop-Location
    }
}

function Get-SsoStatus {
    Write-ColorMessage "📊 SSO environment status:" "Cyan"
    Write-ColorMessage ""
    Push-Location $ScriptDir
    try {
        docker compose ps
        Write-ColorMessage ""
        
        # Test connections
        Write-ColorMessage "🔍 Testing connections..." "Cyan"
        
        try {
            $null = Invoke-WebRequest -Uri "http://localhost:5001/.well-known/openid-configuration" -TimeoutSec 5 -UseBasicParsing
            Write-ColorMessage "   ✅ IdServer:       http://localhost:5001 - RUNNING" "Green"
        }
        catch {
            Write-ColorMessage "   ❌ IdServer:       http://localhost:5001 - NOT RESPONDING" "Red"
        }
        
        try {
            $null = Invoke-WebRequest -Uri "http://localhost:5002/master/clients" -TimeoutSec 5 -UseBasicParsing
            Write-ColorMessage "   ✅ Admin Panel:    http://localhost:5002/master/clients - RUNNING" "Green"
        }
        catch {
            Write-ColorMessage "   ❌ Admin Panel:    http://localhost:5002/master/clients - NOT RESPONDING" "Red"
        }
    }
    finally {
        Pop-Location
    }
}

function Get-SsoLogs {
    Push-Location $ScriptDir
    try {
        docker compose logs -f --tail=100
    }
    finally {
        Pop-Location
    }
}

function Reset-SsoEnvironment {
    Write-ColorMessage "♻️ Resetting SSO environment (deleting data)..." "Yellow"
    $confirm = Read-Host "Are you sure you want to delete all data? (yes/no)"
    if ($confirm -eq "yes") {
        Push-Location $ScriptDir
        try {
            docker compose down -v
            Write-ColorMessage "✅ Data deleted." "Green"
            Write-ColorMessage "Run 'start' to initialize from scratch." "Cyan"
        }
        finally {
            Pop-Location
        }
    }
    else {
        Write-ColorMessage "❌ Cancelled." "Red"
    }
}

function Test-SsoEndpoints {
    Write-ColorMessage "🧪 Testing SSO endpoints..." "Cyan"
    
    $endpoints = @(
        @{ Name = "OpenID Configuration"; Url = "http://localhost:5001/master/.well-known/openid-configuration" }
        @{ Name = "JWKS"; Url = "http://localhost:5001/master/jwks" }
        @{ Name = "Admin Panel"; Url = "http://localhost:5002/master/clients" }
    )
    
    foreach ($endpoint in $endpoints) {
        try {
            $response = Invoke-WebRequest -Uri $endpoint.Url -TimeoutSec 5 -UseBasicParsing
            Write-ColorMessage "   ✅ $($endpoint.Name): $($response.StatusCode)" "Green"
        }
        catch {
            Write-ColorMessage "   ❌ $($endpoint.Name): Error - $($_.Exception.Message)" "Red"
        }
    }
}

function Show-Help {
    Write-ColorMessage "SimpleIdServer SSO - Environment Management" "Cyan"
    Write-ColorMessage ""
    Write-ColorMessage "This solution is fully self-contained - all Docker images" "Yellow"
    Write-ColorMessage "are pulled from public Docker Hub." "Yellow"
    Write-ColorMessage ""
    Write-ColorMessage "Usage: .\manage-sso.ps1 <action>" "White"
    Write-ColorMessage ""
    Write-ColorMessage "Available actions:" "Yellow"
    Write-ColorMessage "   start   - Start SSO environment" "White"
    Write-ColorMessage "   stop    - Stop SSO environment" "White"
    Write-ColorMessage "   status  - Check service status" "White"
    Write-ColorMessage "   logs    - View logs (Ctrl+C to exit)" "White"
    Write-ColorMessage "   reset   - Delete data and reset environment" "White"
    Write-ColorMessage "   test    - Test SSO endpoints" "White"
    Write-ColorMessage "   help    - Show this help" "White"
    Write-ColorMessage ""
    Write-ColorMessage "Requirements:" "Yellow"
    Write-ColorMessage "   - Docker Desktop running" "White"
    Write-ColorMessage "   - Free ports: 5001, 5002, 5433" "White"
}

# Main logic
switch ($Action) {
    "start"  { Start-SsoEnvironment }
    "stop"   { Stop-SsoEnvironment }
    "status" { Get-SsoStatus }
    "logs"   { Get-SsoLogs }
    "reset"  { Reset-SsoEnvironment }
    "test"   { Test-SsoEndpoints }
    "help"   { Show-Help }
}
