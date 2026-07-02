<#
.SYNOPSIS
    Registers the SDC-CRM OIDC objects in the running SimpleIdServer instance.

.DESCRIPTION
    Creates (idempotently) everything the SDC-CRM IAM needs:
      * API scope + API resource `sdc-crm-api` (so access tokens carry aud=sdc-crm-api),
      * public SPA client `sdc-crm-web`   (Authorization Code + PKCE),
      * public mobile client `sdc-crm-mobile` (Authorization Code + PKCE),
      * CRM role scopes: Salesperson, SalesManager, BackofficeUser, BackofficeManager, Admin.

    It authenticates with the seeded `SIDS-manager` client via client_credentials
    and calls the realm-prefixed management API (e.g. http://localhost:5001/master/clients).

    Assigning roles to concrete users/groups is a data decision and is left to the
    admin panel (http://localhost:5002/master) - see README.md.

.EXAMPLE
    ./register-sdc-crm-clients.ps1
    ./register-sdc-crm-clients.ps1 -Authority http://localhost:5001 -Realm master -SkipRoles
#>

param(
    [string]$Authority = "http://localhost:5001",
    [string]$Realm = "master",
    [string]$AdminClientId = "SIDS-manager",
    [string]$AdminClientSecret = "password",
    [string[]]$WebRedirectUris = @("http://localhost:4200/", "http://localhost:4200"),
    [string]$MobileRedirectUri = "com.sdc.crm.mobile://callback",
    [switch]$SkipRoles
)

$ErrorActionPreference = "Stop"
$base = "$Authority/$Realm"

# Scope/type enums from SimpleIdServer domain model.
$ScopeType_ApiResource = 1
$ScopeType_Role = 2
$Protocol_OpenId = 0
$Protocol_OAuth = 2
$ClientType_Spa = 0
$ClientType_Mobile = 3
$SecretAlg_PlainText = 0

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg) { Write-Host "    OK  $msg" -ForegroundColor Green }
function Write-Skip($msg) { Write-Host "    --  $msg" -ForegroundColor DarkGray }
function Write-Warn2($msg) { Write-Host "    !!  $msg" -ForegroundColor Yellow }

function Get-AdminToken {
    Write-Step "Requesting management token ($AdminClientId)"
    $scopes = @(
        "openid", "profile", "role",
        "clients", "scopes", "apiresources", "users", "groups", "realms"
    ) -join " "

    $body = @{
        grant_type    = "client_credentials"
        client_id     = $AdminClientId
        client_secret = $AdminClientSecret
        scope         = $scopes
    }

    $resp = Invoke-RestMethod -Uri "$base/token" -Method Post -Body $body `
        -ContentType "application/x-www-form-urlencoded"
    Write-Ok "token acquired"
    return $resp.access_token
}

function New-Headers($token) {
    return @{
        Authorization = "Bearer $token"
        "Content-Type" = "application/json"
        Language      = "en"
    }
}

function Invoke-Sid {
    param([string]$Method, [string]$Path, $Body)
    $uri = "$base/$Path"
    if ($null -ne $Body) {
        $json = ($Body | ConvertTo-Json -Depth 10)
        return Invoke-RestMethod -Uri $uri -Method $Method -Headers $script:Headers -Body $json
    }
    return Invoke-RestMethod -Uri $uri -Method $Method -Headers $script:Headers
}

function Test-Exists {
    param([string]$Path)
    try {
        Invoke-RestMethod -Uri "$base/$Path" -Method Get -Headers $script:Headers | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

function Get-EntityId($entity) {
    if ($null -eq $entity) { return $null }
    if ($entity.PSObject.Properties.Name -contains "id") { return $entity.id }
    return $null
}

# ---------------------------------------------------------------------------

$token = Get-AdminToken
$script:Headers = New-Headers $token

# 1) API scope ---------------------------------------------------------------
Write-Step "Ensuring API scope 'sdc-crm-api'"
$now = (Get-Date).ToUniversalTime().ToString("o")
$apiScope = $null
try {
    $apiScope = Invoke-Sid -Method Post -Path "scopes" -Body @{
        name        = "sdc-crm-api"
        description = "SDC CRM API access"
        type        = $ScopeType_ApiResource
        protocol    = $Protocol_OAuth
        is_exposed  = $true
        create_datetime = $now
        update_datetime = $now
    }
    Write-Ok "created scope sdc-crm-api"
}
catch {
    Write-Skip "scope sdc-crm-api probably already exists ($($_.Exception.Message))"
    try { $apiScope = Invoke-Sid -Method Get -Path "scopes/sdc-crm-api" } catch { }
}
$apiScopeId = Get-EntityId $apiScope

# 2) API resource (audience) -------------------------------------------------
Write-Step "Ensuring API resource 'sdc-crm-api' (aud=sdc-crm-api)"
try {
    Invoke-Sid -Method Post -Path "apiresources" -Body @{
        name        = "sdc-crm-api"
        aud         = "sdc-crm-api"
        description = "SDC CRM API resource"
    } | Out-Null
    Write-Ok "created api resource sdc-crm-api"
}
catch {
    Write-Skip "api resource sdc-crm-api probably already exists"
}

# 3) Link scope -> resource --------------------------------------------------
if ($apiScopeId) {
    Write-Step "Linking scope 'sdc-crm-api' to resource 'sdc-crm-api'"
    try {
        Invoke-Sid -Method Put -Path "scopes/$apiScopeId/resources" -Body @{
            resources = @("sdc-crm-api")
        } | Out-Null
        Write-Ok "scope linked to api resource"
    }
    catch {
        Write-Warn2 "could not link scope to resource: $($_.Exception.Message)"
    }
}
else {
    Write-Warn2 "scope id unknown - link scope 'sdc-crm-api' to resource 'sdc-crm-api' manually in the admin panel"
}

# 4) CRM role scopes ---------------------------------------------------------
if (-not $SkipRoles) {
    Write-Step "Ensuring CRM role scopes"
    foreach ($role in @("Salesperson", "SalesManager", "BackofficeUser", "BackofficeManager", "Admin")) {
        try {
            Invoke-Sid -Method Post -Path "scopes" -Body @{
                name        = $role
                description = "CRM role: $role"
                type        = $ScopeType_Role
                protocol    = $Protocol_OpenId
                is_exposed  = $false
                create_datetime = $now
                update_datetime = $now
            } | Out-Null
            Write-Ok "created role scope $role"
        }
        catch {
            Write-Skip "role scope $role probably already exists"
        }
    }
}

# 5) Public SPA client -------------------------------------------------------
Write-Step "Ensuring public SPA client 'sdc-crm-web'"
if (Test-Exists -Path "clients/sdc-crm-web") {
    Write-Skip "client sdc-crm-web already exists"
}
else {
    Invoke-Sid -Method Post -Path "clients" -Body @{
        client_id           = "sdc-crm-web"
        client_name         = "SDC CRM Web"
        is_public           = $true
        client_type         = $ClientType_Spa
        redirect_uris       = $WebRedirectUris
        post_logout_redirect_uris = $WebRedirectUris
        grant_types         = @("authorization_code", "refresh_token")
        response_types      = @("code")
        refresh_token_usage = 0
        scope               = "openid profile email role offline_access sdc-crm-api"
        client_secrets      = @(@{ value = [guid]::NewGuid().ToString(); alg = $SecretAlg_PlainText })
    } | Out-Null
    Write-Ok "created client sdc-crm-web"
}

# 6) Public mobile client ----------------------------------------------------
Write-Step "Ensuring public mobile client 'sdc-crm-mobile'"
if (Test-Exists -Path "clients/sdc-crm-mobile") {
    Write-Skip "client sdc-crm-mobile already exists"
}
else {
    Invoke-Sid -Method Post -Path "clients" -Body @{
        client_id           = "sdc-crm-mobile"
        client_name         = "SDC CRM Mobile"
        is_public           = $true
        client_type         = $ClientType_Mobile
        redirect_uris       = @($MobileRedirectUri)
        grant_types         = @("authorization_code", "refresh_token")
        response_types      = @("code")
        refresh_token_usage = 0
        scope               = "openid profile email role offline_access sdc-crm-api"
        client_secrets      = @(@{ value = [guid]::NewGuid().ToString(); alg = $SecretAlg_PlainText })
    } | Out-Null
    Write-Ok "created client sdc-crm-mobile"
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "Next: in the admin panel (http://localhost:5002/master) assign the CRM role" -ForegroundColor Yellow
Write-Host "scopes (Salesperson/SalesManager/.../Admin) to a group and add your user to it," -ForegroundColor Yellow
Write-Host "so the 'role' claim is issued in the access token. See README.md." -ForegroundColor Yellow
