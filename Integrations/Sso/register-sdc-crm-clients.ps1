<#
.SYNOPSIS
    Bootstraps every SDC-CRM OIDC object in a running SimpleIdServer instance.

.DESCRIPTION
    Creates (idempotently) everything the SDC-CRM IAM needs, so nothing has to be
    clicked in the admin UI:
      * API scope + API resource `sdc-crm-api` (access tokens carry aud=sdc-crm-api),
      * public SPA client   `sdc-crm-web`    (Authorization Code + PKCE, is_public),
      * public mobile client `sdc-crm-mobile` (Authorization Code + PKCE, is_public),
      * CRM role scopes: Salesperson, SalesManager, BackofficeUser, BackofficeManager, Admin,
      * one group per CRM role ("SDC CRM <Role>") with the matching role attached,
      * test users for each CRM role (handlowiec, kierownik.sprzedazy, backoffice, 
        kierownik.backoffice, admin) with default password,
      * assignment of the administrator user to the requested role group(s), so a fresh
        token immediately carries the `role` claim.

    It authenticates with the seeded `SIDS-manager` client via client_credentials and
    calls the realm-prefixed management API (e.g. http://localhost:5001/master/...).

.EXAMPLE
    ./register-sdc-crm-clients.ps1
    ./register-sdc-crm-clients.ps1 -Authority http://localhost:5001 -Realm master
    ./register-sdc-crm-clients.ps1 -AdminUserLogin administrator -AdminUserRoles Admin,SalesManager
    ./register-sdc-crm-clients.ps1 -SkipUserAssignment
    ./register-sdc-crm-clients.ps1 -SkipTestUsers
#>

param(
    [string]$Authority = "http://localhost:5001",
    [string]$Realm = "master",
    [string]$AdminClientId = "SIDS-manager",
    [string]$AdminClientSecret = "password",
    [string[]]$WebRedirectUris = @("http://localhost:4200/", "http://localhost:4200"),
    [string]$MobileRedirectUri = "com.sdc.crm.mobile://callback",
    [string]$MobilePostLogoutRedirectUri = "com.sdc.crm.mobile://signout",
    [string]$AdminUserLogin = "administrator",
    [string[]]$AdminUserRoles = @("Admin"),
    [string]$TestUserPassword = "Test123!",
    [switch]$SkipRoles,
    [switch]$SkipGroups,
    [switch]$SkipUserAssignment,
    [switch]$SkipTestUsers
)

$ErrorActionPreference = "Stop"
$base = "$Authority/$Realm"

# Scope/type enums from the SimpleIdServer domain model.
$ScopeType_ApiResource = 1
$ScopeType_Role = 2
$Protocol_OpenId = 0
$Protocol_OAuth = 2
$ClientType_Spa = 0
$ClientType_Mobile = 3

# CRM role names - must match the backend `CrmRoles` constants exactly.
$CrmRoles = @("Salesperson", "SalesManager", "BackofficeUser", "BackofficeManager", "Admin")

# Test users - one per CRM role for development/testing.
$TestUsers = @(
    @{ Login = "handlowiec";          Email = "handlowiec@test.local";          FirstName = "Jan";    LastName = "Handlowiec";       Role = "Salesperson" }
    @{ Login = "kierownik.sprzedazy"; Email = "kierownik.sprzedazy@test.local"; FirstName = "Anna";   LastName = "Kierownik";        Role = "SalesManager" }
    @{ Login = "backoffice";          Email = "backoffice@test.local";          FirstName = "Piotr";  LastName = "Backoffice";       Role = "BackofficeUser" }
    @{ Login = "kierownik.backoffice";Email = "kierownik.backoffice@test.local";FirstName = "Maria";  LastName = "Kierownik-BO";     Role = "BackofficeManager" }
    @{ Login = "admin";               Email = "admin@test.local";               FirstName = "Tomasz"; LastName = "Administrator";    Role = "Admin" }
)

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg) { Write-Host "    OK  $msg" -ForegroundColor Green }
function Write-Skip($msg) { Write-Host "    --  $msg" -ForegroundColor DarkGray }
function Write-Warn2($msg) { Write-Host "    !!  $msg" -ForegroundColor Yellow }

function Get-GroupNameForRole([string]$role) { return "SDC CRM $role" }

# ---------------------------------------------------------------------------
# Deterministic JSON serializer.
# Windows PowerShell's ConvertTo-Json unwraps single-element arrays into scalars
# (e.g. @("code") -> "code"), which breaks the SimpleIdServer API (it expects JSON
# arrays for redirect_uris, response_types, grant_types, resources, ...). This
# serializer always emits arrays as arrays regardless of the PowerShell version.
# ---------------------------------------------------------------------------
function ConvertTo-SidJson {
    param($Value)
    if ($null -eq $Value) { return "null" }
    if ($Value -is [bool]) { if ($Value) { return "true" } else { return "false" } }
    if ($Value -is [int] -or $Value -is [long] -or $Value -is [double] -or $Value -is [decimal]) {
        return [string]$Value
    }
    if ($Value -is [string]) { return (ConvertTo-Json -InputObject $Value -Compress) }
    if ($Value -is [System.Collections.IDictionary]) {
        $props = foreach ($key in $Value.Keys) {
            $encKey = ConvertTo-Json -InputObject ([string]$key) -Compress
            "$encKey`:$(ConvertTo-SidJson $Value[$key])"
        }
        return "{" + ($props -join ",") + "}"
    }
    if ($Value -is [System.Collections.IEnumerable]) {
        $items = foreach ($item in $Value) { ConvertTo-SidJson $item }
        return "[" + ($items -join ",") + "]"
    }
    # Fallback: treat everything else as a string.
    return (ConvertTo-Json -InputObject ([string]$Value) -Compress)
}

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
        Authorization  = "Bearer $token"
        "Content-Type" = "application/json"
        Language       = "en"
    }
}

function Invoke-Sid {
    param([string]$Method, [string]$Path, $Body)
    $uri = "$base/$Path"
    if ($null -ne $Body) {
        $json = ConvertTo-SidJson $Body
        return Invoke-RestMethod -Uri $uri -Method $Method -Headers $script:Headers -Body $json
    }
    return Invoke-RestMethod -Uri $uri -Method $Method -Headers $script:Headers
}

function Invoke-Search {
    param([string]$Path, [int]$Take = 1000)
    # The management search endpoints dereference skip/take without null checks,
    # so both must always be provided.
    $resp = Invoke-Sid -Method Post -Path $Path -Body @{ skip = 0; take = $Take }
    if ($null -eq $resp) { return @() }
    if ($resp.PSObject.Properties.Name -contains "content") { return @($resp.content) }
    return @($resp)
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

# Scopes are looked up by name via .search endpoint (the GET scopes/{id} endpoint matches
# on the GUID id, not the name), which keeps every re-run idempotent.
function Get-AllScopes {
    $result = Invoke-Search -Path "scopes/.search"
    return @($result)
}

function Get-ScopeByName([string]$name) {
    return (Get-AllScopes | Where-Object { $_.name -eq $name } | Select-Object -First 1)
}

function Get-GroupByName([string]$name) {
    $groups = Invoke-Search -Path "groups/.search"
    return ($groups | Where-Object { $_.name -eq $name } | Select-Object -First 1)
}

function Get-UserByLogin([string]$login) {
    $users = Invoke-Search -Path "users/.search"
    return ($users | Where-Object { $_.name -eq $login } | Select-Object -First 1)
}

# ---------------------------------------------------------------------------

$token = Get-AdminToken
$script:Headers = New-Headers $token
$now = (Get-Date).ToUniversalTime().ToString("o")

# 1) API scope ---------------------------------------------------------------
Write-Step "Ensuring API scope 'sdc-crm-api'"
$apiScope = Get-ScopeByName "sdc-crm-api"
if ($apiScope) {
    Write-Skip "scope sdc-crm-api already exists"
}
else {
    try {
        $apiScope = Invoke-Sid -Method Post -Path "scopes" -Body @{
            name            = "sdc-crm-api"
            description     = "SDC CRM API access"
            type            = $ScopeType_ApiResource
            protocol        = $Protocol_OAuth
            is_exposed      = $true
            create_datetime = $now
            update_datetime = $now
        }
        Write-Ok "created scope sdc-crm-api"
    }
    catch {
        if ($_.Exception.Message -match "already exists" -or $_.ErrorDetails -match "already exists") {
            Write-Skip "scope sdc-crm-api already exists"
            $apiScope = Get-ScopeByName "sdc-crm-api"
        }
        else {
            throw
        }
    }
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
    Write-Skip "api resource sdc-crm-api already exists"
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
    Write-Warn2 "scope id unknown - link scope 'sdc-crm-api' to resource 'sdc-crm-api' manually"
}

# 4) CRM role scopes ---------------------------------------------------------
if (-not $SkipRoles) {
    Write-Step "Ensuring CRM role scopes"
    foreach ($role in $CrmRoles) {
        if (Get-ScopeByName $role) {
            Write-Skip "role scope $role already exists"
            continue
        }
        try {
            Invoke-Sid -Method Post -Path "scopes" -Body @{
                name            = $role
                description     = "CRM role: $role"
                type            = $ScopeType_Role
                protocol        = $Protocol_OpenId
                is_exposed      = $false
                create_datetime = $now
                update_datetime = $now
            } | Out-Null
            Write-Ok "created role scope $role"
        }
        catch {
            # Handle "already exists" error as success (idempotent)
            if ($_.Exception.Message -match "already exists" -or $_.ErrorDetails -match "already exists") {
                Write-Skip "role scope $role already exists"
            }
            else {
                throw
            }
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
        id                        = [guid]::NewGuid().ToString()
        client_id                 = "sdc-crm-web"
        client_name               = "SDC CRM Web"
        # Public client: no secret; PKCE is enforced per authorization request.
        is_public                 = $true
        client_type               = $ClientType_Spa
        redirect_uris             = $WebRedirectUris
        post_logout_redirect_uris = $WebRedirectUris
        grant_types               = @("authorization_code", "refresh_token")
        response_types            = @("code")
        scope                     = "openid profile email role offline_access sdc-crm-api"
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
        id             = [guid]::NewGuid().ToString()
        client_id      = "sdc-crm-mobile"
        client_name    = "SDC CRM Mobile"
        is_public      = $true
        client_type    = $ClientType_Mobile
        redirect_uris  = @($MobileRedirectUri)
        # Target of the end_session redirect after logout in the mobile app (system browser).
        post_logout_redirect_uris = @($MobilePostLogoutRedirectUri)
        grant_types    = @("authorization_code", "refresh_token")
        response_types = @("code")
        scope          = "openid profile email role offline_access sdc-crm-api"
    } | Out-Null
    Write-Ok "created client sdc-crm-mobile"
}

# 7) Groups + role assignment ------------------------------------------------
$roleGroups = @{}
if (-not $SkipGroups -and -not $SkipRoles) {
    Write-Step "Ensuring one group per CRM role"
    foreach ($role in $CrmRoles) {
        $groupName = Get-GroupNameForRole $role

        $group = Get-GroupByName $groupName
        if ($group) {
            Write-Skip "group '$groupName' already exists"
        }
        else {
            $group = Invoke-Sid -Method Post -Path "groups" -Body @{
                name        = $groupName
                description = "CRM role group: $role"
            }
            Write-Ok "created group '$groupName'"
        }
        $roleGroups[$role] = $group

        # Attach the role scope to the group (idempotent).
        $groupId = Get-EntityId $group
        $full = $null
        try { $full = Invoke-Sid -Method Get -Path "groups/$groupId" } catch { }
        $existingRoles = @()
        if ($full -and $full.target -and $full.target.roles) {
            $existingRoles = @($full.target.roles | ForEach-Object { $_.name })
        }
        if ($existingRoles -contains $role) {
            Write-Skip "group '$groupName' already has role $role"
        }
        else {
            Invoke-Sid -Method Post -Path "groups/$groupId/roles" -Body @{ scope = $role } | Out-Null
            Write-Ok "assigned role $role to group '$groupName'"
        }
    }
}
elseif ($SkipGroups) {
    Write-Skip "skipping group creation (-SkipGroups)"
}

# 8) Assign the admin user to the requested role group(s) --------------------
if (-not $SkipUserAssignment -and -not $SkipGroups -and -not $SkipRoles) {
    Write-Step "Assigning user '$AdminUserLogin' to role group(s): $($AdminUserRoles -join ', ')"
    $user = Get-UserByLogin $AdminUserLogin
    if (-not $user) {
        Write-Warn2 "user '$AdminUserLogin' not found - assign role groups manually in the admin panel"
    }
    else {
        $userId = Get-EntityId $user
        foreach ($role in $AdminUserRoles) {
            $groupName = Get-GroupNameForRole $role
            $group = $roleGroups[$role]
            if (-not $group) { $group = Get-GroupByName $groupName }
            if (-not $group) {
                Write-Warn2 "group '$groupName' not found - cannot assign role $role"
                continue
            }
            $groupId = Get-EntityId $group

            $fullUser = $null
            try { $fullUser = Invoke-Sid -Method Get -Path "users/$userId" } catch { }
            $alreadyMember = $false
            if ($fullUser -and $fullUser.groups) {
                foreach ($gu in $fullUser.groups) {
                    if ($gu.group -and $gu.group.id -eq $groupId) { $alreadyMember = $true; break }
                }
            }
            if ($alreadyMember) {
                Write-Skip "user '$AdminUserLogin' already in group '$groupName'"
            }
            else {
                Invoke-Sid -Method Post -Path "users/$userId/groups/$groupId" | Out-Null
                Write-Ok "added user '$AdminUserLogin' to group '$groupName'"
            }
        }
    }
}
elseif ($SkipUserAssignment) {
    Write-Skip "skipping user assignment (-SkipUserAssignment)"
}

# 9) Create test users for each CRM role -------------------------------------
if (-not $SkipTestUsers -and -not $SkipGroups -and -not $SkipRoles) {
    Write-Step "Creating test users for each CRM role (password: $TestUserPassword)"
    foreach ($testUser in $TestUsers) {
        $login = $testUser.Login
        $email = $testUser.Email
        $firstName = $testUser.FirstName
        $lastName = $testUser.LastName
        $role = $testUser.Role

        $existingUser = Get-UserByLogin $login
        if ($existingUser) {
            Write-Skip "user '$login' already exists"
            $userId = Get-EntityId $existingUser
        }
        else {
            # Create user via SimpleIdServer API with password credential
            try {
                $newUser = Invoke-Sid -Method Post -Path "users" -Body @{
                    id         = [guid]::NewGuid().ToString()
                    name       = $login
                    email      = $email
                    firstname  = $firstName
                    lastname   = $lastName
                    email_verified = $true
                    create_datetime = $now
                    update_datetime = $now
                    credentials = @(
                        @{
                            id              = [guid]::NewGuid().ToString()
                            credential_type = "pwd"
                            value           = $TestUserPassword
                            is_active       = $true
                        }
                    )
                }
                Write-Ok "created user '$login' ($email) with password"
                $userId = Get-EntityId $newUser
            }
            catch {
                Write-Warn2 "could not create user '$login': $($_.Exception.Message)"
                continue
            }
        }


        # Assign user to role group
        if ($userId) {
            $groupName = Get-GroupNameForRole $role
            $group = $roleGroups[$role]
            if (-not $group) { $group = Get-GroupByName $groupName }
            if (-not $group) {
                Write-Warn2 "group '$groupName' not found - cannot assign user '$login'"
                continue
            }
            $groupId = Get-EntityId $group

            $fullUser = $null
            try { $fullUser = Invoke-Sid -Method Get -Path "users/$userId" } catch { }
            $alreadyMember = $false
            if ($fullUser -and $fullUser.groups) {
                foreach ($gu in $fullUser.groups) {
                    if ($gu.group -and $gu.group.id -eq $groupId) { $alreadyMember = $true; break }
                }
            }
            if ($alreadyMember) {
                Write-Skip "user '$login' already in group '$groupName'"
            }
            else {
                try {
                    Invoke-Sid -Method Post -Path "users/$userId/groups/$groupId" | Out-Null
                    Write-Ok "added user '$login' to group '$groupName' (role: $role)"
                }
                catch {
                    Write-Warn2 "could not add user '$login' to group '$groupName': $($_.Exception.Message)"
                }
            }
        }
    }
}
elseif ($SkipTestUsers) {
    Write-Skip "skipping test users creation (-SkipTestUsers)"
}

Write-Host ""
Write-Host "Done. All SDC-CRM IAM objects are provisioned." -ForegroundColor Green
Write-Host ""
Write-Host "Test users created (password: $TestUserPassword):" -ForegroundColor Yellow
Write-Host "  - handlowiec           (Salesperson)" -ForegroundColor Yellow
Write-Host "  - kierownik.sprzedazy  (SalesManager)" -ForegroundColor Yellow
Write-Host "  - backoffice           (BackofficeUser)" -ForegroundColor Yellow
Write-Host "  - kierownik.backoffice (BackofficeManager)" -ForegroundColor Yellow
Write-Host "  - admin                (Admin)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Sign the user out/in so a fresh token carries the 'role' claim." -ForegroundColor Yellow
