# Console Daemon - TodoList API Client

## ?? Overview

This is a .NET 8 console application that demonstrates the **Client Credentials flow** (daemon/service authentication) to call the TodoList API using **Application Permissions** (roles).

### Key Concepts

- **Client Credentials Flow**: Authentication without a user (service-to-service)
- **Application Permissions**: Uses `roles` claim instead of `scp` (scopes)
- **Daemon Application**: Runs unattended, no user interaction
- **TodoList API Integration**: Calls GET /api/TodoList endpoint

---

## ??? Architecture

```
????????????????      ???????????????      ????????????????
? Console App  ????????  Azure AD   ???????? TodoList API ?
?  (Daemon)    ?      ?             ?      ?              ?
????????????????      ???????????????      ????????????????
       ?                      ?                     ?
   Client ID          Client Credentials       App Permission
   Client Secret       (App Token)          (ToDoList.Read.All)
```

---

## ?? Azure AD Configuration

### Step 1: Register the Application

1. **Azure Portal** ? **Azure AD** ? **App registrations** ? **New registration**

2. **Name**: `TodoList-Console-Daemon`

3. **Supported account types**: Accounts in this organizational directory only

4. **Redirect URI**: Leave blank (not needed for daemon apps)

5. Click **Register**

6. **Copy the Client ID and Tenant ID**

### Step 2: Create Client Secret

1. Go to **Certificates & secrets** ? **New client secret**

2. **Description**: `Console-Daemon-Secret`

3. **Expires**: 6 months (or as per policy)

4. Click **Add**

5. **COPY THE SECRET VALUE IMMEDIATELY** - it won't be shown again

### Step 3: Configure API Permissions

1. Go to **API permissions** ? **Add a permission**

2. Click **My APIs**

3. Select **TodoList API** (`ciam-msal-dotnet-api`)

4. Select **Application permissions** (NOT Delegated permissions)

5. Select `ToDoList.Read.All` or `ToDoList.ReadWrite.All`

6. Click **Add permissions**

7. **IMPORTANT**: Click **Grant admin consent for [tenant]** ?

---

## ?? Configuration

### Update `appsettings.json`

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_DAEMON_APP_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET"
  },
  "ToDoListApi": {
    "BaseUrl": "https://localhost:44351/api",
    "Scopes": ["api://TODOLIST_API_CLIENT_ID/.default"]
  }
}
```

**Replace:**
- `YOUR_TENANT_ID`: From Azure Portal
- `YOUR_DAEMON_APP_CLIENT_ID`: From Step 1
- `YOUR_CLIENT_SECRET`: From Step 2
- `TODOLIST_API_CLIENT_ID`: The Client ID of the TodoList API

**Scope Format:**
- Use `api://TODOLIST_API_CLIENT_ID/.default` for application permissions
- The `.default` scope requests all app permissions granted to the app

---

## ?? Running the Application

### Build

```bash
cd console-daemon\ConsoleApp
dotnet build
```

### Run

```bash
dotnet run
```

### Expected Output

```
==============================================
Console Daemon - TodoList API Client
Using Client Credentials Flow (App Permissions)
==============================================

?? Acquiring access token...
   Authority: https://login.microsoftonline.com/50bd1379-...
   Client ID: abcd1234-...
   Scopes: api://1b782430-.../. default

? Access token acquired successfully!
Token (first 50 chars): eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik...

?? Calling TodoList API...
   Endpoint: https://localhost:44351/api/ToDoList

?? Response Status: 200 OK

? Success! TodoList API Response:
==============================================
[
  {
    "id": 1,
    "owner": "9ee74d6e-3398-40a1-943b-72289fe14505",
    "description": "Sample todo item"
  }
]
==============================================

Press any key to exit...
```

---

## ?? How It Works

### 1. **Client Credentials Flow**

```csharp
var app = ConfidentialClientApplicationBuilder.Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(new Uri(authority))
    .Build();

var result = await app.AcquireTokenForClient(scopes)
    .ExecuteAsync();
```

- Uses **client ID + client secret** (not user credentials)
- Returns an **app token** (not a user token)
- Token contains `roles` claim (not `scp`)

### 2. **Token Characteristics**

**App Token (Client Credentials):**
```json
{
  "aud": "api://1b782430-a646-4a1b-ada6-44f897ccd4e6",
  "iss": "https://login.microsoftonline.com/.../v2.0",
  "roles": ["ToDoList.Read.All"],  // ? Application Permission
  "sub": "...",  // Service Principal ID (not user)
  "tid": "50bd1379-08e1-4e5a-92c9-56baf968402f"
}
```

**User Token (for comparison):**
```json
{
  "aud": "api://1b782430-a646-4a1b-ada6-44f897ccd4e6",
  "scp": "ToDoList.Read",  // ? Delegated Permission
  "oid": "user-object-id",  // ? User ID
  "sub": "..."
}
```

### 3. **API Validation**

The TodoList API validates using `RequiredScopeOrAppPermission`:

```csharp
[RequiredScopeOrAppPermission(
    RequiredScopesConfigurationKey = "AzureAd:Scopes:ToDoList:Read",  // User: ToDoList.Read
    RequiredAppPermissionsConfigurationKey = "AzureAd:AppPermissions:ToDoList:Read"  // App: ToDoList.Read.All
)]
```

- **User token**: Checks `scp` claim for `ToDoList.Read`
- **App token**: Checks `roles` claim for `ToDoList.Read.All`

---

## ?? Troubleshooting

### Error: "AADSTS7000215: Invalid client secret"

**Cause**: Secret is expired or incorrect  
**Solution**: Create a new secret in Azure Portal and update `appsettings.json`

### Error: "AADSTS650053: The application requires access to a service"

**Cause**: API permissions not configured or admin consent not granted  
**Solution**: 
1. Go to Azure Portal ? App registrations ? Your daemon app
2. API permissions ? Verify `ToDoList.Read.All` is added
3. Click "Grant admin consent"

### Error: "401 Unauthorized" from TodoList API

**Cause**: Token doesn't have required app permission  
**Solution**:
1. Verify `roles` claim in token (decode at jwt.ms)
2. Should contain `ToDoList.Read.All` or `ToDoList.ReadWrite.All`
3. Verify API's `appsettings.json` has correct `AppPermissions` configuration

### Error: "The remote certificate is invalid"

**Cause**: HTTPS certificate issue (common with localhost)  
**Solution**: 
```csharp
// Add to Program.cs (development only!)
ServicePointManager.ServerCertificateValidationCallback += 
    (sender, cert, chain, sslPolicyErrors) => true;
```

### Error: "Configuration is incomplete"

**Cause**: Missing values in `appsettings.json`  
**Solution**: Ensure all placeholders are replaced with actual values

---

## ?? Comparison: User vs App Permissions

| Aspect | User (Delegated) | App (Application) |
|--------|------------------|-------------------|
| **Authentication** | User login required | No user, uses client credentials |
| **Token Claim** | `scp` (scope) | `roles` (role) |
| **Permission Type** | Delegated | Application |
| **Use Case** | Interactive apps (SPA, Web) | Daemon, background services |
| **Consent** | User or admin | Admin only |
| **Example Scope** | `ToDoList.Read` | `ToDoList.Read.All` |

---

## ?? Files in This Project

| File | Purpose |
|------|---------|
| `Program.cs` | Main application logic |
| `appsettings.json` | Configuration (Azure AD, API settings) |
| `ConsoleApp.csproj` | Project file with dependencies |
| `README.md` | This documentation |

---

## ?? Next Steps

1. ? **Configure Azure AD** (follow steps above)
2. ? **Update appsettings.json** with real values
3. ? **Run the application**
4. ?? **Extend functionality**:
   - Add POST endpoint to create todos
   - Add PUT/DELETE for full CRUD
   - Add error handling and retry logic
   - Implement logging

---

## ?? References

- [Microsoft Docs - Client Credentials Flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)
- [Microsoft.Identity.Client Documentation](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis)
- [Application Permissions vs Delegated Permissions](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent)

---

**Created**: 2025-01-20  
**Version**: 1.0  
**Target Framework**: .NET 8.0  
**.NET SDK Required**: 8.0 or later
