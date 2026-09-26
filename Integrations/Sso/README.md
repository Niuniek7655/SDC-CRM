# SimpleIdServer SSO - Local Simulation for SDC-CRM

## 📋 Overview

Local SSO (Single Sign-On) environment based on **SimpleIdServer** for the SDC-CRM project.
Used to simulate IAM (Identity and Access Management) process for both the application and the administration panel.

> ℹ️ **This solution is fully self-contained** - it does not require access to any other repositories.
> All Docker images are pulled from public Docker Hub.

## 📦 Requirements

- **Docker Desktop** (Windows/Mac) or **Docker Engine** (Linux)
- **Docker Compose** (built into Docker Desktop since version 3.x)
- Minimum **2 GB RAM** for containers
- Free ports: **5001**, **5002**, **5433**

## 🐳 Docker Images Used

All images are public and available on [Docker Hub](https://hub.docker.com/u/simpleidserver):

| Image | Version | Source |
|-------|---------|--------|
| `simpleidserver/idserver` | 6.0.4 | [Docker Hub](https://hub.docker.com/r/simpleidserver/idserver) |
| `simpleidserver/website` | 6.0.4 | [Docker Hub](https://hub.docker.com/r/simpleidserver/website) |
| `postgres` | 17-alpine | [Docker Hub](https://hub.docker.com/_/postgres) |

### Components

| Service | Port | Description |
|---------|------|-------------|
| **PostgreSQL** | 5433 | Database for Identity Server |
| **IdServer** | 5001 | OAuth2/OpenID Connect authorization server |
| **IdServerWebsite** | 5002 | Administration panel for managing users, clients and permissions |

---

## 🚀 Quick Start

### 1. Start the environment

**Requirements:**
- Docker Desktop running
- Docker Compose (built into Docker Desktop)

```powershell
cd D:\Users\szymo\repo\SDC-CRM\Integrations\Sso

# Start all containers
docker compose up -d

# Check status
docker compose ps
```

> **Note:** Make sure Docker Desktop is running before executing the above commands.

### Configuration (optional `.env`)

`docker-compose.yml` works out of the box with local development defaults (ports `5001`/`5002`/`5433`,
database user `idserver`, image version `6.0.4`). To change them, copy `.env.example` to `.env`
in this directory (the file is git-ignored) and edit the values. PostgreSQL applies the database
credentials only when its volume is created, so run `docker compose down -v` after changing them.
Never reuse these development values in a production identity provider.

### 2. Wait for initialization

On first start, IdServer will automatically:
- ✅ Create tables in PostgreSQL database
- ✅ Load seed data
- ✅ Create administrator user
- ✅ Create default OAuth clients

**This process may take 30-60 seconds.**

Check logs:
```powershell
docker compose logs -f idserver
```

### 3. Access the applications

| Application | URL | Description |
|-------------|-----|-------------|
| **Identity Server** | http://localhost:5001/master | Login page and user profile |
| **Admin Panel** | http://localhost:5002/master/clients | IAM management panel |
| **OpenID Configuration** | http://localhost:5001/master/.well-known/openid-configuration | OIDC Metadata |

> ⚠️ **Important:** Admin Panel requires path with realm - use `/master/clients` instead of `/`.

### 4. Login credentials

**Default administrator account:**
- 👤 **Login:** `administrator`
- 🔑 **Password:** `password`

---

## 🔧 Environment Management

### Stop
```powershell
docker compose stop
```

### Restart
```powershell
docker compose start
```

### Remove (keeping data)
```powershell
docker compose down
```

### Remove with data
```powershell
docker compose down -v
```

### Reset from scratch
```powershell
docker compose down -v ; docker compose up -d
```

### View logs
```powershell
# All services
docker compose logs -f

# IdServer only
docker compose logs -f idserver

# Database only
docker compose logs -f sso-postgres
```

---

## 🔗 Integration with SDC-CRM (IAM)

The SDC-CRM IAM is wired as: **API = OAuth2 resource server** (validates JWT bearer
tokens), **Angular Web + MAUI Mobile = public OIDC clients** (Authorization Code +
PKCE). Both clients sign in against this SimpleIdServer and call the API with a
bearer access token.

### Required objects in SimpleIdServer

Register these once (via `register-sdc-crm-clients.ps1` or the admin panel):

| Object | Kind | Key values |
|--------|------|-----------|
| `sdc-crm-api` | API scope / resource | Audience `sdc-crm-api`, exposed |
| `sdc-crm-web` | Public SPA client | Redirect `http://localhost:4200/`, PKCE, no secret |
| `sdc-crm-mobile` | Public mobile client | Redirect `com.sdc.crm.mobile://callback`, post-logout redirect `com.sdc.crm.mobile://signout`, PKCE, no secret |
| CRM roles | Roles/groups | `Salesperson`, `SalesManager`, `BackofficeUser`, `BackofficeManager`, `Admin` |

All clients request scopes: `openid profile email role offline_access sdc-crm-api`.

### API configuration (already committed)

`Backend/src/SDC.CRM.Api/appsettings.Development.json` (other environments provide the same keys
through environment variables, e.g. `Oidc__Authority`):

```json
{
  "Oidc": {
    "Authority": "http://localhost:5001/master",
    "Audience": "sdc-crm-api",
    "RequireHttpsMetadata": false,
    "ValidateAudience": true,
    "RoleClaimType": "role",
    "AllowedCorsOrigins": ["http://localhost:4200"]
  }
}
```

> Fallback for quick local testing before the `sdc-crm-api` scope exists: set
> `"ValidateAudience": false`.

### Register the OAuth clients automatically

```powershell
cd D:\Users\szymo\repo\SDC-CRM\Integrations\Sso
./register-sdc-crm-clients.ps1
```

The script authenticates with the seeded `SIDS-manager` client and provisions
**everything** needed, so nothing has to be clicked in the admin UI:

- the API scope + API resource `sdc-crm-api` (and links them),
- the CRM role scopes (`Salesperson`, `SalesManager`, `BackofficeUser`,
  `BackofficeManager`, `Admin`),
- the two **public** clients `sdc-crm-web` (SPA) and `sdc-crm-mobile` (mobile),
  created with `is_public=true` (no secret, Authorization Code + PKCE),
- one group per role (`SDC CRM <Role>`) with the matching role attached,
- assignment of the `administrator` user to the requested role group(s).

The script is idempotent - it is safe to re-run. Existing clients are skipped, not updated: if
`sdc-crm-mobile` was registered before the mobile logout support, add the post-logout redirect URI
`com.sdc.crm.mobile://signout` manually (admin panel → Clients → `sdc-crm-mobile` → post logout redirect URIs)
or delete the client and re-run the script. Without it the identity provider rejects the end-session request
and the mobile app signs out only locally. Useful switches:

```powershell
# Give the administrator more than just the Admin role
./register-sdc-crm-clients.ps1 -AdminUserRoles Admin,SalesManager

# Assign a different user, or skip user/group provisioning entirely
./register-sdc-crm-clients.ps1 -AdminUserLogin alice
./register-sdc-crm-clients.ps1 -SkipUserAssignment
./register-sdc-crm-clients.ps1 -SkipGroups -SkipRoles
```

> The role scope names (`Salesperson`, `SalesManager`, `BackofficeUser`,
> `BackofficeManager`, `Admin`) match the backend `CrmRoles` constants exactly.

After running the script, **sign out / sign in again** so a fresh token carries
the `role` claim.

### End-to-end run order

```powershell
# 1. Identity provider (already running in your case)
cd D:\Users\szymo\repo\SDC-CRM\Integrations\Sso
./manage-sso.ps1 status
./register-sdc-crm-clients.ps1          # one-time client/scope registration

# 2. Database + backend API (resource server)  -> http://localhost:5080
cd ..\..
docker compose up -d postgres
dotnet tool restore
dotnet ef database update --project Backend/src/SDC.CRM.Infrastructure --startup-project Backend/src/SDC.CRM.Api
dotnet run --project Backend/src/SDC.CRM.Api

# 3. Angular web (public OIDC client) -> http://localhost:4200
cd Frontend\Web
npm install
npm start

# 4. MAUI mobile (public OIDC client)
#    Android emulator: map device localhost to the host first:
#      adb reverse tcp:5001 tcp:5001
#      adb reverse tcp:5080 tcp:5080
cd ..\Mobile\src\SDC.CRM.Mobile
dotnet build -t:Run -f net10.0-android
```

---

## 🏗️ User Management

### Test users (created by script)

The `register-sdc-crm-clients.ps1` script automatically creates test users for each CRM role:

| Login | Email | Role | Password |
|-------|-------|------|----------|
| `handlowiec` | handlowiec@test.local | Salesperson | `Test123!` |
| `kierownik.sprzedazy` | kierownik.sprzedazy@test.local | SalesManager | `Test123!` |
| `backoffice` | backoffice@test.local | BackofficeUser | `Test123!` |
| `kierownik.backoffice` | kierownik.backoffice@test.local | BackofficeManager | `Test123!` |
| `admin` | admin@test.local | Admin | `Test123!` |
| `administrator` | (seeded) | Admin | `password` |

To skip test user creation:
```powershell
./register-sdc-crm-clients.ps1 -SkipTestUsers
```

### Adding users manually

1. Open http://localhost:5002/master/users
2. Click **Add User**
3. Set:
   - Login
   - Email
   - Password
   - Roles (optional)

### Roles and permissions

The admin panel allows:
- Creating user groups: http://localhost:5002/master/groups
- Defining roles/scopes: http://localhost:5002/master/scopes
- Assigning permissions to scopes
- Managing claims

---

## 🔐 OAuth2/OIDC Endpoints

| Endpoint | URL |
|----------|-----|
| **Authorization** | http://localhost:5001/master/authorization |
| **Token** | http://localhost:5001/master/token |
| **UserInfo** | http://localhost:5001/master/userinfo |
| **JWKS** | http://localhost:5001/master/jwks |
| **End Session** | http://localhost:5001/master/end_session |
| **Introspection** | http://localhost:5001/master/token_info |
| **Revocation** | http://localhost:5001/master/token/revoke |

---

## 🐘 PostgreSQL Database Access

### Connection String

Default development values (see `.env.example`):

```
Host=localhost;Port=5433;Database=IdServer;Username=idserver;Password=SsoSecurePassword123!
```

### Connect via psql
```powershell
docker exec -it sso-postgres psql -U idserver -d IdServer
```

### Basic SQL commands
```sql
-- List tables
\dt

-- Check users
SELECT * FROM "Users";

-- Check OAuth clients
SELECT * FROM "Clients";

-- Exit
\q
```

### Database backup
```powershell
docker exec sso-postgres pg_dump -U idserver IdServer > sso-backup.sql
```

### Database restore
```powershell
Get-Content sso-backup.sql | docker exec -i sso-postgres psql -U idserver -d IdServer
```

---

## ⚠️ Troubleshooting

### Problem: "Connection refused" when connecting to IdServer

**Cause:** IdServer has not started yet.

**Solution:**
```powershell
# Check container status
docker compose ps

# Check logs
docker compose logs idserver
```

### Problem: "Cannot connect to PostgreSQL"

**Cause:** Database has not started yet or port 5433 is in use.

**Solution:**
```powershell
# Check if port is available
netstat -an | Select-String "5433"

# Check database logs
docker compose logs sso-postgres
```

### Problem: Admin panel cannot connect to IdServer

**Cause:** IdServer is not responding on port 5001.

**Solution:**
1. Check if IdServer is running
2. Wait for full initialization (30-60 seconds)
3. Check metadata: http://localhost:5001/.well-known/openid-configuration

### Problem: "Invalid redirect_uri"

**Cause:** Redirect URI is not registered for the client.

**Solution:**
1. Open admin panel: http://localhost:5002
2. Find your client in **Clients**
3. Add the correct Redirect URI

---

## 📚 SimpleIdServer Documentation

- [Project website](https://simpleidserver.com)
- [Documentation](https://simpleidserver.com/docs/intro)
- [GitHub](https://github.com/simpleidserver/SimpleIdServer)

---

## 🔄 Updating Images

```powershell
# Pull latest images
docker compose pull

# Restart with new images
docker compose up -d
```

---

## 📊 Ports Used in Project

| Service | Port | Conflict with main docker-compose |
|---------|------|-----------------------------------|
| SSO PostgreSQL | 5433 | ❌ None (main uses 5432) |
| IdServer | 5001 | ❌ None |
| IdServerWebsite | 5002 | ❌ None |

**Note:** Port 5432 is used by the main PostgreSQL database of the SDC-CRM project.

---

## ✅ First Run Checklist

- [ ] (Optional) Copy `.env.example` to `.env` to change ports or credentials
- [ ] Run `docker compose up -d`
- [ ] Wait 30-60 seconds for initialization
- [ ] Check http://localhost:5001/master/.well-known/openid-configuration
- [ ] Run `./register-sdc-crm-clients.ps1` (API scope, `sdc-crm-web` and `sdc-crm-mobile` clients, roles, groups, test users)
- [ ] Log in to the admin panel http://localhost:5002/master/clients to verify the clients
