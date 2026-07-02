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
| `sdc-crm-mobile` | Public mobile client | Redirect `com.sdc.crm.mobile://callback`, PKCE, no secret |
| CRM roles | Roles/groups | `Salesperson`, `SalesManager`, `BackofficeUser`, `BackofficeManager`, `Admin` |

All clients request scopes: `openid profile email role offline_access sdc-crm-api`.

### API configuration (already committed)

`Backend/src/SDC.CRM.Api/appsettings*.json`:

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

The script authenticates with the seeded `SIDS-manager` client and creates the
API scope, the API resource, the two public clients and the CRM role scopes. If
your environment differs, register them manually in the admin panel
(http://localhost:5002/master/clients).

### Assign CRM roles to a user (required)

The API authorizes by the `role` claim, so a user must carry CRM roles to use
protected endpoints. Roles are assigned to groups, and users belong to groups:

1. Admin panel → **Groups** → create a group (e.g. `SDC CRM Admins`).
2. Open the group → **Roles** → add the role scope(s) created by the script
   (`Admin`, `Salesperson`, ...).
3. Admin panel → **Users** → open your user (e.g. `administrator`) → **Groups** →
   add the group.
4. Sign out / sign in again so a fresh token carries the `role` claim.

> The role scope names (`Salesperson`, `SalesManager`, `BackofficeUser`,
> `BackofficeManager`, `Admin`) match the backend `CrmRoles` constants exactly.

### End-to-end run order

```powershell
# 1. Identity provider (already running in your case)
cd D:\Users\szymo\repo\SDC-CRM\Integrations\Sso
./manage-sso.ps1 status
./register-sdc-crm-clients.ps1          # one-time client/scope registration

# 2. Backend API (resource server)  -> http://localhost:5080
cd ..\..\Backend\src\SDC.CRM.Api
dotnet run

# 3. Angular web (public OIDC client) -> http://localhost:4200
cd ..\..\..\Frontend\Web
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

### Adding users

1. Open http://localhost:5002
2. Go to **Users** → **Add User**
3. Set:
   - Login
   - Email
   - Password
   - Roles (optional)

### Roles and permissions

The admin panel allows:
- Creating user groups
- Defining roles
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

- [ ] Run `docker compose up -d`
- [ ] Wait 30-60 seconds for initialization
- [ ] Check http://localhost:5001/.well-known/openid-configuration
- [ ] Log in to admin panel http://localhost:5002
- [ ] Create a new OAuth client for SDC-CRM application
- [ ] Configure SDC-CRM application with OAuth client data
