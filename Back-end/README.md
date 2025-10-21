# MVP Conf 2025 - Microsoft Entra External ID Flows

## ?? Solution Structure

This solution demonstrates various authentication and authorization flows using Microsoft Entra External ID with a React SPA, ASP.NET Core Web API, and Console Daemon application.

### Projects in Solution

```
mvpconf-entraflows.sln
??? API/
?   ??? ToDoListAPI/                    # ASP.NET Core Web API
?   ?   ??? ToDoListAPI.csproj
?   ??? ToDoListAPI.Tests/              # Unit Tests
?       ??? ToDoListAPI.Tests.csproj
??? console-daemon/
?   ??? ConsoleApp/                     # Console Daemon (Client Credentials)
?       ??? ConsoleApp.csproj
??? SPA/                                # React Single Page Application
    ??? package.json                    # (Not in .sln, managed by npm)
```

---

## ?? Projects Overview

### 1. **ToDoListAPI** (.NET 8 Web API)
**Location:** `API/TodoListAPI/`

Protected Web API that demonstrates:
- ? **JWT Bearer Token Authentication**
- ? **Scope-based Authorization** (ToDoList.Read, ToDoList.ReadWrite)
- ? **App Permission Authorization** (ToDoList.Read.All, ToDoList.ReadWrite.All)
- ? **On-Behalf-Of (OBO) Flow** to call Microsoft Graph
- ? **Swagger UI with OAuth2** integration
- ? **CRUD operations** on TodoList items

**Endpoints:**
- `GET /api/ToDoList` - Get all todos
- `GET /api/ToDoList/{id}` - Get todo by ID
- `POST /api/ToDoList` - Create new todo
- `PUT /api/ToDoList/{id}` - Update todo
- `DELETE /api/ToDoList/{id}` - Delete todo
- `GET /api/UserProfile/me` - Get user profile from Graph (OBO)

**Runs on:** `https://localhost:44351`

---

### 2. **ToDoListAPI.Tests** (.NET 8 Test Project)
**Location:** `API/TodoListAPI.Tests/`

Unit tests for the ToDoListAPI:
- ? Configuration validation tests
- ? Controller tests
- ? Authentication tests

**Run tests:**
```bash
dotnet test
```

---

### 3. **ConsoleApp** (.NET 8 Console)
**Location:** `console-daemon/ConsoleApp/`

Daemon/Service application that demonstrates:
- ? **Client Credentials Flow** (no user interaction)
- ? **Application Permissions** (uses `roles` claim)
- ? **Service-to-Service Authentication**
- ? Calling protected API with app token

**Features:**
- Acquires app token using client credentials
- Calls ToDoListAPI GET endpoint
- Pretty-prints JSON responses

**Run:**
```bash
cd console-daemon/ConsoleApp
dotnet run
```

---

### 4. **SPA** (React Application)
**Location:** `SPA/`

React Single Page Application that demonstrates:
- ? **User Authentication** with MSAL React
- ? **Authorization Code Flow with PKCE**
- ? **Delegated Permissions** (ToDoList.Read, ToDoList.ReadWrite)
- ? **Token Acquisition** for API calls
- ? **Interactive UI** for CRUD operations

**Runs on:** `http://localhost:3000`

**Run:**
```bash
cd SPA
npm install
npm start
```

---

## ?? Quick Start

### Build the Solution
```bash
# From solution root
dotnet build mvpconf-entraflows.sln
```

### Run Individual Projects

#### Start the API
```bash
cd API/TodoListAPI
dotnet run
```

#### Start the SPA
```bash
cd SPA
npm install
npm start
```

#### Run the Console Daemon
```bash
cd console-daemon/ConsoleApp
dotnet run
```

---

## ?? Authentication Flows Demonstrated

### 1. **Authorization Code Flow with PKCE** (SPA ? API)
```
User ? SPA (React) ? Azure AD (login) ? SPA (token) ? API
```
- **Token Type:** User token (delegated permissions)
- **Claims:** `scp` (scopes), `oid` (user ID)
- **Use Case:** Interactive user applications

### 2. **On-Behalf-Of (OBO) Flow** (API ? Graph)
```
SPA ? API (user token) ? Azure AD (OBO) ? API (Graph token) ? Microsoft Graph
```
- **Token Type:** User token exchanged for Graph token
- **Claims:** `scp` with Graph scopes
- **Use Case:** Middle-tier API calling downstream APIs

### 3. **Client Credentials Flow** (Daemon ? API)
```
Console App ? Azure AD (client credentials) ? Console App (app token) ? API
```
- **Token Type:** App token (application permissions)
- **Claims:** `roles` (app roles), no user context
- **Use Case:** Background services, scheduled jobs

---

## ?? Permission Types

| Permission Type | Who Uses | Token Claim | Example | Project |
|----------------|----------|-------------|---------|---------|
| **Delegated** | User apps | `scp` | `ToDoList.Read` | SPA |
| **Application** | Daemon apps | `roles` | `ToDoList.Read.All` | ConsoleApp |

---

## ??? Architecture Diagram

```
???????????????
?   Browser   ?
?   (User)    ?
???????????????
       ? 1. Login
       ?
???????????????????     2. Token     ????????????????
?   React SPA     ????????????????????  Azure AD    ?
?  (Port 3000)    ?                  ?  (Entra ID)  ?
???????????????????                  ????????????????
         ? 3. API Call                       ?
         ? (Bearer Token)                    ? 5. OBO Token
         ?                                   ?
????????????????????                         ?
?  ToDoListAPI     ???????????????????????????
?  (Port 44351)    ?     6. Graph Token
????????????????????
         ? 7. API Call
         ?
????????????????????
? Microsoft Graph  ?
?      API         ?
????????????????????

???????????????????     8. Client Credentials    ????????????????
?  Console Daemon ????????????????????????????????  Azure AD    ?
?    (Service)    ????????????????????????????????              ?
???????????????????     9. App Token             ????????????????
         ? 10. API Call
         ? (App Token)
         ?
????????????????????
?  ToDoListAPI     ?
?  (Port 44351)    ?
????????????????????
```

---

## ?? Key Concepts

### Scopes vs Roles

**Scopes (Delegated Permissions):**
- Used when a user is present
- Token contains `scp` claim
- Example: `ToDoList.Read`
- Format: `api://{clientId}/ToDoList.Read`

**Roles (Application Permissions):**
- Used for daemon/service apps
- Token contains `roles` claim
- Example: `ToDoList.Read.All`
- Requires admin consent

### Token Types

**ID Token:**
- Contains user identity claims
- Used for authentication
- Not sent to API

**Access Token:**
- Contains permissions (scp or roles)
- Used for authorization
- Sent as Bearer token to API

---

## ??? Development Workflow

### 1. Configure Azure AD
- Register applications
- Configure permissions
- Grant admin consent
- Create client secrets

### 2. Update Configuration Files
**API:**
- `API/TodoListAPI/appsettings.json`
- Use User Secrets for sensitive data

**SPA:**
- `SPA/src/authConfig.js`

**Console:**
- `console-daemon/ConsoleApp/appsettings.json`

### 3. Run Applications
```bash
# Terminal 1: API
cd API/TodoListAPI
dotnet run

# Terminal 2: SPA
cd SPA
npm start

# Terminal 3 (optional): Console Daemon
cd console-daemon/ConsoleApp
dotnet run
```

---

## ?? Testing

### Run Unit Tests
```bash
dotnet test
```

### Test Endpoints

**Via Swagger UI:**
```
https://localhost:44351/swagger
```

**Via SPA:**
```
http://localhost:3000
```

**Via Console Daemon:**
```bash
cd console-daemon/ConsoleApp
dotnet run
```

---

## ?? Configuration Files

### API Configuration
```json
// API/TodoListAPI/appsettings.json
{
  "AzureAd": {
    "Instance": "https://subdomain.ciamlogin.com/",
    "TenantId": "...",
    "ClientId": "...",
    "ClientSecret": "..." // Use User Secrets!
  }
}
```

### SPA Configuration
```javascript
// SPA/src/authConfig.js
export const msalConfig = {
  auth: {
    clientId: "...",
    authority: "https://subdomain.ciamlogin.com/...",
    redirectUri: "http://localhost:3000"
  }
}
```

### Console Configuration
```json
// console-daemon/ConsoleApp/appsettings.json
{
  "AzureAd": {
    "TenantId": "...",
    "ClientId": "...",
    "ClientSecret": "..."
  },
  "ToDoListApi": {
    "Scopes": ["api://{api-client-id}/.default"]
  }
}
```

---

## ?? Troubleshooting

### API won't start
- Check if port 44351 is already in use
- Verify configuration in `appsettings.json`
- Check User Secrets are configured

### SPA authentication fails
- Verify `authConfig.js` has correct values
- Check redirect URI in Azure AD
- Clear browser cache/cookies

### Console app returns 401
- Verify admin consent was granted
- Check app permissions in Azure AD
- Verify API is running

### OBO flow fails
- Verify API has client secret configured
- Check Graph API permissions
- Verify `User.Read` permission exists

---

## ?? Documentation

| Document | Location | Description |
|----------|----------|-------------|
| API README | `API/TodoListAPI/README.md` | API setup and configuration |
| Console README | `console-daemon/ConsoleApp/README.md` | Daemon app guide |
| Console Quick Start | `console-daemon/ConsoleApp/QUICKSTART.md` | 5-minute setup |
| Instructions | `instructions.md` | Original tutorial instructions |

---

## ?? Learning Resources

- [Microsoft Entra External ID Documentation](https://learn.microsoft.com/entra/external-id/)
- [MSAL React Tutorial](https://github.com/AzureAD/microsoft-authentication-library-for-js/tree/dev/lib/msal-react)
- [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web)
- [OAuth 2.0 Flows](https://oauth.net/2/)

---

## ?? Contributing

This is a tutorial/demo project for MVP Conf 2025.

---

## ?? License

Educational use for MVP Conf 2025.

---

**Created for:** MVP Conf 2025  
**Topics:** Microsoft Entra External ID, OAuth 2.0, MSAL, ASP.NET Core  
**Version:** 1.0  
**.NET Version:** 8.0  
**React Version:** 18+
